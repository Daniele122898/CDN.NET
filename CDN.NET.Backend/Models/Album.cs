using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CDN.NET.Backend.Models
{
    public class Album
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public bool IsPublic { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public DateTime DateAdded { get; set; }

        public Album(bool isPublic, int ownerId, string name = null)
        {
            IsPublic = isPublic;
            OwnerId = ownerId;
            Name = string.IsNullOrWhiteSpace(name) ? Guid.NewGuid().ToString() : name;
            DateAdded = DateTime.UtcNow;
        }

        public virtual ICollection<UFile> UFiles { get; set; }
        public int OwnerId { get; set; }
        public virtual User Owner { get; set; }
    }
}