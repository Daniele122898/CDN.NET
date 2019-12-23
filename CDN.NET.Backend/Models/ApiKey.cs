using System.ComponentModel.DataAnnotations;

namespace CDN.NET.Backend.Models
{
    public class ApiKey
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Key { get; set; }
        [Required]
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}