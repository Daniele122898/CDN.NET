using System.Threading.Tasks;
using CDN.NET.Backend.Models;

namespace CDN.NET.Backend.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<User> Register(User user, string password);
        Task<User> Register(string username, string password);
        Task<User> Login(string username, string password);
        Task<bool> UserExistsByUsername(string username);
        Task<bool> UserExistsById(int id);
    }
}