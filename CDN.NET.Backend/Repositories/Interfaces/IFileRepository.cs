using System.Collections.Generic;
using System.Threading.Tasks;
using CDN.NET.Backend.Helpers;
using CDN.NET.Backend.Models;

namespace CDN.NET.Backend.Repositories.Interfaces
{
    public interface IFileRepository
    {
        void Add<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        Task<bool> SaveAll();
        Task<UFile> GetFile(int id);
        Task<UFile> GetFile(string publicId);
        Task<List<UFile>> GetFiles(List<string> publicIds);
        Task<List<UFile>> GetFilesFromUser(int userId);
        Task<PagedList<UFile>> GetFilesFromUserPaged(int userId, PageUserParams userParams);
        Task<int> GetFileCount(int userId);

    }
}