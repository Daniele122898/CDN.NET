using System.Threading.Tasks;
using CDN.NET.Backend.Models;

namespace CDN.NET.Backend.Repositories.Interfaces
{
    public interface IApiKeyRepository
    {
        ApiKey CreateApiKey(User user);
        Task<bool> IsActiveApiKey(string key);
        Task<ApiKey> GetKey(string key);
        void RemoveApiKey(ApiKey key);
        Task<bool> SaveAll();
    }
}