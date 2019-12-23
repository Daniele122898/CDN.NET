using System.ComponentModel.DataAnnotations;

namespace CDN.NET.Backend.Dtos.AuthDtos
{
    public class UserForRegisterDto
    {
        [Required]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "You must specify a username between 3 and 20 characters long")]
        public string Username { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "You must specify a password between 4 and 20 characters long")]
        public string Password { get; set; }
    }
}