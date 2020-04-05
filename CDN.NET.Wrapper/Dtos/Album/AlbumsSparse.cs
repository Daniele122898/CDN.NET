using System;

namespace CDN.NET.Wrapper.Dtos.Album
{
    public class AlbumsSparse
    {
        public int Id { get; set; }
        public bool IsPublic { get; set; }
        public string Name { get; set; }
        public DateTime DateAdded { get; set; }
        public int OwnerId { get; set; }
    }
}