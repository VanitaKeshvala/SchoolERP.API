using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Services.Clients;

namespace SchoolERP.Net.Controllers
{
    /// <summary>
    /// This controller manages the main dashboard, which gives you a quick overview of the whole system's status.
    /// </summary>
    public class DashboardController : Controller
    {
        private readonly IUtilityClientService _utilityClient;
        private readonly ICompanyClientService _companyClient;

        public DashboardController(IUtilityClientService utilityClient, ICompanyClientService companyClient)
        {
            _utilityClient = utilityClient;
            _companyClient = companyClient;
        }

        /// <summary>
        /// Shows the main dashboard page, providing high-level summaries like the total number of companies registered.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Step 1: Ask the system for a list of all schools or companies registered.
            var companiesResponse = await _companyClient.GetAllAsync();
            
            // Step 2: Count how many companies were found and save that number to show on the dashboard screen.
            ViewBag.totalCompanies = companiesResponse.Data.Count();
            
            // Step 3: Open the dashboard page for the user to see.
            return View();
        }
    }
}
