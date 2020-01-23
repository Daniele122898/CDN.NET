namespace CDN.NET.Backend.Helpers
{
    public class AppSettings
    {
        public string Token { get; set; }
        public string BaseUrl { get; set; }
        public bool Private { get; set; }
        public int DaysUntilTokenExpiration { get; set; }
    }
}