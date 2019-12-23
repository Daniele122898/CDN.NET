using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using CDN.NET.Wrapper.Dtos;
using CDN.NET.Wrapper.Enums;
using CDN.NET.Wrapper.Models;
using CDN.NET.Wrapper.Utils;

namespace CDN.NET.Wrapper.Client
{
    public partial class CdnClient
    {
        public async Task<FileUploadResponse> UploadFile(UploadFileInfo fileToUpload)
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

            throw new ArgumentException("File has to have a valid stream or path");
        }

        public async Task<FileUploadResponse> UploadFile(string pathToFile, string name = null, bool isPublic = true, int? albumId = null)
        {
            if (!File.Exists(pathToFile))
            {
                throw new FileNotFoundException($"Your specified file could not be found: {pathToFile}");
            }

            await using var stream = File.OpenRead(pathToFile);
            return await this.UploadFile(stream, name, isPublic, albumId).ConfigureAwait(false);
        }

        public async Task<FileUploadResponse> UploadFile(FileStream fileStream, string name = null, bool isPublic = true, int? albumId = null)
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

            return await this.GetAndMapResponse<FileUploadResponse>(
                Endpoints.FileUpload, 
                HttpMethods.Post,
                form, castPayloadWithoutJsonParsing: true).ConfigureAwait(false);
        }

        public async Task<IEnumerable<FileUploadResponse>> UploadFiles(UploadFileInfo[] filesToUpload, int? albumId = null)
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
            
            var resp = await this.GetAndMapResponse<IEnumerable<FileUploadResponse>>(
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