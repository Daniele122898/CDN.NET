namespace CDN.NET.Backend.Dtos.AlbumDtos
{
    public class CreateAlbumDto
    {
        public bool IsPublic { get; set; } = true;
        public string Name { get; set; }
    }
}