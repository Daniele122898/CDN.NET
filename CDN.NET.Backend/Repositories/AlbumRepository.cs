using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CDN.NET.Backend.Data;
using CDN.NET.Backend.Models;
using CDN.NET.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CDN.NET.Backend.Repositories
{
    public class AlbumRepository : IAlbumRepository
    {
        private readonly DataContext _context;

        public AlbumRepository(DataContext context)
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
        
        public async Task<Album> GetAlbumById(int albumId)
        {
            return await _context.Albums.FindAsync(albumId);
        }

        public async Task<int> GetAlbumCount(int userId)
        {
            return await _context.Albums.CountAsync(a => a.OwnerId == userId);
        }

        public async Task<List<Album>> GetAllAlbumsFromUser(int userId)
        {
            return await _context.Albums.Where(a => a.OwnerId == userId).ToListAsync();
        }
    }
}