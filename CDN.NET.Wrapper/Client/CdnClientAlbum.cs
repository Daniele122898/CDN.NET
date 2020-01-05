using System.Collections.Generic;
using System.Threading.Tasks;
using CDN.NET.Wrapper.Dtos;
using CDN.NET.Wrapper.Enums;
using CDN.NET.Wrapper.Utils;

namespace CDN.NET.Wrapper.Client
{
    public partial class CdnClient
    {
        public async Task<IEnumerable<Album>> GetAllAlbums()
        {
            return await this.GetAndMapResponse<IEnumerable<Album>>(Endpoints.AlbumGetAll)
                .ConfigureAwait(false);
        }

        public async Task<Album> GetPrivateAlbum(string publicId)
        {
            return await this.GetAndMapResponse<Album>($"{Endpoints.AlbumGetPrivate}/{publicId}")
                .ConfigureAwait(false);
        }

        public async Task<Album> GetAlbum(string publicId)
        {
            return await this.GetAndMapResponse<Album>($"{Endpoints.AlbumBase}/{publicId}")
                .ConfigureAwait(false);
        }

        public async Task DeleteAlbum(string publicId)
        {
            await this.GetRawResponseAndEnsureSuccess($"{Endpoints.AlbumBase}/{publicId}", HttpMethods.Delete)
                .ConfigureAwait(false);
        }

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