using Microsoft.AspNetCore.Mvc;

namespace SchoolERP.Net.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
