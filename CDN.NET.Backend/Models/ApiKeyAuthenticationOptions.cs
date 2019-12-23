using Microsoft.AspNetCore.Authentication;

namespace CDN.NET.Backend.Models
{
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DEFAULT_SCHEME = "APIKey";
        public string Scheme => DEFAULT_SCHEME;
        public string AuthenticationType = DEFAULT_SCHEME;
    }
}