using System.Collections.Generic;
using System.Threading.Tasks;
using CDN.NET.Backend.Data;
using CDN.NET.Backend.Dtos.UserDtos;
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

        public async Task<bool> TryRemoveUserById(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }
            
            // Make sure he doesn't have any files left
            if (user.Files.Count > 0)
            {
                return false;
            }

            _context.Users.Remove(user);
            // Make sure we only return true if we actually removed anything.
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task RemoveUser(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsAdmin(int userId)
        {
            return (await _context.Users.FindAsync(userId))?.IsAdmin ?? false;
        }

        public async Task<User> GetUserByUsername(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Username.ToLower() == username.ToLower());
        }

        public async Task<User> UpdateUserInfo(User user, UserAdminEditDto info)
        {
            user.Username = info.Username ?? user.Username;
            user.IsAdmin = info.IsAdmin ?? user.IsAdmin;
            
            if (!string.IsNullOrWhiteSpace(info.Password))
            {
                AuthRepository.CreatePasswordHash(info.Password, out var passwordHash, out var passwordSalt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<List<User>> GetAllUser()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}