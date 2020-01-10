using System.Collections.Generic;
using System.Threading.Tasks;
using CDN.NET.Wrapper.Dtos;
using CDN.NET.Wrapper.Enums;
using CDN.NET.Wrapper.Utils;

namespace CDN.NET.Wrapper.Client
{
    public partial class CdnClient
    {
        /// <inheritdoc />
        public async Task<IEnumerable<Album>> GetAllAlbums()
        {
            return await this.GetAndMapResponse<IEnumerable<Album>>(Endpoints.AlbumGetAll)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<Album> GetPrivateAlbum(int id)
        {
            return await this.GetAndMapResponse<Album>($"{Endpoints.AlbumGetPrivate}/{id.ToString()}")
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<Album> GetAlbum(int id)
        {
            return await this.GetAndMapResponse<Album>($"{Endpoints.AlbumBase}/{id.ToString()}")
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task DeleteAlbum(int id)
        {
            await this.GetRawResponseAndEnsureSuccess($"{Endpoints.AlbumBase}/{id.ToString()}", HttpMethods.Delete)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<Album> CreateAlbum(string name = null, bool isPublic = true)
        {
            return await this.GetAndMapResponse<Album>(
                    Endpoints.AlbumBase,
                    HttpMethods.Post,
                    new {isPublic, name}
            ).ConfigureAwait(false);
        }
    }
}