using System.Collections.Generic;
using System.Threading.Tasks;
using ArgonautCore.Maybe;
using CDN.NET.Wrapper.Dtos.Album;
using CDN.NET.Wrapper.Enums;
using CDN.NET.Wrapper.Utils;

namespace CDN.NET.Wrapper.Client
{
    public partial class CdnClient
    {
        /// <inheritdoc />
        public async Task<Maybe<IEnumerable<Album>>> GetAllAlbums()
        {
            return await this.GetAndMapResponse<IEnumerable<Album>>(Endpoints.AlbumGetAll)
                .ConfigureAwait(false);
        }


        /// <inheritdoc />
        public async Task<Maybe<IEnumerable<AlbumsSparse>>> GetAllAlbumsSparse()
        {
            return await this.GetAndMapResponse<IEnumerable<AlbumsSparse>>(Endpoints.AlbumGetAllSparse)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<Maybe<Album>> GetPrivateAlbum(int id)
        {
            return await this.GetAndMapResponse<Album>($"{Endpoints.AlbumGetPrivate}/{id.ToString()}")
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<Maybe<Album>> GetAlbum(int id)
        {
            return await this.GetAndMapResponse<Album>($"{Endpoints.AlbumBase}/{id.ToString()}")
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<Maybe<bool>> DeleteAlbum(int id)
        {
            return (await this.GetRawResponseAndEnsureSuccess($"{Endpoints.AlbumBase}/{id.ToString()}", HttpMethods.Delete)
                .ConfigureAwait(false)).ToSuccessMaybe();
        }

        /// <inheritdoc />
        public async Task<Maybe<Album>> CreateAlbum(string name = null, bool isPublic = true)
        {
            return await this.GetAndMapResponse<Album>(
                Endpoints.AlbumBase,
                HttpMethods.Post,
                new {isPublic, name}
            ).ConfigureAwait(false);
        }
    }
}