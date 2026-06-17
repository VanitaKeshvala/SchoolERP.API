using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Models;
using SchoolERP.Net.Services;
using SchoolERP.Net.Services.Clients;
using System.Threading.Tasks;

namespace SchoolERP.Net.Controllers
{
    /// <summary>
    /// This controller manages user roles (like 'Admin' or 'Staff') and controls what each role is allowed to see and do in the system.
    /// </summary>
    public class RoleController : Controller
    {
        private readonly IRoleClientService _roleClient;
        private readonly IUserMenuPermissionClientService _menuPerm;
        private const string MenuPath = "/Role";

        public RoleController(IRoleClientService roleClient, IUserMenuPermissionClientService menuPerm)
        {
            _roleClient = roleClient;
            _menuPerm = menuPerm;
        }

        /// <summary>
        /// Shows the main page with a list of all defined user roles.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var response = await _roleClient.GetAllRolesAsync();
            var model = new MstUserManagementPageViewModel
            {
                Roles = response.Success ? response.Data : new System.Collections.Generic.List<MstRoleViewModel>()
            };
            return View(model);
        }

        /// <summary>
        /// Gets the details of a specific role so you can view or edit its basic information.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRole(int roleId)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit roles." });

            var response = await _roleClient.GetRoleByIdAsync(roleId);

            if (!response.Success) return Json(new { success = false, message = response.Message });

            return Json(new { success = true, role = response.Data });
        }

        /// <summary>
        /// Saves a new role or updates an existing one with the name and details you provided.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] MstRoleUpsertRequest request)
        {
            var isCreate = request.RoleID <= 0;
            if (isCreate && !(await _menuPerm.Has(MenuPath, "Add")).Data)
                return Json(new { success = false, message = "You do not have permission to add roles." });
            if (!isCreate && !(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit roles." });

            var response = await _roleClient.SaveRoleAsync(request);

            return Json(new { success = response.Success, message = response.Message, roleId = response.Data });
        }

        /// <summary>
        /// Turns a role on or off, making it available or unavailable for users.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int roleId, bool isActive)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to change role status." });

            var response = await _roleClient.ToggleStatusAsync(roleId, isActive);
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Gets the list of menu items and actions that a specific role is allowed to access.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPermissions(int roleId)
        {
            if (!(await _menuPerm.Has(MenuPath, "View")).Data)
                return Json(new { success = false, message = "You do not have permission to view role permissions." });

            var response = await _roleClient.GetPermissionsAsync(roleId);
            if (!response.Success) return Json(new { success = false, message = response.Message });

            return Json(new { success = true, data = response.Data ?? new System.Collections.Generic.List<RoleMenuPermissionViewModel>() });
        }

        /// <summary>
        /// Saves the chosen list of allowed actions for a role, defining what they can do in the application.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SavePermissions([FromBody] MstRolePermissionSaveRequest request)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to update role permissions." });

            var response = await _roleClient.SavePermissionsAsync(request);
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Permanently removes a role from the system's records.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody]  List<int> roleId)
        {
            if (!(await _menuPerm.Has(MenuPath, "Delete")).Data)
                return Json(new { success = false, message = "You do not have permission to delete roles." });

            var response = await _roleClient.DeleteRoleAsync(roleId);
            return Json(new { success = response.Success, message = response.Message });
        }
    }
}
