using Microsoft.AspNetCore.Mvc;
using SchoolERP.Shared.Models;
using SchoolERP.Net.Services.Clients;
using System.Threading.Tasks;

namespace SchoolERP.Net.Controllers
{
    /// <summary>
    /// This controller manages company-related UI actions, such as switching the current active company.
    /// </summary>
    public class CompanyController : Controller
    {
        private readonly ICompanyClientService _companyClient;

        public CompanyController(ICompanyClientService companyClient)
        {
            _companyClient = companyClient;
        }

        /// <summary>
        /// Sets the chosen school company as the 'active' one for the current user's session.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SetCurrent([FromBody] SetCurrentCompanyRequest request)
        {
            if (request == null || request.CompanyId <= 0)
                return Json(new { success = false, message = "Invalid company selection." });

            var response = await _companyClient.SetCurrentCompanyAsync(request);
            return Json(new { success = response.Success, message = response.Message });
        }

        [HttpGet]
        public async Task<IActionResult> GetAssignedCompanies()
        {
            var response = await _companyClient.GetAssignedCompaniesAsync();
            return Json(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserCurrentCompany()
        {
            var response = await _companyClient.GetUserCurrentCompanyAsync();
            return Json(new { success = true, data = response.Data });
        }
    }
}
