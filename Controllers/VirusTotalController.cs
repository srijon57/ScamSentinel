using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;

namespace ScamSentinel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VirusTotalController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public VirusTotalController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpPost("check")]
        public async Task<IActionResult> CheckUrl([FromBody] string siteUrl)
        {
            var apiKey = _configuration["VirusTotal:ApiKey"]; // stored in appsettings.json
            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Add("x-apikey", apiKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var content = new StringContent("url=" + Uri.EscapeDataString(siteUrl), Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await client.PostAsync("https://www.virustotal.com/api/v3/urls", content);
            var result = await response.Content.ReadAsStringAsync();

            return Content(result, "application/json");
        }
    }
}
