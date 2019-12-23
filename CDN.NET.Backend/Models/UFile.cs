using System;
using System.ComponentModel.DataAnnotations;

namespace CDN.NET.Backend.Models
{
    public class UFile
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string PublicId { get; set; }
        [Required]
        public bool IsPublic { get; set; }
        [Required]
        public string FileExtension { get; set; }
        [Required]
        public string ContentType { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public DateTime DateAdded { get; set; }


        public UFile(string publicId, bool isPublic, string fileExtension, string contentType,int ownerId, string name = null)
        {
            PublicId = publicId;
            IsPublic = isPublic;
            FileExtension = fileExtension;
            ContentType = contentType;
            OwnerId = ownerId;
            Name = string.IsNullOrWhiteSpace(name) ? publicId : name;
            DateAdded = DateTime.UtcNow;
        }
        public int? AlbumId { get; set; }
        public virtual Album Album { get; set; }
        public int OwnerId { get; set; }
        public virtual User Owner { get; set; }
    }
}