using Microsoft.AspNetCore.Mvc;

namespace OnlineVotingApplication.Controllers
{
    public class FeaturesController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}