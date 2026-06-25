using Microsoft.AspNetCore.Mvc;
using SchoolERP.Shared.Models;
using SchoolERP.Net.Services.Clients;
using System.Threading.Tasks;

namespace SchoolERP.Net.Controllers
{
    /// <summary>
    /// This controller manages the door to the application, handling when users try to log in or log out.
    /// </summary>
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly IAuthClientService _authClient;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthClientService authClient, IConfiguration configuration)
        {
            _authClient = authClient;
            _configuration = configuration;
        }

        /// <summary>
        /// Shows the login screen where you enter your username and password.
        /// </summary>
        public IActionResult Login()
        {
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"];
            // Simply return the default view without a model on first load.
            return View();
        }

        /// <summary>
        /// Takes the username and password you entered, checks if they are correct, and lets you into the system if they match.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Ensure the data annotations on the LoginRequest model are valid before making a network call.
            if (!ModelState.IsValid) return View(request);

            // Execute the backend API or service routine to validate credentials.
            var response = await _authClient.LoginAsync(request);

            // Check if the service returned a successful validation flag.
            if (response.Success)
            {
                string redirectUrl = "/Dashboard/Index"; // default fallback
                                                         // In a real application, the authentication cookie or JWT would be materialized here 
                                                         // e.g. HttpContext.SignInAsync(...)

                if (response.Data.DashboardURL != null) 
                {
                    redirectUrl = response.Data.DashboardURL;
                }
                // Route the successfully authenticated user into the secure system layout.
                return Json(new
                {
                    success = true,
                    message = response.Message,
                    redirectUrl = redirectUrl,
                    data = response.Data
                });
            }

            // Validation failed. Pass the failure string back to the user interface natively.
            ViewBag.ErrorMessage = response.Message;
            return View(request);
        }

        /// <summary>
        /// Logs you out of the application and clears your session so you can safely leave the computer.
        /// </summary>
        public IActionResult Logout()
        {
            // Step 1: Tell the browser to throw away the 'token' and 'CurrentSessionId' which were like your temporary access badges.
            Response.Cookies.Delete("token");
            Response.Cookies.Delete("CurrentSessionId");

            // Step 2: Show a tiny logout screen that also cleans up any remaining traces from the browser's memory.
            return View();
        }
    }
}
