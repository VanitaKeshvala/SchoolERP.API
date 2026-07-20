using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Helpers;
using SchoolERP.Net.Services.Clients;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using static System.Collections.Specialized.BitVector32;

namespace SchoolERP.Net.Controllers
{
    public class PurposeController : BaseController
    {
        private readonly IFrontOfficeClientService _client;
        private readonly IUserMenuPermissionClientService _menuPerm;
        private readonly ISectionClientService _sectionClient;
        private readonly ICompanyClientService _companyService;
        private const string MenuPath = "/FrontOffice/Setup";
        private readonly ISessionClientService _sessionService;
        public PurposeController(
            IFrontOfficeClientService client,
            IUserMenuPermissionClientService menuPerm,
            ISessionClientService sessionService,
            PermissionHelper permHelper,
            ICompanyClientService companyService, ISectionClientService sectionClient) : base(permHelper)
        {
            _client = client;
            _menuPerm = menuPerm;
            _sessionService = sessionService;
            _companyService = companyService;
            _sectionClient = sectionClient;
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
            var model = new PurposesPageViewModel();
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/FrontOffice/Setup"
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
                var classesResponse = _client.GetAllPurposesWithPageAsync(request);
                var sectionsResponse = await _sectionClient.GetAllAsync(false, request.SessionID, request.CompanyID);

                var sessionTask = _sessionService.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();

                await Task.WhenAll(classesResponse, sessionTask, companiesTask);

                var pagedResult = await classesResponse;

                model = new PurposesPageViewModel
                {
                    Purposes = pagedResult.Success ? pagedResult.Data.Data : new List<MstFOPurposeViewModel>(),
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

        public async Task<IActionResult> AddPurposes(int? id)
        {
            var model = new PurposesAddPageViewModel();
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/FrontOffice/PostalReceive"
               );

                if (id.HasValue && id.Value > 0)
                {
                    var response = await _client.GetPurposeByIDAsync(id.Value);
                    if (response.Success)
                    {
                        model.Purposes = response.Data;
                        model.EditPurposes = response.Data;
                    }
                }
                else
                {
                    model.EditPurposes = null;
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

        [HttpPost]
        public async Task<IActionResult> SavePurpose([FromBody] MstFOPurposeUpsertRequest req)
        {
            try
            {
                bool isCreate = req.PurposeID <= 0;
                if (isCreate && !(await _menuPerm.Has(MenuPath, "Add")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                if (!isCreate && !(await _menuPerm.Has(MenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                req.CompanyID = await GetCompanyId();
                req.SessionID = await GetSessionId();
                var r = await _client.UpsertPurposeAsync(req);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = ex.Message });
            }

        }

        [HttpPost]
        public async Task<IActionResult> TogglePurposeStatus([FromBody] StatusUpdateRequest request)
        {
            try
            {
                if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                var r = await _client.TogglePurposeStatusAsync(request);
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
        public async Task<IActionResult> DeletePurpose([FromBody] List<int> id)
        {
            try
            {
                if (!(await _menuPerm.Has(MenuPath, "Delete")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                var r = await _client.DeletePurposeAsync(id);
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
