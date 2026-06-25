using Microsoft.AspNetCore.Mvc;
using SchoolERP.Shared.Models;
using SchoolERP.Net.Services;
using SchoolERP.Net.Services.Clients;
using System.Threading.Tasks;
using SchoolERP.Shared.Models.Common;
using SchoolERP.Net.Helpers;

namespace SchoolERP.Net.Controllers
{
    /// <summary>
    /// This controller manages user roles (like 'Admin' or 'Staff') and controls what each role is allowed to see and do in the system.
    /// </summary>
    public class RoleController : BaseController
    {
        private readonly IRoleClientService _roleClient;
        private readonly IDashboardClientService _dashboardClient;
        private readonly IUserMenuPermissionClientService _menuPerm;
        private const string MenuPath = "/Role";

        public RoleController(IRoleClientService roleClient, IDashboardClientService dashboardClient, IUserMenuPermissionClientService menuPerm, PermissionHelper permHelper) : base(permHelper)
        {
            _roleClient = roleClient;
            _dashboardClient = dashboardClient;
            _menuPerm = menuPerm;
        }

        /// <summary>
        /// Shows the main page with a list of all defined user roles.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/StudentInformation/DisabledStudents"
               );
                var response = await _roleClient.GetAllRolesAsync();
                var model = new MstUserManagementPageViewModel
                {
                    Roles = response.Success ? response.Data : new System.Collections.Generic.List<MstRoleViewModel>()
                };
                model.PermissionsButton = perms;
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
           
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

        /// <summary>
        /// Shows the main page with a list of all defined user roles.
        /// </summary>
        
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var response = await _dashboardClient.GetAllAsync();

                var model = new DashboardPageViewModel
                {
                    Dashboard = response.Success ? response.Data : new List<DashboardModel>()
                };
                return View(model);
            }
            catch (Exception ex)
            {
                throw ex;
            }            
        }

        public async Task<IActionResult> AddDashboard(int? id) 
        {
            try
            {
                // Step 1: Initialize a new blank page model.
                var model = new DashboardViewModel();

                var rolesRes = await _roleClient.GetAllRolesAsync();
                model.Roles = rolesRes.Success ? rolesRes.Data : new List<MstRoleViewModel>();
                // Step 3: If we are editing an existing person (ID is provided), fetch their details.

                if (id.HasValue && id.Value > 0)
                {
                    var dashboardRes = await _dashboardClient.GetByIdAsync(id.Value);
                    if (dashboardRes.Success)
                    {
                        model.EditDashboard = dashboardRes.Data;
                    }
                }
                return View(model);
            }
            catch (Exception ex)
            {
                throw ex;
            }           
        }

        [HttpPost]
        public async Task<IActionResult> SaveDashboard([FromBody] DashboardRequestModel request)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to update role permissions." });

            var response = await _dashboardClient.SaveAsync(request);
            return Json(new { success = response.Success, message = response.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDashboard([FromBody] List<int> id)
        {
            var res = await _dashboardClient.DeleteMultipleAsync(id);
            return Json(new { success = true, message = res.Message });
        }


        /// <summary>
        /// Turns a company's active status on or off.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ToggleDashboardStatus([FromBody] StatusUpdateRequest request)
        {
            
            var response = await _dashboardClient.ToggleStatusAsync(request);
            return Json(new { success = response.Success, message = response.Message });
        }

        [HttpGet]
        public async Task<DashboardModel> GetByRoleId(int roleId)
        {
            try
            {
                var dashboardRes = await _dashboardClient.GetByRoleIdAsync(roleId);
                return dashboardRes.Data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
