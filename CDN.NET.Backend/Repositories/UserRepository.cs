using System.Threading.Tasks;
using CDN.NET.Backend.Data;
using CDN.NET.Backend.Models;
using CDN.NET.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CDN.NET.Backend.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;

        public UserRepository(DataContext context)
        {
            _context = context;
        }
        
        public async Task<User> GetUserById(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<User> GetUserByUsername(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Username.ToLower() == username.ToLower());
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}