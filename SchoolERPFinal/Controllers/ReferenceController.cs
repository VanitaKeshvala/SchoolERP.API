using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Helpers;
using SchoolERP.Net.Services.Clients;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Controllers
{
    public class ReferenceController : BaseController
    {
        private readonly IFrontOfficeClientService _client;
        private readonly IUserMenuPermissionClientService _menuPerm;
        private readonly ISectionClientService _sectionClient;
        private readonly ICompanyClientService _companyService;
        private const string MenuPath = "/FrontOffice/Setup";
        private readonly ISessionClientService _sessionService;
        public ReferenceController(
            IFrontOfficeClientService client,
            IUserMenuPermissionClientService menuPerm,
            ISessionClientService sessionService,
            PermissionHelper permHelper,
            ICompanyClientService companyService) : base(permHelper)
        {
            _client = client;
            _menuPerm = menuPerm;
            _sessionService = sessionService;
            _companyService = companyService;
        }
        private async Task<int> GetCompanyId()
        {
            if (CurrentCompanyId == null)
            {
                var response = await _companyService.GetUserCurrentCompanyAsync();
                return response?.Data ?? 0;
            }
            return CurrentCompanyId;
        }
        private async Task<int> GetSessionId()
        {
            if (CurrentSessionId == null)
            {
                var response = await _sessionService.GetUserCurrentSessionAsync();
                return response?.Data ?? 0;
            }
            return CurrentSessionId;
        }

        public async Task<IActionResult> Index(int? pageIndex,
      int? pageSize,
      string? search,
      int? companyId,
      int? sessionID)
        {
            var model = new ReferencePageViewModel();
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/ComplaintType/Index"
               );

                var request = new ClassSearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionID = sessionID ?? await GetSessionId()
                };


                var sessionId = await GetSessionId();
                var classesResponse = _client.GetAllReferenceWithPageAsync(request);

                var sessionTask = _sessionService.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();

                await Task.WhenAll(classesResponse, sessionTask, companiesTask);

                var pagedResult = await classesResponse;

                model = new ReferencePageViewModel
                {
                    Reference = pagedResult.Success ? pagedResult.Data.Data : new List<MstFOReferenceViewModel>(),
                    Companies = (await companiesTask).Data ?? new(),
                    Sessions = (await sessionTask).Data ?? new(),
                    TotalRecords = pagedResult.Data.TotalRecords,
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,
                    SearchTerm = search,
                    CompanyId = companyId,
                    SessionId = sessionId
                };
                model.Permissions = perms;
                // Step 3: Open the 'Setup' page for the user.
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
        }

        public async Task<IActionResult> AddReference(int? id)
        {
            var model = new ReferenceAddPageViewModel();
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/ComplaintType/PostalReceive"
               );

                if (id.HasValue && id.Value > 0)
                {
                    var response = await _client.GetReferenceByIDAsync(id.Value);
                    if (response.Success)
                    {
                        model.Reference = response.Data;
                        model.EditReference = response.Data;
                    }
                }
                else
                {
                    model.EditReference = null;
                }
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
        }

        /// <summary>
        /// One upsert endpoint per entity. PurposeID <= 0 (etc.) decides Create vs Edit,
        /// and checks the matching Add or Edit permission accordingly before calling the service layer.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveReference([FromBody] MstFOReferenceUpsertRequest req)
        {
            try
            {
                bool isCreate = req.ReferenceID <= 0;
                if (isCreate && !(await _menuPerm.Has(MenuPath, "Add")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                if (!isCreate && !(await _menuPerm.Has(MenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                req.CompanyID = await GetCompanyId();
                req.SessionID = await GetSessionId();
                var r = await _client.UpsertReferenceAsync(req);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = ex.Message });
            }

        }

        /// <summary>
        /// Bulk delete — accepts List<int> so multiple rows can be removed in one call. Checks Delete permission.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteReference([FromBody] List<int> id)
        {
            try
            {
                if (!(await _menuPerm.Has(MenuPath, "Delete")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                var r = await _client.DeleteReferenceAsync(id);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = ex.Message });
            }

        }

        /// <summary>
        /// Flips a record's active/inactive flag (e.g. an Active/Inactive switch in a table row) without a full edit round-trip. Checks Edit permission.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ToggleReferenceStatus([FromBody] StatusUpdateRequest request)
        {
            try
            {
                if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                var r = await _client.ToggleReferenceStatusAsync(request);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = ex.Message });
            }

        }

    }
}
