using Microsoft.AspNetCore.Mvc;
using ScamSentinel.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class ContactController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public ContactController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Index(ContactModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var formspreeUrl = _configuration["Formspree:Url"];
            var client = _httpClientFactory.CreateClient();

            var response = await client.PostAsync(formspreeUrl, new StringContent(
                JsonSerializer.Serialize(model),
                Encoding.UTF8,
                "application/json"
            ));

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Message sent successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to send message. Please try again.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An error occurred. Please try again.";
        }

        return View(model);
    }
}