using Microsoft.AspNetCore.Mvc;
using SchoolERP.Shared.Models;
using SchoolERP.Net.Services;
using SchoolERP.Net.Services.Clients;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Net.Helpers;

namespace SchoolERP.Net.Controllers
{
    /// <summary>
    /// This controller manages all user accounts in the school, allowing you to create staff, teacher, or student login accounts.
    /// </summary>
    public class UserController : BaseController
    {
        private readonly IUserClientService _userClient;
        private readonly ICompanyClientService _companyClient;
        private readonly IUserMenuPermissionClientService _menuPerm;
        private const string MenuPath = "/User";

        public UserController(IUserClientService userClient, ICompanyClientService companyClient, IUserMenuPermissionClientService menuPerm, PermissionHelper permHelper) : base(permHelper)
        {
            _userClient = userClient;
            _companyClient = companyClient;
            _menuPerm = menuPerm;
        }

        /// <summary>
        /// Shows the main list of all users registered in the system.
        /// </summary>
        
        public async Task<IActionResult> Index(
        int pageIndex ,
        int pageSize ,
        string? search,
        int? companyId,
        bool? isActive,
        int? userTypeId)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/User"
               );
                var request = new UserSearchRequest
                {
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    search = search,
                    companyId = companyId,
                    isActive = isActive,
                    userTypeId = userTypeId
                };

                // ── Parallel fetch — dropdowns + paged users ─────────────────
                var usersTask = _userClient.GetAllUsersWithPaginationAsync(request);
                var rolesTask = _userClient.GetRolesDropdownAsync();
                var typesTask = _userClient.GetUserTypesDropdownAsync();
                var companiesTask = _companyClient.GetAllAsync();

                await Task.WhenAll(usersTask, rolesTask, typesTask, companiesTask);

                var pagedResult = await usersTask;

                var model = new UsersPageViewModel
                {
                    Users = pagedResult.Data.Data,
                    TotalRecords = pagedResult.Data.TotalRecords,
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,   

                    Roles = (await rolesTask).Data ?? new(),
                    UserTypes = (await typesTask).Data ?? new(),
                    Companies = (await companiesTask).Data ?? new(),

                    // ── Preserve filter state ─────────────────────────────────
                    SearchTerm = search,
                    CompanyId = companyId,
                    IsActive = isActive,
                    UserTypeId = userTypeId
                };
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(new UsersPageViewModel());
            }
        }

        /// <summary>
        /// Gets the details of a specific user, including their name, login info, and roles, for viewing or editing.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUser(int userId)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit users." });

            try
            {
                var userResponse = await _userClient.GetUserByIdAsync(userId);
                if (!userResponse.Success)
                    return Json(new { success = false, message = userResponse.Message });

                var roleIdsResponse = await _userClient.GetUserRoleIdsAsync(userId);
                var user = userResponse.Data;

                return Json(new
                {
                    success = true,
                    user = new
                    {
                        user.UserID,
                        user.FullName,
                        user.Username,
                        user.Email,
                        user.PhoneNo,
                        user.UserTypeID,
                        user.DefaultRoleID,
                        user.DashboardID,
                        user.BackDaysAllow,
                        user.IsOTPEnabled,
                        user.OTPSecret,
                        OTPExpiry  = user.OTPExpiry?.ToString("yyyy-MM-dd"),
                        StartDate  = user.StartDate?.ToString("yyyy-MM-dd"),
                        EndDate    = user.EndDate?.ToString("yyyy-MM-dd"),
                        StartTime  = user.StartTime?.ToString(@"hh\:mm"),
                        EndTime    = user.EndTime?.ToString(@"hh\:mm"),
                        user.IsActive,
                        user.IsLocked,
                        RoleIDs    = roleIdsResponse.Success ? roleIdsResponse.Data : new List<int>()
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }); 
            }
        }

        /// <summary>
        /// Saves a new user account or updates an existing one with the information you provided.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] UserUpsertRequest request)
        {
            var isCreate = request.UserID <= 0;
            if (isCreate && !(await _menuPerm.Has(MenuPath, "Add")).Data)
                return Json(new { success = false, message = "You do not have permission to add users." });
            if (!isCreate && !(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit users." });

            try
            {
                if (string.IsNullOrWhiteSpace(request.FullName))
                    return Json(new { success = false, message = "Full name is required" });

                if (string.IsNullOrWhiteSpace(request.Username))
                    return Json(new { success = false, message = "Username is required" });

                if (request.UserTypeID <= 0)
                    return Json(new { success = false, message = "User type is required" });

                var response = await _userClient.SaveUserAsync(request);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Turns a user account on or off, determining if they can log into the system.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int userId, bool isActive)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to change user status." });

            try
            {
                var response = await _userClient.ToggleStatusAsync(userId, isActive);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Unlocks a user account that might have been locked due to too many failed login attempts.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Unlock(int userId)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to unlock users." });

            try
            {
                var response = await _userClient.UnlockUserAsync(userId);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #region User API Proxy Endpoints

        [HttpGet]
        public async Task<IActionResult> CheckUsername(string username, int userId = 0)
        {
            if (string.IsNullOrWhiteSpace(username))
                return Json(new { success = false, message = "Username is required" });

            var response = await _userClient.IsUsernameUniqueAsync(username, userId);
            return Json(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserRoles(int userId)
        {
            var response = await _userClient.GetUserRoleIdsAsync(userId);
            return Json(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetWizardData(int userId = 0, string roleIds = "")
        {
            var response = await _userClient.GetUserWizardDataAsync(userId, roleIds);
            return Json(response);
        }

        [HttpPost]
        public async Task<IActionResult> SaveWizard([FromBody] UserUpsertRequest request)
        {
            var isCreate = request.UserID <= 0;
            if (isCreate && !(await _menuPerm.Has(MenuPath, "Add")).Data)
                return Json(new { success = false, message = "You do not have permission to add users." });
            if (!isCreate && !(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit users." });

            var response = await _userClient.SaveUserWizardAsync(request);
            return Json(response);
        }

        #endregion


        public async Task<IActionResult> AddUser(int? id)
        {
            ViewData["Title"] = "Add User";

            //var usersResponse = await _userClient.GetAllUsersAsync();
            var rolesResponse = await _userClient.GetRolesDropdownAsync();
            var typesResponse = await _userClient.GetUserTypesDropdownAsync();
            var companiesResponse = await _companyClient.GetAllAsync();

            var model = new UsersPageViewModel
            {
                // Users = (usersResponse.Success && usersResponse.Data != null) ? usersResponse.Data : new List<UserViewModel>(),
                Roles = (rolesResponse.Success && rolesResponse.Data != null) ? rolesResponse.Data : new List<RoleViewModel>(),
                UserTypes = (typesResponse.Success && typesResponse.Data != null) ? typesResponse.Data : new List<MstUserTypeViewModel>(),
                Companies = (companiesResponse.Success && companiesResponse.Data != null) ? companiesResponse.Data : new List<MstCompanyViewModel>()
            };


            if (id.HasValue && id.Value > 0)
            {
                var userRes = await _userClient.GetUserByIdAsync(id.Value);
                if (userRes.Success)
                {
                    model.EditUser = userRes.Data;
                }
            }


            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> BulkToggleStatus([FromBody] UserStatusUpdateRequest request)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to change user status." });

            try
            {
                var response = await _userClient.BulkToggleStatusAsync(request);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> DeleteBulkUser(string ids)
        {
            try
            {
                var response = await _userClient.DeleteBulkUserAsync(ids);

                if (response.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = response.Message
                    });
                }

                return Json(new
                {
                    success = false,
                    message = response.Message
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
