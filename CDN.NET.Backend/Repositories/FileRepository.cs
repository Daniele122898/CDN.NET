using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CDN.NET.Backend.Data;
using CDN.NET.Backend.Helpers;
using CDN.NET.Backend.Models;
using CDN.NET.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CDN.NET.Backend.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly DataContext _context;

        public FileRepository(DataContext context)
        {
            _context = context;
        }
        
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<UFile> GetFile(int id)
        {
            return await _context.UFiles.FindAsync(id);
        }

        public async Task<UFile> GetFile(string publicId)
        {
            return await _context.UFiles.FirstOrDefaultAsync(f => f.PublicId == publicId);
        }

        public async Task<List<UFile>> GetFiles(List<string> publicIds)
        {
            return await _context.UFiles.Where(x => publicIds.Contains(x.PublicId)).ToListAsync();
        }

        public async Task<List<UFile>> GetFilesFromUser(int userId)
        {
            return await _context.UFiles.Where(f => f.OwnerId == userId).ToListAsync();
        }

        public async Task<PagedList<UFile>> GetFilesFromUserPaged(int userId, PageUserParams userParams)
        {
            var users = _context.UFiles.Where(f => f.OwnerId == userId).OrderByDescending(o => o.DateAdded);

            return await PagedList<UFile>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        public async Task<int> GetFileCount(int userId)
        {
            return await _context.UFiles.CountAsync(f => f.OwnerId == userId);
        }
    }
}