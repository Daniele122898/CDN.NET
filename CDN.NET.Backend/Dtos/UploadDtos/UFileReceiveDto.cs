using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CDN.NET.Backend.Dtos.UploadDtos
{
    public class UFileReceiveDto
    {
        public bool IsPublic { get; set; } = true;
        public string Name { get; set; }
        [Required]
        public IFormFile File { get; set; }

        public int? AlbumId { get; set; }

    }
}