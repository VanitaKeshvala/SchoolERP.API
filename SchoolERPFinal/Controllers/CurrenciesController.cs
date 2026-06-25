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
    /// This controller manages the currency settings page, where you can see, add, or change the different types of money used in the system.
    /// </summary>
    public class CurrenciesController : BaseController
    {
        private readonly ICurrencyClientService _currencyClient;
        private readonly IUserMenuPermissionClientService _menuPerm;
        private const string MenuPath = "/Currencies";

        public CurrenciesController(ICurrencyClientService currencyClient, IUserMenuPermissionClientService menuPerm, PermissionHelper permHelper) : base(permHelper)
        {
            _currencyClient = currencyClient;
            _menuPerm = menuPerm;
        }

        /// <summary>
        /// Shows the main list of all currencies available in the system.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Currencies"
               );

                // Step 1: Ask the system for a list of all different currencies (types of money) set up.
                var response = await _currencyClient.GetAllAsync();

                // Step 2: Prepare the data to be shown on the screen.
                var model = new MstCurrencyPageViewModel
                {
                    Currencies = response.Success ? response.Data : new List<MstCurrencyViewModel>()
                };
                model.Permissions = perms;
                // Step 3: Open the 'Currencies' management page.
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        /// <summary>
        /// Gets the details of one specific currency so you can view or edit it.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCurrency(int id)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit currencies." });

            var response = await _currencyClient.GetByIDAsync(id);
            if (!response.Success) return Json(new { success = false, message = response.Message });
            return Json(new { success = true, data = response.Data });
        }

        /// <summary>
        /// Saves a new currency or updates an existing one with the information you provided.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] MstCurrencyUpsertRequest request)
        {
            // Step 1: Check if the user is allowed to add or edit currencies based on whether the currency already exists.
            var isCreate = request.CurrencyId <= 0;
            if (isCreate && !(await _menuPerm.Has(MenuPath, "Add")).Data)
                return Json(new { success = false, message = "You do not have permission to add currencies." });
            if (!isCreate && !(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit currencies." });

            // Step 2: Send the new currency details to the backend system to be saved.
            var response = await _currencyClient.UpsertAsync(request);

            // Step 3: Inform the user if the record was saved successfully.
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Turns a currency on or off, making it active or inactive in the system.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id, bool isActive)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to change currency status." });

            var response = await _currencyClient.ToggleStatusAsync(id, isActive);
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Permanently removes a currency from the system's records.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] List<int> id)
        {
            if (!(await _menuPerm.Has(MenuPath, "Delete")).Data)
                return Json(new { success = false, message = "You do not have permission to delete currencies." });

            var response = await _currencyClient.DeleteAsync(id);
            return Json(new { success = response.Success, message = response.Message });
        }
    }
}
