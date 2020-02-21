using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CDN.NET.Backend.Dtos.AuthDtos;
using CDN.NET.Backend.Helpers;
using CDN.NET.Backend.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CDN.NET.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepo;
        private readonly IMapper _mapper;
        private readonly Constants _constants;
        private readonly IApiKeyRepository _keyRepo;
        private readonly IUserRepository _userRepository;
        private readonly IOptions<AppSettings> _appSettings;

        public AuthController(IAuthRepository authRepo, IMapper mapper, Constants constants,
            IApiKeyRepository keyRepo, IUserRepository userRepository, IOptions<AppSettings> appSettings)
        {
            _authRepo = authRepo;
            _mapper = mapper;
            _constants = constants;
            _keyRepo = keyRepo;
            _userRepository = userRepository;
            _appSettings = appSettings;
        }

        [Authorize]
        [HttpGet("test")]
        public IActionResult TestAuthentication()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            return Ok(new {message = $"You are identified with ID {userId}"});
        }

        [Authorize]
        [HttpGet("apikey")]
        public async Task<ActionResult<string>> GetApiKey()
        {
            // get the userId from the api key
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userFromRepo = await _userRepository.GetUserById(userId);
            if (userFromRepo == null)
                return BadRequest();

            var key = _keyRepo.CreateApiKey(userFromRepo);
            if (userFromRepo.ApiKey == null)
            {
                userFromRepo.ApiKey = key;
            }
            else
            {
                userFromRepo.ApiKey.Key = key.Key;
            }
            if (!await _userRepository.SaveAll())
                return BadRequest("Couldn't save api key'");

            return Ok(key.Key);
        }

        [Authorize]
        [HttpDelete("apikey")]
        public async Task<IActionResult> RemoveApiKey()
        {
            // get the userId from the api key
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userFromRepo = await _userRepository.GetUserById(userId);
            if (userFromRepo == null)
                return BadRequest();
            
            _keyRepo.RemoveApiKey(userFromRepo.ApiKey);
            if (!await _keyRepo.SaveAll())
                return BadRequest("Failed to remove key");

            return Ok();
        }
        
        [HttpPost("register")]
        public async Task<ActionResult<UserDetailDto>> Register(UserForRegisterDto userForRegisterDto)
        {
            if (_appSettings.Value.Private)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new {message = "CDN is in private mode, registration is not allowed"});
            }
            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();
            if (await _authRepo.UserExistsByUsername(userForRegisterDto.Username))
                return BadRequest("Username already exists");

            var createdUser = await _authRepo.Register(userForRegisterDto.Username, userForRegisterDto.Password);
            var userToReturn = _mapper.Map<UserDetailDto>(createdUser);
            // TODO change to created at route
            //return CreatedAtRoute("GetUser", new {controller = "Users", id = createdUser.Id}, userToReturn);
            return Ok(userToReturn);
        }
        
        [HttpPost("login")]
        public async Task<ActionResult<LoginReturnDto>> Login(UserForLoginDto userForLoginDto)
        {
            var userFromRepo = await _authRepo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);
            // this means either the username wasnt found
            // or the password hash wasn't correct
            if (userFromRepo == null)
                return Unauthorized();

            // Additional info to add to the JWT
            // ATTENTION this is public information
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username),
            };

            var key = _constants.LazyGetJwtKey();
            
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(_appSettings.Value.DaysUntilTokenExpiration),
                NotBefore = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)),
                SigningCredentials = creds,
                Issuer = Constants.TOKEN_ISSUER
            };
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var user = _mapper.Map<UserDetailDto>(userFromRepo);

            return Ok(new LoginReturnDto()
            {
                Token = tokenHandler.WriteToken(token),
                User = user
            });

        }
    }
}