using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CDN.NET.Backend.Dtos.UploadDtos
{
    public class UFilesReceiveDto
    {
        public string Infos { get; set; }
        [Required]
        public List<IFormFile> Files { get; set; }

        public int? AlbumId { get; set; }
        
    }

    public class MultiFileInfoDto
    {
        public string Name { get; set; }
        public bool IsPublic { get; set; } = true;
    }
}