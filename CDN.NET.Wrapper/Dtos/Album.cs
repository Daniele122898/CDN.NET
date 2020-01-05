using System;
using System.Collections.Generic;

namespace CDN.NET.Wrapper.Dtos
{
    public class Album
    {
        public int Id { get; set; }
        public bool IsPublic { get; set; }
        public string Name { get; set; }
        public DateTime DateAdded { get; set; }
        public ICollection<FileResponse> Files { get; set; }
        public int OwnerId { get; set; }
    }
}