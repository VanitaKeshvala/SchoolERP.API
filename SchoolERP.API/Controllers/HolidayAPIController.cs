using Microsoft.AspNetCore.Mvc;

namespace SchoolERP.API.Controllers
{
    public class HolidayAPIController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
