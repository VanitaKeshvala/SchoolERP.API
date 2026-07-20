using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Services.Clients;

namespace SchoolERP.Net.Controllers
{
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public class SiteController : Controller
    {
        private readonly IAuthClientService _authClient;
        private readonly IConfiguration _configuration;

        public SiteController(IAuthClientService authClient, IConfiguration configuration)
        {
            _authClient = authClient;
            _configuration = configuration;
        }
        /// <summary>
        /// Shows the login screen where you enter your username and password.
        /// </summary>
        public IActionResult UserLogin()
        {
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"];
            // Simply return the default view without a model on first load.
            return View();
        }

    }
}
