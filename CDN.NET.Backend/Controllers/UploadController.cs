using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using CDN.NET.Backend.Dtos.UploadDtos;
using CDN.NET.Backend.Helpers;
using CDN.NET.Backend.Models;
using CDN.NET.Backend.Repositories.Interfaces;
using CDN.NET.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CDN.NET.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UploadController : ControllerBase
    {
        private readonly IFileRepository _fileRepo;
        private readonly IUserRepository _userRepo;
        private readonly IOptions<UploadSettings> _uploadSettings;
        private readonly ILogger<UploadController> _logger;
        private readonly IMapper _mapper;
        private readonly IAlbumRepository _albumRepo;
        private readonly IOptions<LimitSettings> _limitSettings;
        private readonly IUtilsService _utilsService;

        public UploadController(IFileRepository fileRepo, IUserRepository userRepo,
            IOptions<UploadSettings> uploadSettings, ILogger<UploadController> logger, IMapper mapper, 
            IAlbumRepository albumRepo, IOptions<LimitSettings> limitSettings, IUtilsService utilsService)
        {
            _fileRepo = fileRepo;
            _userRepo = userRepo;
            _uploadSettings = uploadSettings;
            _logger = logger;
            _mapper = mapper;
            _albumRepo = albumRepo;
            _limitSettings = limitSettings;
            _utilsService = utilsService;
        }

        class FileStreamData
        {
            public string Path { get; set; }
            public Task CopyTask { get; set; }
            public FileStream FileStream { get; set; }
        }

        [HttpPost("multi")]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [RequestSizeLimit(int.MaxValue)]
        public async Task<ActionResult<IEnumerable<UFileReturnDto>>> UploadFiles([FromForm] UFilesReceiveDto filesReceiveDto)
        {
            
            if (filesReceiveDto.Files == null || filesReceiveDto.Files.Count == 0)
                return BadRequest("No files found");

            if (filesReceiveDto.Files.Count > _uploadSettings.Value.MaxFiles)
                return BadRequest($"Too many files. Max files is {_uploadSettings.Value.MaxFiles.ToString()}");
            
            // Serialize the info tab
            var infoList = filesReceiveDto.Infos == null ? null : 
                JsonSerializer.Deserialize<List<MultiFileInfoDto>>(filesReceiveDto.Infos, new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (infoList != null && infoList.Count != 0 && infoList.Count != filesReceiveDto.Files.Count)
                return BadRequest("Either leave infos empty or add the same amount of info entries as files!");
            
            // before we do any computation or allocation
            foreach (var file in filesReceiveDto.Files)
            {
                if (file.Length > _uploadSettings.Value.MaxSize)
                {
                    return BadRequest(
                        $"File too big. Maximum file size allowed is {_uploadSettings.Value.MaxSize.ToString()} bytes");
                }
                string extension = Path.GetExtension(file.FileName);
                if (_uploadSettings.Value.BlockedExtensions.Any(
                    e => e.Equals(extension, StringComparison.OrdinalIgnoreCase)))
                {
                    return BadRequest($"File extension {extension} is not allowed");
                }
            }

            Album album = null;
            // check if specified album exists
            if (filesReceiveDto.AlbumId.HasValue)
            {
                album = await _albumRepo.GetAlbumById(filesReceiveDto.AlbumId.Value);
                if (album == null)
                {
                    return BadRequest("Album doesn't exist.");
                }
            }

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            
            // Check if user exceeded upload limit
            if (_limitSettings.Value.MaxFiles != 0 && await _fileRepo.GetFileCount(userId) >= _limitSettings.Value.MaxFiles)
            {
                return BadRequest($"You have reached the maximum file limit of {_limitSettings.Value.MaxFiles.ToString()}");
            }
            
            var userFromRepo = await _userRepo.GetUserById(userId);
            List<FileStreamData> fileStreams = new List<FileStreamData>(filesReceiveDto.Files.Count);
            List<UFile> dbFiles = new List<UFile>(filesReceiveDto.Files.Count);

            try
            {
                int index = 0;
                foreach (var file in filesReceiveDto.Files)
                {
                    
                    string extension = Path.GetExtension(file.FileName);

                    string fileId = Guid.NewGuid().ToString();
                    string path = _utilsService.GenerateFilePath(fileId, extension);
                    var fileData = new FileStreamData()
                    {
                        Path = path,
                        CopyTask = null,
                        FileStream = new FileStream(path, FileMode.Create)
                    };
                    // begin entire file download and saving
                    fileData.CopyTask = file.CopyToAsync(fileData.FileStream);
                    fileStreams.Add(fileData);
                    // create files for database
                    bool isPublic = true;
                    string name = fileId;
                    if (infoList != null && infoList.Count != 0)
                    {
                        isPublic = infoList[index].IsPublic;
                        name = infoList[index].Name ?? fileId;
                    }

                    var uFile = new UFile(fileId, isPublic, extension, file.ContentType,
                        userFromRepo.Id, name);
                    if (album != null)
                    {
                        uFile.AlbumId = album.Id;
                    }
                    dbFiles.Add(uFile);

                    index++;
                }

                // now await the files one after another
                foreach (var file in fileStreams)
                {
                    await file.CopyTask;
                }

                // now we know all files successfully downloaded and saved
                // add to the DB
                foreach (var dbFile in dbFiles)
                {
                    _fileRepo.Add(dbFile);
                }

                if (!await _fileRepo.SaveAll())
                {
                    await UndoChanges();
                    return BadRequest("Failed saving the files");
                }

                var filesToReturn = _mapper.Map<IEnumerable<UFileReturnDto>>(dbFiles);
                return Ok(filesToReturn);
            }
            catch (Exception e)
            {
                await UndoChanges();
                _logger.Log(LogLevel.Error, e, "Failed to save Files!");
                return BadRequest("Something failed sorry :(");
            }
            finally
            {
                foreach (var data in fileStreams)
                {
                    await data.FileStream.DisposeAsync();
                    data.CopyTask.Dispose();
                }
            }
            
            async Task UndoChanges()
            {
                for (int i = 0; i < fileStreams.Count; i++)
                {
                    var file = fileStreams[i];
                    var dbFile = dbFiles[i];

                    // wait in case its not finished yet
                    await file.CopyTask;
                    await file.FileStream.DisposeAsync();
                    file.CopyTask.Dispose();
                    // remove photo if it exists
                    if (System.IO.File.Exists(file.Path))
                        System.IO.File.Delete(file.Path);
                    // remove Db entries
                    var fileFromDb = await _fileRepo.GetFile(dbFile.PublicId);
                    if (fileFromDb != null)
                    {
                        _fileRepo.Delete(fileFromDb);
                    }
                }

                await _fileRepo.SaveAll();
            }
        }

        [HttpPost]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [RequestSizeLimit(int.MaxValue)]
        public async Task<ActionResult<UFileReturnDto>> UploadFile([FromForm] UFileReceiveDto fileReceiveDto)
        {
            if (fileReceiveDto.File == null || fileReceiveDto.File.Length == 0)
                return BadRequest("No file found");

            if (fileReceiveDto.File.Length > _uploadSettings.Value.MaxSize)
                return BadRequest($"File too big. Maximum file size allowed is {_uploadSettings.Value.MaxSize.ToString()} bytes");
            
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            
            // Check if user exceeded upload limit
            if (_limitSettings.Value.MaxFiles != 0 && await _fileRepo.GetFileCount(userId) >= _limitSettings.Value.MaxFiles)
            {
                return BadRequest($"You have reached the maximum file limit of {_limitSettings.Value.MaxFiles.ToString()}");
            }
            
            string extension = Path.GetExtension(fileReceiveDto.File.FileName);
            if (_uploadSettings.Value.BlockedExtensions.Any(
                e => e.Equals(extension, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest("File extension is not allowed");
            }
            
            var userFromRepo = await _userRepo.GetUserById(userId);
            
            string fileId = Guid.NewGuid().ToString();
            string path = _utilsService.GenerateFilePath(fileId, extension);
            Task copyTask = null;
            try
            {
                if (string.IsNullOrWhiteSpace(fileReceiveDto.File.ContentType))
                {
                    return BadRequest("File content type has to be set");
                }
                await using FileStream fs = new FileStream(path, FileMode.Create);
                // let the copy task run but let's not wait for it as memory is slow
                copyTask = fileReceiveDto.File.CopyToAsync(fs);
                var uFile = new UFile(fileId, fileReceiveDto.IsPublic,extension, 
                    fileReceiveDto.File.ContentType, userFromRepo.Id ,fileReceiveDto.Name);
                // Add to album if album Id is given            
                if (fileReceiveDto.AlbumId.HasValue)
                {
                    var album = await _albumRepo.GetAlbumById(fileReceiveDto.AlbumId.Value);
                    if (album == null)
                    {
                        return await WaitForFileAndRemove("AlbumId is invalid.");
                    }

                    uFile.AlbumId = album.Id;
                    album.UFiles.Add(uFile);
                }
                _fileRepo.Add(uFile);
                if (!await _fileRepo.SaveAll())
                {
                    return await WaitForFileAndRemove("Failed at saving the file");
                }
                // wait now so we can make sure there are no exceptions before we reply with Ok.
                await copyTask;
                var fileToReturn = _mapper.Map<UFileReturnDto>(uFile);
                if (uFile.IsPublic)
                {
                    return CreatedAtRoute("GetFile",
                        new {controller = "File", publicIdAndExtension = uFile.PublicId + uFile.FileExtension}, fileToReturn);
                }
                return CreatedAtRoute("GetPrivateFile",
                    new {controller = "File", publicIdAndExtension = uFile.PublicId + uFile.FileExtension}, fileToReturn);
            }
            catch (Exception e)
            {
                if (copyTask != null)
                    await copyTask;
                // remove photo if it exists
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
                // check if there's an entry with the current file so we can remove it
                var fileFound = await _fileRepo.GetFile(fileId);
                if (fileFound != null)
                {
                    _fileRepo.Delete(fileFound);
                    await _fileRepo.SaveAll();
                }
                // log it and return
                _logger.Log(LogLevel.Error, e, "Failed to save File!");
                return this.BadRequest("Something broke sorry :(");
            }

            async Task<ActionResult<UFileReturnDto>> WaitForFileAndRemove(string reason)
            {
                // remove photo if it exists
                // gotta wait for it now or we may miss to remove it
                await copyTask;
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
                return BadRequest(reason);
            }
        }
    }
}