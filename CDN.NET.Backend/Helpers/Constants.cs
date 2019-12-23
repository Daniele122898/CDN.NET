using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CDN.NET.Backend.Helpers
{
    public class Constants
    {
        public const string TOKEN_ISSUER = "http://localhost:5000/";
        public const string STORAGE_FOLDER_NAME = "fileStorage";

        public string RootPath { get; private set; }
        public string ContentPath { get; private set; }
        public readonly string BaseUrl;
        
        private SymmetricSecurityKey _jwtKey;
        private readonly AppSettings _appSettings;

        public Constants(IConfiguration configuration)
        {
            _appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();
            BaseUrl = _appSettings.BaseUrl;
        }

        public SymmetricSecurityKey LazyGetJwtKey()
        {
            if (_jwtKey == null)
            {
                _jwtKey = new SymmetricSecurityKey(
                    Encoding.ASCII.GetBytes(_appSettings.Token));
            }

            return _jwtKey;
        }

        public void SetRootPath(string path)
        {
            RootPath = path;
        }

        public void SetContentPath(string path)
        {
            ContentPath = path;
        }
    }
}