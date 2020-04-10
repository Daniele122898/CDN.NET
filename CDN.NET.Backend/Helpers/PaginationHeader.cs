using System.Text.Json.Serialization;

namespace CDN.NET.Backend.Helpers
{
    public class PaginationHeader
    {
        [JsonPropertyName("currentPage")]
        public int CurrentPage { get; set; }
        
        [JsonPropertyName("itemsPerPage")]
        public int ItemsPerPage { get; set; }
        
        [JsonPropertyName("totalItems")]
        public int TotalItems { get; set; }
        
        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }
    }
}