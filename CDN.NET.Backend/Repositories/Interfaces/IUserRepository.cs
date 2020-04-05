using System.Collections.Generic;
using System.Threading.Tasks;
using CDN.NET.Backend.Dtos.UserDtos;
using CDN.NET.Backend.Models;

namespace CDN.NET.Backend.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUserById(int userId);
        Task<bool> TryRemoveUserById(int userId);
        Task RemoveUser(User user);
        Task<bool> IsAdmin(int userId);
        Task<User> GetUserByUsername(string username);
        Task<User> UpdateUserInfo(User user, UserAdminEditDto info);
        Task<List<User>> GetAllUser();
        Task<bool> SaveAll();
    }
}