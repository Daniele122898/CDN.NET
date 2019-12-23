using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CDN.NET.Wrapper.Dtos;
using CDN.NET.Wrapper.Enums;
using CDN.NET.Wrapper.Models;

namespace CDN.NET.Wrapper.Client
{
    public interface ICdnClient : IDisposable
    {
        
        /// <summary>
        /// Login using your username and password. This will also log you in
        /// on the client so you can use Authenticated endpoints
        /// </summary>
        /// <param name="username">Your username</param>
        /// <param name="password">Your password</param>
        /// <returns>Returns your user info on success and logs you into the client</returns>
        /// <exception cref="HttpRequestException">When something went wrong in the request</exception>
        public Task<LoginResponse> Login(string username, string password);
        
        /// <summary>
        /// Create an account with the specified username and password
        /// </summary>
        /// <param name="username">Your Username</param>
        /// <param name="password">Your password</param>
        /// <returns>Returns your username and Id on success</returns>
        /// <exception cref="HttpRequestException">When something went wrong in the request</exception>
        public Task<User> Register(string username, string password);
        
        /// <summary>
        /// Uses your client token to generate a new Api Key
        /// </summary>
        /// <returns>Returns a string with your Api key and stores it in the client or null if unexpected errors happen.</returns>
        /// <exception cref="HttpRequestException">When something went wrong in the request</exception>
        public Task<string> GetApiKey();
        
        /// <summary>
        /// Uses your client token to remove your API key (on the server not from the client)
        /// Attention: Deleting the api token deletes it from the actual database not just from the client.
        /// If you did not use the login method before your jwt token cannot be refreshed and thus the client
        /// cannot authenticate until you login.
        /// </summary>
        /// <returns>Task complete or error</returns>
        /// <exception cref="HttpRequestException">When something went wrong in the request</exception>
        public Task DeleteApiKey();
        
        /// <summary>
        /// Change the authentication method used in the client
        /// </summary>
        /// <param name="authenticationType">Specify what authentication type you wish to use</param>
        public void UseAuthenticationMethod(AuthenticationType authenticationType);

        /// <summary>
        /// Test if the authentication is successful with your current client settings
        /// </summary>
        /// <returns>A boolean to indicate success and the string that is returned on success</returns>
        /// <exception cref="HttpRequestException">When something went wrong in the request</exception>
        public Task<(bool success, string message)> TestAuthentication();

        public Task<FileUploadResponse> UploadFile(UploadFileInfo fileToUpload);
        public Task<FileUploadResponse> UploadFile(string pathToFile, string name = null, bool isPublic = true, int? albumId = null);
        public Task<FileUploadResponse> UploadFile(FileStream fileStream, string name = null, bool isPublic = true, int? albumId = null);
        public Task<IEnumerable<FileUploadResponse>> UploadFiles(UploadFileInfo[] filesToUpload, int? albumId = null);
        
        public Task RemoveFile(string publicId);
        public Task<IEnumerable<FileRemoveResponse>> RemoveFiles(string[] publicIds);
        

    }
}