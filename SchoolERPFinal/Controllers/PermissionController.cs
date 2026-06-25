using Microsoft.AspNetCore.Mvc;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using SchoolERP.Net.Services;
using SchoolERP.Net.Services.Clients;
using SchoolERP.Net.Helpers;

namespace SchoolERP.Net.Controllers
{
    /// <summary>
    /// This controller manages the different types of actions users are allowed to perform (like 'Add', 'Edit', or 'Delete') across the system.
    /// </summary>
    public class PermissionController : BaseController
    {
        private readonly IUserManagementClientService _userMgmtService;
        private readonly IUserMenuPermissionClientService _menuPerm;
        private const string MenuPath = "/Permission";

        public PermissionController(IUserManagementClientService userMgmtService, IUserMenuPermissionClientService menuPerm, PermissionHelper permHelper) : base(permHelper)
        {
            _userMgmtService = userMgmtService;
            _menuPerm = menuPerm;
        }

        /// <summary>
        /// Shows the main page with a list of all defined permissions.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Permission"
               );
                var permissionsResponse = await _userMgmtService.GetAllPermissionsAsync();

                var model = new MstUserManagementPageViewModel
                {
                    Permissions = permissionsResponse?.Data ?? new List<MstPermissionViewModel>()
                };
                model.PermissionsButton = perms;
                return View(model);
            }
            catch (Exception ex)
            {
                throw;
            }
            
        }

        /// <summary>
        /// Gets the details of a specific permission type so you can view or edit it.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPermission(int permissionId)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit permissions." });

            var permission =(await _userMgmtService.GetPermissionByIDAsync(permissionId)).Data;
            if (permission == null) return Json(new { success = false, message = "Permission not found" });
            return Json(new { success = true, permission = permission });
        }

        /// <summary>
        /// Saves a new permission type or updates an existing one with the details you provided.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] MstPermissionUpsertRequest request)
        {
            var isCreate = request.PermissionID <= 0;
            if (isCreate && !(await _menuPerm.Has(MenuPath, "Add")).Data)
                return Json(new { success = false, message = "You do not have permission to add permissions." });
            if (!isCreate && !(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit permissions." });

            var userId = (await _menuPerm.GetCurrentUserIdAsync()).Data;
            if (userId <= 0)
                return Json(new { success = false, message = "You must be signed in to save." });

            var result =(await _userMgmtService.UpsertPermissionAsync(request)).Data;
            return Json(new { success = result.success, message = result.message });
        }

        /// <summary>
        /// Turns a specific permission type on or off, determining if it can be assigned to roles.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ToggleStatus([FromBody] StatusUpdateRequest request)
        {
            try
            {
                if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "You do not have permission to change permission status." });

                var userId = (await _menuPerm.GetCurrentUserIdAsync()).Data;
                if (userId <= 0)
                    return Json(new { success = false, message = "You must be signed in." });

                var result = await _userMgmtService.TogglePermissionStatusAsync(request);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            
        }

        /// <summary>
        /// Permanently removes a permission type from the system's records.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] List<int> permissionId)
        {
            if (!(await _menuPerm.Has(MenuPath, "Delete")).Data)
                return Json(new { success = false, message = "You do not have permission to delete permissions." });

            var userId = (await _menuPerm.GetCurrentUserIdAsync()).Data;
            if (userId <= 0)
                return Json(new { success = false, message = "You must be signed in." });

            var result =await _userMgmtService.DeletePermissionAsync(permissionId);
            return Json(new { success = result.Success, message = result.Message });
        }
    }
}
