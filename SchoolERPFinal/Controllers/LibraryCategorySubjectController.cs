using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Helpers;
using SchoolERP.Net.Services.Clients;
using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;
using System.ComponentModel.Design;

namespace SchoolERP.Net.Controllers
{
    public class LibraryCategorySubjectController : BaseController
    {
        private readonly ILibraryCategorySubjectClientService _client;
        private readonly ICompanyClientService _companyService;
        private readonly ISessionClientService _sessionService;
        public LibraryCategorySubjectController(ILibraryCategorySubjectClientService client,
            ICompanyClientService companyService, ISessionClientService sessionService, PermissionHelper permHelper) : base(permHelper)
        {
            _client = client;
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
        int? categoryId,
        int? subjectId)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/LibraryCategory/Index"
               );

                var request = new LibraryCategorySubjectSearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    CategoryId =categoryId ?? null,
                    SubjectId = subjectId ?? null
                };

                var sessionId = await GetSessionId();
                var budgetResponse = _client.GetAllLibraryCategorySubjectWithPageAsync(request);
                var companiesTask = _companyService.GetAllAsync();
                var category = await _client.GetAllLibraryCategoryAsync(await GetCompanyId());
                var subject = await _client.GetAllLibrarySubjectAsync(await GetCompanyId());
                await Task.WhenAll(budgetResponse, companiesTask);

                var pagedResult = await budgetResponse;

                var model = new LibraryCategorySubjectPageViewModel
                {
                    CategorySubject = pagedResult.Success ? pagedResult.Data.Data : new List<LibraryCategorySubjectModel>(),
                    Companies = (await companiesTask).Data ?? new(),
                    TotalRecords = pagedResult.Data.TotalRecords,
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,
                    SearchTerm = search,
                    CompanyId = companyId,
                    Category = category.Data,
                    Subject = subject.Data
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
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/LibraryCategory/Index"
               );
                var model = new LibraryCategorySubjectAddViewModel();
                var category = await _client.GetAllLibraryCategoryAsync(await GetCompanyId());
                var subject = await _client.GetAllLibrarySubjectAsync(await GetCompanyId());
                if (id.HasValue && id.Value > 0)
                {
                    var response = await _client.GetByIDAsync(id.Value);
                    if (response.Success)
                    {
                        model.CategorySubject = response.Data;
                        model.EditCategorySubject = response.Data;
                    }
                }
                else
                {
                    model.EditCategorySubject = null;
                }
                model.Category = category.Data;
                model.Subject=subject.Data;
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveLibraryCategory([FromBody] LibraryCategorySubjectRequest request)
        {
            try
            {
                var isCreate = request.CompanyID <= 0;
                request.CompanyID = await GetCompanyId();
                var response = await _client.UpsertLibraryCategorySubjectAsync(request);
                return Json(new { success = response.Data.Result, message = response.Data.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }

        [HttpPost]
        public async Task<IActionResult> DeleteLibraryCategory([FromBody] List<int> ids)
        {
            try
            {
                var response = await _client.DeleteAsync(ids);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }

        [HttpPost]
        public async Task<IActionResult> ToggleLibraryCategory([FromBody] StatusUpdateRequest request)
        {
            try
            {
                var response = await _client.ToggleStatusAsync(request);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }
    }
}
