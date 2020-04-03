using System.Collections.Generic;
using System.Threading.Tasks;
using ArgonautCore.Maybe;
using CDN.NET.Wrapper.Dtos.User;
using CDN.NET.Wrapper.Models;

namespace CDN.NET.Wrapper.Client
{
    public partial class CdnClient 
    {
        /// <inheritdoc />
        public Task<Maybe<IEnumerable<AdminUserInfoDto>>> AdminGetAllUsers()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public Task<Maybe<bool>> AdminRemoveUser(int userId)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public Task<Maybe<UserInfoDto>> AdminUpdateUser(int userId, UserUpdateInfo updateInfo)
        {
            throw new System.NotImplementedException();
        }
    }
}