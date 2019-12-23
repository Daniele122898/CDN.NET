using System.IO;
using CDN.NET.Backend.Helpers;
using CDN.NET.Backend.Services.Interfaces;

namespace CDN.NET.Backend.Services
{
    public class UtilsService : IUtilsService
    {
        private readonly Constants _constants;

        public UtilsService(Constants constants)
        {
            _constants = constants;
        }
        
        public string GenerateFilePath(string fileId, string fileExtension)
        {
            return Path.Combine(_constants.ContentPath, Constants.STORAGE_FOLDER_NAME, fileId + fileExtension);
        }
    }
}