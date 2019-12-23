using System;

namespace CDN.NET.Backend.Dtos.UploadDtos
{
    public class UFileReturnDto
    {
        public int Id { get; set; }
        public string PublicId { get; set; }
        public bool IsPublic { get; set; }
        public string FileExtension { get; set; }
        public string ContentType { get; set; }
        public string Name { get; set; }
        public DateTime DateAdded { get; set; }
        public int OwnerId { get; set; }
        public int? AlbumId { get; set; }
        public string Url { get; set; }
    }
}