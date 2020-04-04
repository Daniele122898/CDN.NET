using System.Collections.Generic;
using System.Threading.Tasks;
using ArgonautCore.Maybe;
using CDN.NET.Wrapper.Dtos.User;
using CDN.NET.Wrapper.Enums;
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
        public async Task<Maybe<bool>> AdminRemoveUser(int userId)
        {
            return (await this.GetRawResponseAndEnsureSuccess($"{Endpoints.AdminDeleteUser}/{userId.ToString()}",
                    HttpMethods.Delete)
                .ConfigureAwait(false)).ToSuccessMaybe();
        }

        /// <inheritdoc />
        public async Task<Maybe<UserInfoDto>> AdminUpdateUser(int userId, UserUpdateInfo updateInfo)
        {
            throw new System.NotImplementedException();
        }
    }
}