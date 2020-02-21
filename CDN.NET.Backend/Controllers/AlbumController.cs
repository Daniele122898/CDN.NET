using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CDN.NET.Backend.Dtos.AlbumDtos;
using CDN.NET.Backend.Helpers;
using CDN.NET.Backend.Models;
using CDN.NET.Backend.Repositories.Interfaces;
using CDN.NET.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CDN.NET.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlbumController : ControllerBase
    {
        private readonly IOptions<LimitSettings> _limitSettings;
        private readonly IAlbumRepository _albumRepo;
        private readonly IMapper _mapper;
        private readonly IUtilsService _utilsService;
        private readonly IFileRepository _fileRepo;

        public AlbumController(IOptions<LimitSettings> limitSettings,
            IAlbumRepository albumRepo, IMapper mapper, IUtilsService utilsService,
            IFileRepository fileRepo)
        {
            _limitSettings = limitSettings;
            _albumRepo = albumRepo;
            _mapper = mapper;
            _utilsService = utilsService;
            _fileRepo = fileRepo;
        }
        
        /// <summary>
        /// Get all albums of a user without the files
        /// </summary>
        /// <returns></returns>
        [HttpGet("getAllSparse")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<AlbumsToReturnSparseDto>>> GetAllAlbumsSparse()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var albums = await _albumRepo.GetAllAlbumsFromUser(userId);

            var albumsToReturn = _mapper.Map<IEnumerable<AlbumsToReturnSparseDto>>(albums);
            return Ok(albumsToReturn);
        }

        [HttpGet("getAll")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<AlbumToReturnDto>>> GetAllAlbums()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var albums = await _albumRepo.GetAllAlbumsFromUser(userId);

            var albumsToReturn = _mapper.Map<IEnumerable<AlbumToReturnDto>>(albums);

            return Ok(albumsToReturn);
        }
        
        [HttpGet("private/{albumId}", Name = "GetAlbumPrivate")]
        [Authorize]
        public async Task<ActionResult<AlbumToReturnDto>> GetAlbumPrivate(int albumId)
        {
            var album = await _albumRepo.GetAlbumById(albumId);
            if (album == null)
            {
                return NotFound("Album doesn't exist");
            }
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (album.OwnerId != userId)
            {
                return Unauthorized("This is not your album");
            }
            
            var albumToReturn = _mapper.Map<AlbumToReturnDto>(album);

            return albumToReturn;
        }

        [HttpGet("{albumId}", Name = "GetAlbum")]
        public async Task<ActionResult<AlbumToReturnDto>> GetAlbum(int albumId)
        {
            var album = await _albumRepo.GetAlbumById(albumId);
            if (album == null)
            {
                return NotFound("Album doesn't exist");
            }
            
            if (!album.IsPublic)
            {
                return Unauthorized("This album is not public, use the private endpoint for this");
            }

            var albumToReturn = _mapper.Map<AlbumToReturnDto>(album);

            return albumToReturn;
        }

        [HttpDelete("{albumId}")]
        [Authorize]
        public async Task<IActionResult> DeleteAlbum(int albumId, [FromQuery] bool removeFiles = false)
        {
            var album = await _albumRepo.GetAlbumById(albumId);
            if (album == null)
            {
                return NotFound("Album with that Id was not found");
            }

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (album.OwnerId != userId)
            {
                return Unauthorized("You dont own this album");
            }

            if (album.UFiles.Count != 0)
            {
                // remove files if flag is set
                foreach (var file in album.UFiles)
                {
                    if (removeFiles)
                    {
                        _fileRepo.Delete(file);
                        string path = _utilsService.GenerateFilePath(file.PublicId, file.FileExtension);
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }
                    }
                    else
                    {
                        file.AlbumId = null;
                    }
                }

                if (!await _fileRepo.SaveAll())
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new {message = "Failed saving to database"});
                }
            }

            _albumRepo.Delete(album);

            if (!await _albumRepo.SaveAll())
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new {message = "Failed saving to database"});
            }

            return Ok();
        }
        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAlbum(CreateAlbumDto albumToCreate)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            
            // Check if user exceeded album limit
            if (_limitSettings.Value.MaxAlbums != 0 && await _albumRepo.GetAlbumCount(userId) >= _limitSettings.Value.MaxAlbums)
            {
                return BadRequest($"You have reached the maximum album limit of {_limitSettings.Value.MaxAlbums.ToString()}");
            }
            
            var album = new Album(albumToCreate.IsPublic, userId, albumToCreate.Name);
            _albumRepo.Add(album);

            if (!await _albumRepo.SaveAll())
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new {message = "Failed at saving album"});
            }

            var albumToReturn = _mapper.Map<AlbumToReturnDto>(album);
            if (album.IsPublic)
            {
                return CreatedAtRoute("GetAlbum", new {controller = "Album", albumId = album.Id}, albumToReturn);
            }
            return CreatedAtRoute("GetAlbumPrivate", new {controller = "Album", albumId = album.Id}, albumToReturn);
        }
    }
}