using Microsoft.AspNetCore.Mvc;
using SchoolERP.Shared.Models;
using SchoolERP.Net.Services;
using SchoolERP.Net.Services.Clients;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Net.Helpers;

namespace SchoolERP.Net.Controllers
{
    /// <summary>
    /// This controller manages the languages available in the system, letting you add new ones or change existing ones.
    /// </summary>
    public class LanguageController : BaseController
    {
        private readonly ILanguageClientService _languageClient;
        private readonly IUserMenuPermissionClientService _menuPerm;
        private const string MenuPath = "/Language";

        public LanguageController(ILanguageClientService languageClient, IUserMenuPermissionClientService menuPerm, PermissionHelper permHelper) : base(permHelper)
        {
            _languageClient = languageClient;
            _menuPerm = menuPerm;
        }

        /// <summary>
        /// Shows the main list of all languages that the system currently supports.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Language"
               );
                // Step 1: Ask the system for a list of all supported languages.
                var response = await _languageClient.GetAllAsync();

                // Step 2: Prepare the data to be shown on the language management page.
                var model = new MstLanguagePageViewModel
                {
                    Languages = response.Success ? response.Data : new List<MstLanguageViewModel>()
                };
                model.Permissions = perms;
                // Step 3: Open the 'Language' settings page.
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        /// <summary>
        /// Gets the details of a specific language so you can see its settings or edit it.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLanguage(int id)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit languages." });

            var response = await _languageClient.GetByIDAsync(id);
            if (!response.Success) return Json(new { success = false, message = response.Message });
            return Json(new { success = true, data = response.Data });
        }

        /// <summary>
        /// Saves a new language or updates an existing one with the details you provided.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] MstLanguageUpsertRequest request)
        {
            // Step 1: Check if the user is allowed to add or edit languages based on whether it already exists.
            var isCreate = request.LanguageId <= 0;
            if (isCreate && !(await _menuPerm.Has(MenuPath, "Add")).Data)
                return Json(new { success = false, message = "You do not have permission to add languages." });
            if (!isCreate && !(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit languages." });
 
            // Step 2: Send the new language details to the backend system to be saved.
            var response = await _languageClient.UpsertAsync(request);
            
            // Step 3: Inform the user if the record was saved successfully.
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Turns a language on or off, determining if it can be used in the system.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id, bool isActive)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to change language status." });

            var response = await _languageClient.ToggleStatusAsync(id, isActive);
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Permanently removes a language from the system's records.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody]  List<int> id)
        {
            if (!(await _menuPerm.Has(MenuPath, "Delete")).Data)
                return Json(new { success = false, message = "You do not have permission to delete languages." });

            var response = await _languageClient.DeleteAsync(id);
            return Json(new { success = true, message = response.Message });
        }
    }
}
