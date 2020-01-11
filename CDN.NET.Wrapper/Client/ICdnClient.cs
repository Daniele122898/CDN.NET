using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CDN.NET.Wrapper.Dtos;
using CDN.NET.Wrapper.Enums;
using CDN.NET.Wrapper.Models;
using CDN.NET.Wrapper.Utils;

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
        public Task<Maybe<LoginResponse>> Login(string username, string password);
        
        /// <summary>
        /// Create an account with the specified username and password
        /// </summary>
        /// <param name="username">Your Username</param>
        /// <param name="password">Your password</param>
        /// <returns>Returns your username and Id on success</returns>
        public Task<Maybe<User>> Register(string username, string password);
        
        /// <summary>
        /// Uses your client token to generate a new Api Key
        /// </summary>
        /// <returns>Returns a string with your Api key and stores it in the client</returns>
        public Task<Maybe<string>> GetApiKey();
        
        /// <summary>
        /// Uses your client token to remove your API key (on the server not from the client)
        /// Attention: Deleting the api token deletes it from the actual database not just from the client.
        /// If you did not use the login method before your jwt token cannot be refreshed and thus the client
        /// cannot authenticate until you login.
        /// </summary>
        /// <returns>Maybe indicating success or failiure</returns>
        public Task<Maybe<bool>> DeleteApiKey();
        
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

        /// <summary>
        /// Upload a single file with the specified values
        /// </summary>
        /// <param name="fileToUpload">File info with either path or stream and additional optional information</param>
        /// <returns>The file upload response with public url and co</returns>
        public Task<Maybe<FileResponse>> UploadFile(UploadFileInfo fileToUpload);
        
        /// <summary>
        /// Upload a single file
        /// </summary>
        /// <param name="pathToFile">Path to file</param>
        /// <param name="name">Name of file without any extensions, just the name the file should have publicly.
        /// This does not appear in the public url of the file, that will be its unique public ID</param>
        /// <param name="isPublic">If the file should be public or only reachable with YOUR api authentication.</param>
        /// <param name="albumId">The Id of the album it should belong to if any.</param>
        /// <returns>The file upload response with public url and co</returns>
        public Task<Maybe<FileResponse>> UploadFile(string pathToFile, string name = null, bool isPublic = true, int? albumId = null);
        
        /// <summary>
        /// Upload a single file
        /// </summary>
        /// <param name="fileStream">An open and not disposed file stream of the file</param>
        /// <param name="name">Name of file without any extensions, just the name the file should have publicly.
        /// This does not appear in the public url of the file, that will be its unique public ID</param>
        /// <param name="isPublic">If the file should be public or only reachable with YOUR api authentication.</param>
        /// <param name="albumId">The Id of the album it should belong to if any.</param>
        /// <returns>The file upload response with public url and co</returns>
        public Task<Maybe<FileResponse>> UploadFile(FileStream fileStream, string name = null, bool isPublic = true, int? albumId = null);
        
        /// <summary>
        /// Upload multiple files
        /// </summary>
        /// <param name="filesToUpload">File infos with path or stream. ATTENTION: The album Id within the file infos DO NOT matter!</param>
        /// <param name="albumId">Id of album to add these files to if any.</param>
        /// <returns>The file upload response with public url and co</returns>
        public Task<Maybe<IEnumerable<FileResponse>>> UploadFiles(UploadFileInfo[] filesToUpload, int? albumId = null);
        
        /// <summary>
        /// Removes the file with the specified public id
        /// </summary>
        /// <param name="publicId">Public Id of the file (can have extension)</param>
        /// <returns>Task indicating success</returns>
        public Task<Maybe<bool>> RemoveFile(string publicId);
        
        /// <summary>
        /// Removes the files with the specified public Ids. The backend will remove all the files it has found and
        /// ignore the ones it didn't. ALL files must be owned by you though otherwise the backend will not remove any.
        /// </summary>
        /// <param name="publicIds">Array of public Ids to remove (CANNOT have extension. ONLY id)</param>
        /// <returns>IEnumerable with all the files that have been removed</returns>
        public Task<Maybe<IEnumerable<FileRemoveResponse>>> RemoveFiles(string[] publicIds);

        /// <summary>
        /// Gets all the current users uploaded files
        /// </summary>
        /// <returns>All the files uploaded by the user</returns>
        public Task<Maybe<IEnumerable<FileResponse>>> GetAllFiles();
        
        // TODO maybe add file download capabilities. Might leave it to the user of the lib tho

        /// <summary>
        /// Get all your albums
        /// </summary>
        /// <returns>List of all your albums</returns>
        public Task<Maybe<IEnumerable<Album>>> GetAllAlbums();
        
        /// <summary>
        /// Get a private album of yours and all it's files (info) 
        /// </summary>
        /// <param name="id">Id of the album</param>
        /// <returns>The album info and all it's files (info)</returns>
        public Task<Maybe<Album>> GetPrivateAlbum(int id);
        
        /// <summary>
        /// Get an album and all it's files (info)
        /// </summary>
        /// <param name="id">The id</param>
        /// <returns>Album</returns>
        public Task<Maybe<Album>> GetAlbum(int id);
        
        /// <summary>
        /// Delete an album that is owned by you
        /// </summary>
        /// <param name="id">Id of album</param>
        /// <returns>Success indication</returns>
        public Task<Maybe<bool>> DeleteAlbum(int id);
        
        /// <summary>
        /// Create an album with the specified parameters
        /// </summary>
        /// <param name="name">Name of the album, defaults to unique Id</param>
        /// <param name="isPublic">If the album should be public or not</param>
        /// <returns>When successful, returns the album info</returns>
        public Task<Maybe<Album>> CreateAlbum(string name, bool isPublic = true);

        /// <summary>
        /// Gets the file info without the physical file
        /// </summary>
        /// <param name="publicId">Public Id of image</param>
        /// <returns>The file info</returns>
        public Task<Maybe<FileResponse>> GetFileInfo(string publicId);

        /// <summary>
        /// Gets the file info without the physical file of a private file
        /// </summary>
        /// <param name="publicId">Public Id of image</param>
        /// <returns>The file info</returns>
        public Task<Maybe<FileResponse>> GetPrivateFileInfo(string publicId);
    }
}