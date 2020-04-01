﻿using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CDN.NET.Backend.Dtos.AuthDtos;
using CDN.NET.Backend.Helpers;
using CDN.NET.Backend.Repositories.Interfaces;
using CDN.NET.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CDN.NET.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly IFileRepository _fileRepo;
        private readonly IMapper _mapper;
        private readonly IUtilsService _utilsService;
        private readonly IAlbumRepository _albumRepo;
        private readonly IApiKeyRepository _apiKeyRepo;


        public UserController(IUserRepository userRepo, IFileRepository fileRepo, IMapper mapper, 
            IUtilsService utilsService, IAlbumRepository albumRepo, IApiKeyRepository apiKeyRepo)
        {
            _userRepo = userRepo;
            _fileRepo = fileRepo;
            _mapper = mapper;
            _utilsService = utilsService;
            _albumRepo = albumRepo;
            _apiKeyRepo = apiKeyRepo;
        }

        [HttpGet("users")]
        public async Task<ActionResult<List<UserDetailDto>>> GetAllUsers()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (await _userRepo.IsAdmin(userId) == false)
            {
                return Unauthorized();
            }
            //var user = _mapper.Map<UserDetailDto>(userFromRepo);
            var users = await _userRepo.GetAllUser();
            var usersToReturn = _mapper.Map<List<UserDetailDto>>(users);

            return usersToReturn;
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
            // Remove albums
            foreach (var album in user.Albums)
            {
                _albumRepo.Delete(album);
            }
            await _albumRepo.SaveAll();

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
            
            // Remove ApiKey
            await _apiKeyRepo.RemoveApiKeyByUserId(user.Id);
            
            // remove user
            await _userRepo.RemoveUser(user);

            return Ok();
        }
        
    }
}