using Microsoft.AspNetCore.Mvc;
using SchoolERP.Shared.Models;
using SchoolERP.Net.Services;
using SchoolERP.Net.Services.Clients;
using System.Threading.Tasks;

namespace SchoolERP.Net.Controllers
{
    /// <summary>
    /// This controller manages the different categories of users in the system (like 'Employee', 'Teacher', or 'Student').
    /// </summary>
    public class UserTypeController : Controller
    {
        private readonly IUserTypeClientService _userTypeClient;
        private readonly IUserMenuPermissionClientService _menuPerm;
        private const string MenuPath = "/UserType";

        public UserTypeController(IUserTypeClientService userTypeClient, IUserMenuPermissionClientService menuPerm)
        {
            _userTypeClient = userTypeClient;
            _menuPerm = menuPerm;
        }

        /// <summary>
        /// Shows the main list of all defined user categories.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var response = await _userTypeClient.GetAllAsync();
            var model = new MstUserManagementPageViewModel
            {
                UserTypes = response.Success ? response.Data : new System.Collections.Generic.List<MstUserTypeViewModel>()
            };
            return View(model);
        }

        /// <summary>
        /// Gets the details of a specific user category so you can view or edit its name.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUserType(int typeId)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit user types." });

            var response = await _userTypeClient.GetByIdAsync(typeId);
            if (!response.Success) return Json(new { success = false, message = response.Message });
            return Json(new { success = true, userType = response.Data });
        }

        /// <summary>
        /// Saves a new user category or updates an existing one with the name you provided.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] MstUserTypeUpsertRequest request)
        {
            var isCreate = request.UserTypeID <= 0;
            if (isCreate && !(await _menuPerm.Has(MenuPath, "Add")).Data)
                return Json(new { success = false, message = "You do not have permission to add user types." });
            if (!isCreate && !(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit user types." });

            var response = await _userTypeClient.SaveAsync(request);
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Turns a user category on or off, determining if it can be assigned to users.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int typeId, bool isActive)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to change user type status." });

            var response = await _userTypeClient.ToggleStatusAsync(typeId, isActive);
            return Json(new { success = response.Success, message = response.Message });
        }
    }
}
