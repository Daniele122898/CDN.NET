using System.Collections.Generic;
using CDN.NET.Backend.Dtos.AlbumDtos;
using CDN.NET.Backend.Dtos.UploadDtos;

namespace CDN.NET.Backend.Dtos.UserDtos
{
    public class UserAdminReturnDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public bool IsAdmin { get; set; }
        public List<AlbumsToReturnSparseDto> Albums { get; set; }
        public List<UFileReturnDto> Files { get; set; }
    }
}