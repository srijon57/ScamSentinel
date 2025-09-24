using Microsoft.AspNetCore.Mvc;
using ScamSentinel.Services;

namespace ScamSentinel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VirusTotalController : ControllerBase
    {
        private readonly VirusTotalService _service;

        public VirusTotalController(VirusTotalService service)
        {
            _service = service;
        }

        [HttpPost("check")]
        public async Task<IActionResult> CheckUrl([FromBody] string siteUrl)
        {
            var result = await _service.CheckUrlAsync(siteUrl);
            return Ok(result);
        }
    }
}
