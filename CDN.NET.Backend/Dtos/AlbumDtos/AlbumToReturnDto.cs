using System;
using System.Collections.Generic;
using CDN.NET.Backend.Dtos.UploadDtos;

namespace CDN.NET.Backend.Dtos.AlbumDtos
{
    public class AlbumToReturnDto
    {
        public int Id { get; set; }
        public bool IsPublic { get; set; }
        public string Name { get; set; }
        public DateTime DateAdded { get; set; }
        public virtual ICollection<UFileReturnDto> Files { get; set; }
        public int OwnerId { get; set; }
    }
}