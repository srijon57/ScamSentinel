using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using ScamSentinel.Models;

namespace ScamSentinel.Services
{
    public class VirusTotalService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public VirusTotalService(IConfiguration config, HttpClient httpClient)
        {
            _apiKey = config["VirusTotal:ApiKey"] ?? throw new ArgumentNullException("VirusTotal:ApiKey missing");
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-apikey", _apiKey);
        }

        // Base64 URL-safe encoding without padding (for GET /api/v3/urls/{id})
        private string UrlToVirusTotalId(string url)
        {
            var bytes = Encoding.UTF8.GetBytes(url);
            var b64 = Convert.ToBase64String(bytes) // standard base64
                        .TrimEnd('=')               // remove padding
                        .Replace('+', '-')         // URL-safe
                        .Replace('/', '_');
            return b64;
        }

        // Submit the URL for scanning (optional step, helps get fresh analysis)
        private async Task SubmitUrlAsync(string url)
        {
            try
            {
                var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("url", url) });
                // POST to /api/v3/urls triggers analysis (non-blocking)
                var resp = await _httpClient.PostAsync("https://www.virustotal.com/api/v3/urls", content);
                // We don't require the response body here; it's okay if submission fails.
            }
            catch
            {
                // swallow - we will still try to fetch existing analysis
            }
        }

        public async Task<ScamCheckResponse> CheckUrlAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("url");

            // Normalize: ensure scheme present
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                url = "http://" + url;

            // 1) Ask VirusTotal to analyze (optional)
            await SubmitUrlAsync(url);

            // 2) Build id and GET analysis summary
            var id = UrlToVirusTotalId(url);
            var getUrl = $"https://www.virustotal.com/api/v3/urls/{id}";

            // Try a few times to account for short delay after submission
            JsonDocument? json = null;
            for (int attempt = 0; attempt < 4; attempt++)
            {
                try
                {
                    var resp = await _httpClient.GetAsync(getUrl);
                    var body = await resp.Content.ReadAsStringAsync();

                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        json = JsonDocument.Parse(body);
                        // If it contains data.attributes.last_analysis_stats then break
                        if (json.RootElement.TryGetProperty("data", out var data)
                            && data.TryGetProperty("attributes", out var attributes)
                            && attributes.TryGetProperty("last_analysis_stats", out var stats))
                        {
                            // we have stats — break
                            break;
                        }
                    }
                }
                catch
                {
                    // ignore and retry
                }

                // small delay between retries
                await Task.Delay(1000);
            }

            if (json == null)
            {
                return new ScamCheckResponse
                {
                    IsFraud = false,
                    Message = "Could not retrieve VirusTotal report.",
                    Raw = null
                };
            }

            // Parse stats
            try
            {
                var data = json.RootElement.GetProperty("data");
                var attributes = data.GetProperty("attributes");
                var stats = attributes.GetProperty("last_analysis_stats");

                int malicious = stats.TryGetProperty("malicious", out var m) ? m.GetInt32() : 0;
                int suspicious = stats.TryGetProperty("suspicious", out var s) ? s.GetInt32() : 0;
                int undetected = stats.TryGetProperty("undetected", out var u) ? u.GetInt32() : 0;
                int harmless = stats.TryGetProperty("harmless", out var h) ? h.GetInt32() : 0;

                bool isFraud = (malicious + suspicious) > 0;

                var message = isFraud
                    ? $"⚠️ VirusTotal detected {malicious} malicious / {suspicious} suspicious engines."
                    : "✅ No engines flagged this URL (VirusTotal).";

                // optional: include a small summary of analysis stats
                var rawSummary = new
                {
                    malicious,
                    suspicious,
                    undetected,
                    harmless
                };

                return new ScamCheckResponse
                {
                    IsFraud = isFraud,
                    Message = message,
                    Raw = rawSummary
                };
            }
            catch
            {
                return new ScamCheckResponse
                {
                    IsFraud = false,
                    Message = "Failed to parse VirusTotal response.",
                    Raw = json.RootElement.ToString()
                };
            }
        }
    }
}
