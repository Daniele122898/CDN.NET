using System.Collections.Generic;
using System.Threading.Tasks;
using CDN.NET.Backend.Models;

namespace CDN.NET.Backend.Repositories.Interfaces
{
    public interface IAlbumRepository
    {
        void Add<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        Task<bool> SaveAll();
        Task<Album> GetAlbumById(int albumId);
        Task<int> GetAlbumCount(int userId);
        Task<List<Album>> GetAllAlbumsFromUser(int userId);

    }
}