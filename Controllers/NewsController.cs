using Microsoft.AspNetCore.Mvc;
using ScamSentinel.Services;

namespace ScamSentinel.Controllers
{
    public class NewsController : Controller
    {
        private readonly NewsService _newsService;

        public NewsController(NewsService newsService)
        {
            _newsService = newsService;
        }

        [Route("/scam-news")]
        public IActionResult Index()
        {
            var news = _newsService.GetScamNews();
            return View(news);
        }
    }
}
