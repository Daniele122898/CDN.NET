using System.Collections.Generic;
using System.Threading.Tasks;
using ArgonautCore.Maybe;
using CDN.NET.Wrapper.Dtos.User;
using CDN.NET.Wrapper.Models;
using CDN.NET.Wrapper.Utils;

namespace CDN.NET.Wrapper.Client
{
    public partial class CdnClient 
    {
        /// <inheritdoc />
        public async Task<Maybe<IEnumerable<AdminUserInfoDto>>> AdminGetAllUsers()
        {
            return await this.GetAndMapResponse<IEnumerable<AdminUserInfoDto>>(Endpoints.AdminGetAllUsers)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task<Maybe<bool>> AdminRemoveUser(int userId)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<Maybe<UserInfoDto>> AdminUpdateUser(int userId, UserUpdateInfo updateInfo)
        {
            throw new System.NotImplementedException();
        }
    }
}