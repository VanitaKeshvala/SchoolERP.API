using Microsoft.AspNetCore.Mvc;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using SchoolERP.Net.Services;
using SchoolERP.Net.Services.Clients;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Net.Helpers;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SchoolERP.Net.Controllers
{
    public class SectionController : BaseController
    {
        private readonly ISectionClientService _sectionClient;
        private readonly IUserMenuPermissionClientService _menuPerm;
        private readonly ICompanyClientService _companyService;
        private readonly ISessionClientService _sessionService;
        private const string MenuPath = "/Section";

        public SectionController(ISectionClientService sectionClient, IUserMenuPermissionClientService menuPerm ,
            PermissionHelper permHelper, ICompanyClientService companyService, ISessionClientService sessionService) : base(permHelper)
        {
            _sectionClient = sectionClient;
            _menuPerm = menuPerm;
            _companyService = companyService;
            _sessionService = sessionService;
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
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Section"
               );

                var request = new HostelTypeSearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionID = sessionID ?? CurrentSessionId
                };


                var sectionResponse = _sectionClient.GetAllSectionWithPagePageAsync(request);
                var sessionTask = _sessionService.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();

                await Task.WhenAll(sectionResponse, sessionTask, companiesTask);

                var pagedResult = await sectionResponse;

                var model = new MstSectionPageViewModel
                {
                    Sections = pagedResult.Success ? pagedResult.Data.Data : new List<MstSectionViewModel>(),
                    Companies = (await companiesTask).Data ?? new(),
                    Sessions = (await sessionTask).Data ?? new(),
                    TotalRecords = pagedResult.Data.TotalRecords,
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,
                    SearchTerm = search,
                    CompanyId = companyId,
                    SessionId = sessionID
                };
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        public async Task<IActionResult> Add(int? id)
        {
            // Step 1: Initialize a new blank page model.
            var model = new MstSectionViewModel();
           
            // Step 2: If we are editing an existing person (ID is provided), fetch their details.

            if (id.HasValue && id.Value > 0)
            {
                var sectionRes = await _sectionClient.GetByIDAsync(id.Value);
                if (sectionRes.Success)
                {
                    model.EditSections = sectionRes.Data;
                }
            }
            else 
            {
                model.EditSections = null;
            }
                return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetSection(int id)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit sections." });

            var response = await _sectionClient.GetByIDAsync(id);
            if (!response.Success) return Json(new { success = false, message = response.Message });
            return Json(new { success = true, data = response.Data });
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] MstSectionUpsertRequest request)
        {
            var isCreate = request.SectionID <= 0;
            if (isCreate && !(await _menuPerm.Has(MenuPath, "Add")).Data)
                return Json(new { success = false, message = "You do not have permission to add sections." });
            if (!isCreate && !(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit sections." });
            
            var response = await _sectionClient.UpsertAsync(request);
            return Json(new { success = true, message = response.Message });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus([FromBody] StatusUpdateRequest request)
        {
            try
            {
                if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "You do not have permission to change section status." });

                var response = await _sectionClient.ToggleStatusAsync(request);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] List<int> ids)
        {
            if (!(await _menuPerm.Has(MenuPath, "Delete")).Data)
                return Json(new { success = false, message = "You do not have permission to delete sections." });

            var response = await _sectionClient.DeleteAsync(ids);
            return Json(new { success = response.Success, message = response.Message });
        }

        [HttpPost]
        public async Task<IActionResult> CopyToSession([FromBody] SectionCopyRequest request)
        {
            try
            {
                if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "You do not have permission to delete sections." });

                var response = await _sectionClient.CopyToSessionAsync(request);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            
        }
    }
}
