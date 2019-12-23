using CDN.NET.Wrapper.Models;

namespace CDN.NET.Wrapper.Dtos
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public User User { get; set; }
    }
}