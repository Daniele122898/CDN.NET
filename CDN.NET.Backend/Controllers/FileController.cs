using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CDN.NET.Backend.Dtos.FileDtos;
using CDN.NET.Backend.Dtos.UploadDtos;
using CDN.NET.Backend.Repositories.Interfaces;
using CDN.NET.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;

namespace CDN.NET.Backend.Controllers
{
    /// <summary>
    /// Controller to mostly get files and albums
    /// </summary>
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IFileRepository _fileRepo;
        private readonly IMapper _mapper;
        private readonly IUtilsService _utilsService;

        public FileController(IFileRepository fileRepo, IMapper mapper, IUtilsService utilsService)
        {
            _fileRepo = fileRepo;
            _mapper = mapper;
            _utilsService = utilsService;
        }

        [HttpGet("api/[controller]/getAll")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UFileReturnDto>>> GetAllFilesFromUser()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var files = await _fileRepo.GetFilesFromUser(userId);

            var filesToReturn = _mapper.Map<IEnumerable<UFileReturnDto>>(files);
            return Ok(filesToReturn);
        }
        
        [HttpGet("[controller]/private/{publicIdAndExtension}", Name = "GetPrivateFile")]
        [Authorize]
        public async Task<IActionResult> GetPrivateFileFromPublicId(string publicIdAndExtension)
        {
            int index = publicIdAndExtension.IndexOf('.');
            if (index <= 0)
                return NotFound();
            string publicId = publicIdAndExtension.Remove(index);
            var fileFromRepo = await _fileRepo.GetFile(publicId);
            if (fileFromRepo == null)
                return NotFound();

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (fileFromRepo.OwnerId != userId)
                return Unauthorized();
            
            // File is owned by token holder
            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = fileFromRepo.Name + fileFromRepo.FileExtension,
                Inline = true
            };
            
            Response.Headers.Add("Content-Disposition", cd.ToString());
            var filePath = _utilsService.GenerateFilePath(fileFromRepo.PublicId, fileFromRepo.FileExtension);
            return PhysicalFile(filePath, fileFromRepo.ContentType);
        }

        /// <summary>
        /// Remove multiple files at once. It will try to remove everything it finds, ignoring IDs that dont exist as long
        /// as there is at least one file to remove
        /// </summary>
        /// <param name="publicIds">The public IDs of the files to delete. DO NOT ADD THE FILE EXTENSION</param>
        /// <returns>Returns the infos of the files it deleted</returns>
        [HttpDelete("api/[controller]/multi")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<FileDeleteReturnDto>>> RemoveMultipleFiles(List<string> publicIds)
        {
            if (publicIds == null || publicIds.Count == 0)
                return BadRequest("Please add public IDs even if the file is private");
            
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var fileList = await _fileRepo.GetFiles(publicIds);
            if (fileList == null || fileList.Count == 0)
                return NotFound();

            if (fileList.Any(x => x.OwnerId != userId))
                return Unauthorized("Not all specified files are owned by you");

            foreach (var file in fileList)
            {
                string path = _utilsService.GenerateFilePath(file.PublicId, file.FileExtension);
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
                
                _fileRepo.Delete(file);
            }

            if (!await _fileRepo.SaveAll())
                return BadRequest("Something broke, please try again");

            var filesToReturn = _mapper.Map<IEnumerable<FileDeleteReturnDto>>(fileList);

            return Ok(filesToReturn);
        }
        
        /// <summary>
        /// Deletes the specified file. You only need the public ID but CAN add the file extension. 
        /// </summary>
        /// <param name="publicIdAndExtension">The public Id of the image and optionally the file extension at the end</param>
        /// <returns>Http Status with success code</returns>
        [HttpDelete("api/[controller]/{publicIdAndExtension}")]
        [Authorize]
        public async Task<IActionResult> RemoveFile(string publicIdAndExtension)
        {
            int index = publicIdAndExtension.IndexOf('.');
            string publicId = index <= 0 ? publicIdAndExtension : publicIdAndExtension.Remove(index);

            var fileFromRepo = await _fileRepo.GetFile(publicId);
            if (fileFromRepo == null)
                return NotFound();
            
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (fileFromRepo.OwnerId != userId)
                return Unauthorized();
            // Remove physical file
            string path = _utilsService.GenerateFilePath(fileFromRepo.PublicId, fileFromRepo.FileExtension);
            if(System.IO.File.Exists(path))
                System.IO.File.Delete(path);
            _fileRepo.Delete(fileFromRepo);

            if (!await _fileRepo.SaveAll())
                return BadRequest("Something broke, please retry");

            return Ok();
        }
        
        [HttpGet("[controller]/{publicIdAndExtension}", Name = "GetFile")]
        public async Task<IActionResult> GetFileByPublicId(string publicIdAndExtension)
        {
            int index = publicIdAndExtension.IndexOf('.');
            if (index <= 0)
                return NotFound();
            string publicId = publicIdAndExtension.Remove(index);
            var fileFromRepo = await _fileRepo.GetFile(publicId);

            if (fileFromRepo == null)
                return NotFound();

            // Check permissions and co.
            if (!fileFromRepo.IsPublic)
                return Unauthorized("This image is not public, please use the private URL for this image");
            
            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = fileFromRepo.Name + fileFromRepo.FileExtension,
                Inline = true
            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
            var filePath = _utilsService.GenerateFilePath(publicId, fileFromRepo.FileExtension);
            return PhysicalFile(filePath, fileFromRepo.ContentType);
        }

        /// <summary>
        /// Get file info without actually getting the physical file
        /// </summary>
        /// <param name="publicId">The public Id of the picture</param>
        /// <returns>File information</returns>
        [HttpGet("api/[controller]/{publicId}")]
        public async Task<ActionResult<UFileReturnDto>> GetFileInfo(string publicId)
        {
            var fileFromRepo = await _fileRepo.GetFile(publicId);

            if (fileFromRepo == null)
                return NotFound();
            
            // Check if file is private
            if (!fileFromRepo.IsPublic)
                return Unauthorized("This image is not public, please use the private URL for this image");

            var fileToReturn = _mapper.Map<UFileReturnDto>(fileFromRepo);
            return fileToReturn;
        }
        
        /// <summary>
        /// Get private file info without actually getting the physical file
        /// </summary>
        /// <param name="publicId">The public Id of the picture</param>
        /// <returns>File information</returns>
        [HttpGet("api/[controller]/private/{publicId}")]
        [Authorize]
        public async Task<ActionResult<UFileReturnDto>> GetPrivateFileInfo(string publicId)
        {
            var fileFromRepo = await _fileRepo.GetFile(publicId);

            if (fileFromRepo == null)
                return NotFound();
            
            // Check if you have permission to see the file info
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (fileFromRepo.OwnerId != userId)
                return Unauthorized("This image is not owned by you");

            var fileToReturn = _mapper.Map<UFileReturnDto>(fileFromRepo);
            return fileToReturn;
        }
    }
}