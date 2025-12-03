using Microsoft.AspNetCore.Mvc;

namespace SimsWeb.Controllers
{
    public class SubmissionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
