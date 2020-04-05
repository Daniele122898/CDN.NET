using System.ComponentModel.DataAnnotations;
using CDN.NET.Backend.Helpers;

namespace CDN.NET.Backend.Dtos.UserDtos
{
    [AtLeastOneProperty(ErrorMessage = "Nothing to update since no value has been passed")]
    public class UserAdminEditDto
    {
        [StringLength(20, MinimumLength = 3, ErrorMessage = "You must specify a username between 3 and 20 characters long")]
        public string Username { get; set; }
        
        [StringLength(20, MinimumLength = 4, ErrorMessage = "You must specify a password between 4 and 20 characters long")]
        public string Password { get; set; }

        public bool? IsAdmin { get; set; }
    }
}