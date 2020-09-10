namespace CDN.NET.Backend.Helpers
{
    public class UploadSettings
    {
        public long MaxSize { get; set; }
        public int MaxFiles { get; set; }
        public string[] BlockedExtensions { get; set; }
        public bool Private { get; set; }
    }
}