using System.Threading.Tasks;
using CDN.NET.Backend.Models;

namespace CDN.NET.Backend.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUserById(int userId);
        Task<User> GetUserByUsername(string username);
        Task<bool> SaveAll();
    }
}