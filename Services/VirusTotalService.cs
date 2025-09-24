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
            _apiKey = config["VirusTotal:ApiKey"]
                      ?? throw new ArgumentNullException("VirusTotal:ApiKey missing");
            _httpClient = httpClient;
        }

        // Base64 URL-safe encoding without padding (for GET /api/v3/urls/{id})
        private string UrlToVirusTotalId(string url)
        {
            var bytes = Encoding.UTF8.GetBytes(url);
            var b64 = Convert.ToBase64String(bytes)
                        .TrimEnd('=')
                        .Replace('+', '-')
                        .Replace('/', '_');
            return b64;
        }

        // Submit the URL for scanning (optional step, helps get fresh analysis)
        private async Task SubmitUrlAsync(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.virustotal.com/api/v3/urls");
            request.Headers.Add("x-apikey", _apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("url", url)
            });

            try
            {
                await _httpClient.SendAsync(request);
            }
            catch
            {
                // ignore errors – analysis might still be available
            }
        }

        public async Task<ScamCheckResponse> CheckUrlAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("url");

            // Normalize: ensure scheme present
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                url = "http://" + url;

            // Step 1: ask VirusTotal to analyze (non-blocking)
            await SubmitUrlAsync(url);

            // Step 2: GET analysis summary
            var id = UrlToVirusTotalId(url);
            var getUrl = $"https://www.virustotal.com/api/v3/urls/{id}";

            JsonDocument? json = null;

            // retry a few times, since analysis can take a second
            for (int attempt = 0; attempt < 4; attempt++)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, getUrl);
                request.Headers.Add("x-apikey", _apiKey);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var resp = await _httpClient.SendAsync(request);
                var body = await resp.Content.ReadAsStringAsync();

                if (!string.IsNullOrWhiteSpace(body))
                {
                    json = JsonDocument.Parse(body);

                    if (json.RootElement.TryGetProperty("data", out var data) &&
                        data.TryGetProperty("attributes", out var attributes) &&
                        attributes.TryGetProperty("last_analysis_stats", out var stats))
                    {
                        break; // got valid stats
                    }
                }

                await Task.Delay(1000); // wait before retry
            }

            if (json == null)
            {
                return new ScamCheckResponse
                {
                    IsFraud = false,
                    Message = "❌ Could not retrieve VirusTotal report.",
                    Raw = null
                };
            }

            // Step 3: parse stats
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
                    ? $"⚠️ ScamSentinel flagged this site ({malicious} malicious, {suspicious} suspicious)."
                    : "✅ Safe: no engines flagged this URL.";

                return new ScamCheckResponse
                {
                    IsFraud = isFraud,
                    Message = message,
                    Raw = new { malicious, suspicious, undetected, harmless }
                };
            }
            catch
            {
                return new ScamCheckResponse
                {
                    IsFraud = false,
                    Message = "❌ Failed to parse VirusTotal response.",
                    Raw = json.RootElement.ToString()
                };
            }
        }
    }
}
