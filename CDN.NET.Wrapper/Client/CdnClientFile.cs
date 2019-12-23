using System.Collections.Generic;
using System.Threading.Tasks;
using CDN.NET.Wrapper.Dtos;
using CDN.NET.Wrapper.Enums;
using CDN.NET.Wrapper.Utils;

namespace CDN.NET.Wrapper.Client
{
    public partial class CdnClient
    {
        public Task RemoveFile(string publicId)
        {
            return this.GetResponse(
                $"{Endpoints.FileRemove}/{publicId}", 
                HttpMethods.Delete);
        }

        public async Task<IEnumerable<FileRemoveResponse>> RemoveFiles(string[] publicIds)
        {
            return await this.GetAndMapResponse<IEnumerable<FileRemoveResponse>>(Endpoints.FileRemoveMulti, HttpMethods.Delete, publicIds).ConfigureAwait(false);
        }
    }
}