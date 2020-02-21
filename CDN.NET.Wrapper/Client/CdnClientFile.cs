using System.Collections.Generic;
using System.Threading.Tasks;
using ArgonautCore.Maybe;
using CDN.NET.Wrapper.Dtos;
using CDN.NET.Wrapper.Enums;
using CDN.NET.Wrapper.Utils;

namespace CDN.NET.Wrapper.Client
{
    public partial class CdnClient
    {
        /// <summary>
        /// Removes the file with the specified public id
        /// </summary>
        /// <param name="publicId">Public Id of the file (can have extension)</param>
        /// <returns>Task indicating success</returns>
        public async Task<Maybe<bool>> RemoveFile(string publicId)
        {
            return (await this.GetRawResponseAndEnsureSuccess(
                $"{Endpoints.FileRemove}/{publicId}",
                HttpMethods.Delete).ConfigureAwait(false)).ToSuccessMaybe();
        }

        /// <summary>
        /// Removes the files with the specified public Ids. The backend will remove all the files it has found and
        /// ignore the ones it didn't. ALL files must be owned by you though otherwise the backend will not remove any.
        /// </summary>
        /// <param name="publicIds">Array of public Ids to remove (CANNOT have extension. ONLY id)</param>
        /// <returns>IEnumerable with all the files that have been removed</returns>
        public async Task<Maybe<IEnumerable<FileRemoveResponse>>> RemoveFiles(string[] publicIds)
        {
            return await this.GetAndMapResponse<IEnumerable<FileRemoveResponse>>(Endpoints.FileRemoveMulti, HttpMethods.Delete, publicIds).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<Maybe<IEnumerable<FileResponse>>> GetAllFiles()
        {
            return await this.GetAndMapResponse<IEnumerable<FileResponse>>(
                Endpoints.FileGetAll
            ).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<Maybe<FileResponse>> GetFileInfo(string publicId)
        {
            return await this.GetAndMapResponse<FileResponse>($"{Endpoints.FileGet}/{publicId}")
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<Maybe<FileResponse>> GetPrivateFileInfo(string publicId)
        {
            return await this.GetAndMapResponse<FileResponse>($"{Endpoints.FileGetPrivate}/{publicId}")
                .ConfigureAwait(false);
        }
    }
}