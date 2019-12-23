using System.ComponentModel.DataAnnotations;

namespace CDN.NET.Backend.Dtos.AuthDtos
{
    public class UserForLoginDto
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}