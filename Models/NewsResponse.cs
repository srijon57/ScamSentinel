using System.Collections.Generic;

namespace ScamSentinel.Models
{
    public class NewsResponse
    {
        public string Status { get; set; } = "ok";
        public int TotalResults { get; set; }
        public List<NewsArticle> Articles { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }
}
