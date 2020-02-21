using System;

namespace CDN.NET.Backend.Dtos.AlbumDtos
{
    public class AlbumsToReturnSparseDto
    {
        public int Id { get; set; }
        public bool IsPublic { get; set; }
        public string Name { get; set; }
        public DateTime DateAdded { get; set; }
        public int OwnerId { get; set; }
    }
}