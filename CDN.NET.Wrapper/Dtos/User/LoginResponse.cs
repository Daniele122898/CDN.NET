namespace CDN.NET.Wrapper.Dtos.User
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public Models.User User { get; set; }
    }
}