using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CDN.NET.Backend.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public byte[] PasswordHash { get; set; }
        [Required]
        public byte[] PasswordSalt { get; set; }
        
        public virtual ApiKey ApiKey { get; set; }
        public virtual ICollection<UFile> Files { get; set; }
        
    }
}