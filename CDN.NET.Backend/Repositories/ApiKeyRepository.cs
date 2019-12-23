using System;
using System.Threading.Tasks;
using CDN.NET.Backend.Data;
using CDN.NET.Backend.Models;
using CDN.NET.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CDN.NET.Backend.Repositories
{
    public class ApiKeyRepository : IApiKeyRepository
    {
        // TODO handle caching.
        
        private readonly DataContext _context;

        public ApiKeyRepository(DataContext context)
        {
            _context = context;
        }
        
        /// <summary>
        /// Creates api key and returns said key, No saving or anything with the user.
        /// </summary>
        /// <param name="user">User needed to create key</param>
        /// <returns></returns>
        public ApiKey CreateApiKey(User user)
        {
            string token = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes((Guid.NewGuid().ToString()+Guid.NewGuid().ToString())));
            var apiKey = new ApiKey()
            {
                Key = token,
                UserId = user.Id,
                User = user
            };
            return apiKey;
        }

        public async Task<bool> IsActiveApiKey(string key)
        {
            return await _context.ApiKeys.AnyAsync(x => x.Key == key);
        }

        public async Task<ApiKey> GetKey(string key)
        {
            return await _context.ApiKeys.FirstOrDefaultAsync(x => x.Key == key);
        }

        public void RemoveApiKey(ApiKey key)
        {
            _context.ApiKeys.Remove(key);
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}