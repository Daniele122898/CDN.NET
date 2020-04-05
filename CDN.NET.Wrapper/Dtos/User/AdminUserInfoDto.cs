using System.Collections.Generic;
using CDN.NET.Wrapper.Dtos.Album;
using CDN.NET.Wrapper.Dtos.File;

namespace CDN.NET.Wrapper.Dtos.User
{
    public class AdminUserInfoDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public bool IsAdmin { get; set; }
        public List<AlbumsSparse> Albums { get; set; }
        public List<FileResponse> Files { get; set; }
    }
}