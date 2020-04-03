using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CDN.NET.Backend.Dtos.AuthDtos;
using CDN.NET.Backend.Dtos.UserDtos;
using CDN.NET.Backend.Helpers;
using CDN.NET.Backend.Repositories.Interfaces;
using CDN.NET.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CDN.NET.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly IFileRepository _fileRepo;
        private readonly IMapper _mapper;
        private readonly IUtilsService _utilsService;
        private readonly IAlbumRepository _albumRepo;
        private readonly IApiKeyRepository _apiKeyRepo;
        private readonly IAuthRepository _authRepo;


        public UserController(IUserRepository userRepo, IFileRepository fileRepo, IMapper mapper, 
            IUtilsService utilsService, IAlbumRepository albumRepo, IApiKeyRepository apiKeyRepo, IAuthRepository authRepo)
        {
            _userRepo = userRepo;
            _fileRepo = fileRepo;
            _mapper = mapper;
            _utilsService = utilsService;
            _albumRepo = albumRepo;
            _apiKeyRepo = apiKeyRepo;
            _authRepo = authRepo;
        }

        [HttpGet("users")]
        public async Task<ActionResult<List<UserAdminReturnDto>>> GetAllUsers()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (await _userRepo.IsAdmin(userId) == false)
            {
                return Unauthorized();
            }
            //var user = _mapper.Map<UserDetailDto>(userFromRepo);
            var users = await _userRepo.GetAllUser();
            var usersToReturn = _mapper.Map<List<UserAdminReturnDto>>(users);

            return usersToReturn;
        }

        [HttpPut("user/{userId}")]
        public async Task<ActionResult<UserDetailDto>> AdminUpdateUser(int userId, UserAdminEditDto editInfo)
        {
            int reqUserId = this.GetRequestUserId();
            if (await _userRepo.IsAdmin(reqUserId) == false)
            {
                return Unauthorized();
            }
            
            // Check if User exists
            var user = await _userRepo.GetUserById(userId);
            if (user == null)
            {
                return NotFound();
            }
            
            // Check if username is unique if one is passed
            if (!string.IsNullOrWhiteSpace(editInfo.Username))
            {
                if (await _authRepo.UserExistsByUsername(editInfo.Username))
                {
                    return BadRequest("Username already exists");
                }
            }
            
            // Update user info
            var userToRet = await _userRepo.UpdateUserInfo(user, editInfo);
            var ret = _mapper.Map<UserDetailDto>(userToRet);

            return Ok(ret);
        }

        [HttpDelete("user/{userId}")]
        public async Task<IActionResult> AdminDeleteUser(int userId)
        {
            int reqUserId = this.GetRequestUserId();
            if (await _userRepo.IsAdmin(reqUserId) == false)
            {
                return Unauthorized();
            }
            // get user
            var user = await _userRepo.GetUserById(userId);
            if (user == null)
            {
                return NotFound();
            }
            
            // Remove all files
            foreach (var file in user.Files)
            {
                string path = _utilsService.GenerateFilePath(file.PublicId, file.FileExtension);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                _fileRepo.Delete(file);
            }
            await _fileRepo.SaveAll();
            
            // Remove albums
            foreach (var album in user.Albums)
            {
                _albumRepo.Delete(album);
            }
            await _albumRepo.SaveAll();
            
            // Remove ApiKey
            await _apiKeyRepo.RemoveApiKeyByUserId(user.Id);
            
            // remove user
            await _userRepo.RemoveUser(user);

            return Ok();
        }
        
    }
}