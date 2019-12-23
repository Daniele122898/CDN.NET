namespace CDN.NET.Backend.Dtos.AuthDtos
{
    public class LoginReturnDto
    {
        public string Token { get; set; }
        public UserDetailDto User { get; set; }
    }
}