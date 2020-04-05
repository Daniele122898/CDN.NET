using System.Text.Json.Serialization;

namespace CDN.NET.Wrapper.Models
{
    public class UserUpdateInfo
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool? IsAdmin { get; set; }
    }
}