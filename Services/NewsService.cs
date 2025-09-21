using NewsAPI;
using NewsAPI.Models;
using NewsAPI.Constants;
using ScamSentinel.Models;

namespace ScamSentinel.Services
{
    public class NewsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<NewsService> _logger;

        public NewsService(IConfiguration configuration, ILogger<NewsService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public NewsResponse GetScamNews()
        {
            try
            {
                var apiKey = _configuration["NewsApi:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    return new NewsResponse { Status = "error", ErrorMessage = "API key not configured" };
                }

                var client = new NewsApiClient(apiKey);

                var response = client.GetEverything(new EverythingRequest
                {
                    Q = "scam",
                    SortBy = SortBys.PublishedAt,
                    Language = Languages.EN,
                    From = DateTime.UtcNow.AddDays(-3) // last 3 days instead of just today

                });

                if (response.Status != Statuses.Ok)
                {
                    return new NewsResponse { Status = "error", ErrorMessage = "API request failed" };
                }

                return new NewsResponse
                {
                    Status = "ok",
                    TotalResults = response.TotalResults,
                    Articles = response.Articles.Select(a => new NewsArticle
                    {
                        Title = a.Title,
                        Author = a.Author,
                        Description = a.Description,
                        Url = a.Url,
                        PublishedAt = a.PublishedAt,
                        SourceName = a.Source?.Name
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching news");
                return new NewsResponse { Status = "error", ErrorMessage = "Unexpected error occurred." };
            }
        }
    }
}
