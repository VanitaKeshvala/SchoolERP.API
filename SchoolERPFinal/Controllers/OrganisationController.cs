using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Models;
using SchoolERP.Net.Services;
using SchoolERP.Net.Services.Clients;
using System.Threading.Tasks;

namespace SchoolERP.Net.Controllers
{
    /// <summary>
    /// This controller manages the organization settings, allowing you to set up and maintain the details of different school campuses or branches.
    /// </summary>
    public class OrganisationController : Controller
    {
        private readonly IOrganisationClientService _organisationClient;
        private readonly IUserMenuPermissionClientService _menuPerm;
        private const string MenuPath = "/Organisation";

        public OrganisationController(IOrganisationClientService organisationClient, IUserMenuPermissionClientService menuPerm)
        {
            _organisationClient = organisationClient;
            _menuPerm = menuPerm;
        }

        /// <summary>
        /// Shows the main list of all organizations or campuses registered in the system.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Step 1: Ask the system for a list of all school campuses or branches registered.
            var response = await _organisationClient.GetAllOrganisationsAsync();
            
            // Step 2: Prepare the list to be shown on the management page.
            var model = new OrganisationPageViewModel
            {
                Organisations = response.Success ? response.Data : new System.Collections.Generic.List<OrganisationViewModel>()
            };
            
            // Step 3: Open the 'Organisation' management page.
            return View(model);
        }

        /// <summary>
        /// Gets the details of a specific organization so you can view or edit its information.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetOrganisation(int id)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit organisations." });

            var response = await _organisationClient.GetOrganisationByIDAsync(id);
            if (!response.Success) return Json(new { success = false, message = response.Message });
            return Json(new { success = true, data = response.Data });
        }

        /// <summary>
        /// Saves a new organization's details or updates an existing one with the information you provided.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] OrganisationUpsertRequest request)
        {
            // Step 1: Check if the user has the right permission to add or edit an organisation based on whether it already exists.
            var isCreate = request.OrganisationID <= 0;
            if (isCreate && !(await _menuPerm.Has(MenuPath, "Add")).Data)
                return Json(new { success = false, message = "You do not have permission to add organisations." });
            if (!isCreate && !(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit organisations." });
 
            // Step 2: Send the new or updated organisation details to the backend system to be saved.
            var response = await _organisationClient.UpsertOrganisationAsync(request);
            
            // Step 3: Inform the user if the record was saved successfully.
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Turns an organization's active status on or off, determining if it's currently in use.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id, bool isActive)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to change organisation status." });

            var response = await _organisationClient.ToggleStatusAsync(id, isActive);
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Permanently removes an organization record from the system.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!(await _menuPerm.Has(MenuPath, "Delete")).Data)
                return Json(new { success = false, message = "You do not have permission to delete organisations." });

            var response = await _organisationClient.DeleteOrganisationAsync(id);
            return Json(new { success = response.Success, message = response.Message });
        }
    }
}
