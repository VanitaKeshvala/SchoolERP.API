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
    /// This controller manages the academic sessions (like '2023-24' or '2024-25'), allowing you to define the school years used in the system.
    /// </summary>
    public class SessionsController : BaseController
    {
        private readonly ISessionClientService _sessionClient;
        private readonly IUserMenuPermissionClientService _menuPerm;
        private const string MenuPath = "/Sessions";

        public SessionsController(ISessionClientService sessionClient, IUserMenuPermissionClientService menuPerm, PermissionHelper permHelper) : base(permHelper)
        {
            _sessionClient = sessionClient;
            _menuPerm = menuPerm;
        }

        /// <summary>
        /// Shows the main list of all academic sessions defined in the system.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Sessions"
               );

                var sessionId = CurrentSessionId;
                var response = await _sessionClient.GetAllAsync();
                var model = new MstSessionPageViewModel
                {
                    Sessions = response.Success ? response.Data : new List<MstSessionViewModel>()
                };
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }

        /// <summary>
        /// Gets the details of a specific academic session so you can view or edit its dates and name.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSession(int id)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit sessions." });

            var response = await _sessionClient.GetByIDAsync(id);
            if (!response.Success) return Json(new { success = false, message = response.Message });
            return Json(new { success = true, data = response.Data });
        }

        /// <summary>
        /// Saves a new academic session or updates an existing one with the dates and name you provided.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] MstSessionUpsertRequest request)
        {
            var isCreate = request.SessionId <= 0;
            if (isCreate && !(await _menuPerm.Has(MenuPath, "Add")).Data)
                return Json(new { success = false, message = "You do not have permission to add sessions." });
            if (!isCreate && !(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit sessions." });

            var response = await _sessionClient.UpsertAsync(request);
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Turns an academic session on or off, determining if it can be selected for work.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id, bool isActive)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to change session status." });

            var response = await _sessionClient.ToggleStatusAsync(id, isActive);
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Permanently removes an academic session from the system's records.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody]  List<int> id)
        {
            if (!(await _menuPerm.Has(MenuPath, "Delete")).Data)
                return Json(new { success = false, message = "You do not have permission to delete sessions." });

            var response = await _sessionClient.DeleteAsync(id);
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Sets the chosen academic session as the 'active' one for the current user's session.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SetCurrent([FromBody] SetCurrentSessionRequest request)
        {
            if (request == null || request.SessionId <= 0)
                return Json(new { success = false, message = "Invalid session selection." });

            var response = await _sessionClient.SetCurrentSessionAsync(request);
            return Json(new { success = response.Success, message = response.Message });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(bool includeDeleted = false)
        {
            var response = await _sessionClient.GetAllAsync(includeDeleted);
            return Json(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserCurrentSession()
        {
            var response = await _sessionClient.GetUserCurrentSessionAsync();
            return Json(new { success = true, data = response.Data });
        }
    }
}
