using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using ArgonautCore.Maybe;
using CDN.NET.Wrapper.Dtos.File;
using CDN.NET.Wrapper.Enums;
using CDN.NET.Wrapper.Models;
using CDN.NET.Wrapper.Utils;

namespace CDN.NET.Wrapper.Client
{
    public partial class CdnClient
    {
        /// <summary>
        /// Upload a single file with the specified values
        /// </summary>
        /// <param name="fileToUpload">File info with either path or stream and additional optional information</param>
        /// <returns>The file upload response with public url and co</returns>
        public async Task<Maybe<FileResponse>> UploadFile(UploadFileInfo fileToUpload)
        {
            if (fileToUpload.HasPath)
            {
                return await this.UploadFile(fileToUpload.PathToFile, fileToUpload.Name, fileToUpload.IsPublic,
                    fileToUpload.AlbumId).ConfigureAwait(false);
            }

            if (fileToUpload.HasFileStream)
            {
                return await this.UploadFile(fileToUpload.FileStream, fileToUpload.Name, fileToUpload.IsPublic,
                    fileToUpload.AlbumId).ConfigureAwait(false);
            }

            return Maybe.FromErr<FileResponse>(new ArgumentException("File has to have a valid stream or path"));
        }

        /// <summary>
        /// Upload a single file
        /// </summary>
        /// <param name="pathToFile">Path to file</param>
        /// <param name="name">Name of file without any extensions, just the name the file should have publicly.
        /// This does not appear in the public url of the file, that will be its unique public ID</param>
        /// <param name="isPublic">If the file should be public or only reachable with YOUR api authentication.</param>
        /// <param name="albumId">The Id of the album it should belong to if any.</param>
        /// <returns>The file upload response with public url and co</returns>
        public async Task<Maybe<FileResponse>> UploadFile(string pathToFile, string name = null, bool isPublic = true, int? albumId = null)
        {
            if (!File.Exists(pathToFile))
            {
                return Maybe.FromErr<FileResponse>(
                    new FileNotFoundException($"Your specified file could not be found: {pathToFile}"));
            }

            await using var stream = File.OpenRead(pathToFile);
            return await this.UploadFile(stream, name, isPublic, albumId).ConfigureAwait(false);
        }

        /// <summary>
        /// Upload a single file
        /// </summary>
        /// <param name="fileStream">An open and not disposed file stream of the file</param>
        /// <param name="name">Name of file without any extensions, just the name the file should have publicly.
        /// This does not appear in the public url of the file, that will be its unique public ID</param>
        /// <param name="isPublic">If the file should be public or only reachable with YOUR api authentication.</param>
        /// <param name="albumId">The Id of the album it should belong to if any.</param>
        /// <returns>The file upload response with public url and co</returns>
        public async Task<Maybe<FileResponse>> UploadFile(FileStream fileStream, string name = null, bool isPublic = true, int? albumId = null)
        {
            using var form = new MultipartFormDataContent();
            using var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(MimeTypeMap.GetMimeType(Path.GetExtension(fileStream.Name)));
            form.Add(streamContent, "File", Path.GetFileName(fileStream.Name));
            if (!string.IsNullOrWhiteSpace(name))
            {
                form.Add(new StringContent(name), "Name");
            }
            form.Add(new StringContent(isPublic.ToString()), "IsPublic");
            if (albumId.HasValue)
            {
                form.Add(new StringContent(albumId.Value.ToString()), "AlbumId");
            }

            return await this.GetAndMapResponse<FileResponse>(
                Endpoints.FileUpload, 
                HttpMethods.Post,
                form, castPayloadWithoutJsonParsing: true).ConfigureAwait(false);
        }

        /// <summary>
        /// Upload multiple files
        /// </summary>
        /// <param name="filesToUpload">File infos with path or stream. ATTENTION: The album Id within the file infos DO NOT matter!</param>
        /// <param name="albumId">Id of album to add these files to if any.</param>
        /// <returns>The file upload response with public url and co</returns>
        public async Task<Maybe<IEnumerable<FileResponse>>> UploadFiles(UploadFileInfo[] filesToUpload, int? albumId = null)
        {
            using var form = new MultipartFormDataContent();
            List<MultiFileInfoDto> infoList = new List<MultiFileInfoDto>();
            List<(StreamContent streamContent, FileStream fileStream)> itemsToDispose = new List<(StreamContent streamContent, FileStream fileStream)>();
            foreach (var fileInfo in filesToUpload)
            {
                FileStream fileStream;
                if (fileInfo.HasPath && !fileInfo.HasFileStream)
                {
                    fileStream = File.OpenRead(fileInfo.PathToFile);
                }
                else
                {
                    fileStream = fileInfo.FileStream;
                }
                
                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(MimeTypeMap.GetMimeType(Path.GetExtension(fileStream.Name)));
                form.Add(streamContent, "Files", Path.GetFileName(fileStream.Name));
                
                infoList.Add(new MultiFileInfoDto()
                {
                    IsPublic = fileInfo.IsPublic,
                    Name = fileInfo.Name
                });
                itemsToDispose.Add((streamContent, fileStream));
            }
            
            string listJson = JsonSerializer.Serialize(infoList, _jsonOptions);
            form.Add(new StringContent(listJson), "Infos");
            
            if (albumId.HasValue)
            {
                form.Add(new StringContent(albumId.Value.ToString()), "AlbumId");
            }
            
            var resp = await this.GetAndMapResponse<IEnumerable<FileResponse>>(
                Endpoints.FileUploadMulti,
                HttpMethods.Post,
                form,
                castPayloadWithoutJsonParsing: true).ConfigureAwait(false);

            foreach ((StreamContent streamContent, FileStream fileStream) in itemsToDispose)
            {
                streamContent.Dispose();
                await fileStream.DisposeAsync().ConfigureAwait(false);
            }
            
            return resp;
        }
    }
}