using Microsoft.AspNetCore.Mvc;
using SchoolERP.Shared.Models;
using SchoolERP.Net.Services;
using SchoolERP.Net.Services.Clients;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Net.Helpers;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Controllers
{
    public class HomeworkController : BaseController
    {
        private readonly IHomeworkClientService _homeworkClient;
        private readonly IClassClientService _classClient;
        private readonly ISubjectGroupClientService _subjectGroupClient;
        private readonly ISubjectClientService _subjectClient;
        private readonly IUserMenuPermissionClientService _menuPerm;
        private readonly ICompanyClientService _companyService;
        private readonly ISessionClientService _sessionService;
        private const string MenuPath = "/Homework/Add";

        public HomeworkController(
            IHomeworkClientService homeworkClient, 
            IClassClientService classClient, 
            ISubjectGroupClientService subjectGroupClient,
            ISubjectClientService subjectClient,
            ICompanyClientService companyService,
            ISessionClientService sessionService,
            IUserMenuPermissionClientService menuPerm, PermissionHelper permHelper) : base(permHelper)
        {
            _homeworkClient = homeworkClient;
            _classClient = classClient;
            _subjectGroupClient = subjectGroupClient;
            _subjectClient = subjectClient;
            _menuPerm = menuPerm;
            _companyService = companyService;
            _sessionService = sessionService;
        }

        private async Task<int> GetCompanyId()
        {
            var response = await _companyService.GetUserCurrentCompanyAsync();
            return response?.Data ?? 0;
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
        int? sessionID,string mode)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Homework/Index"
               );

                var request = new SearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionID = sessionID ?? await GetSessionId(),
                    Mode=mode
                };

                var sessionId = await GetSessionId();
                var classesResponse = _homeworkClient.GetAllHomeWorkWithPageAsync(request);

                var sessionTask = _sessionService.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();

                await Task.WhenAll(classesResponse, sessionTask, companiesTask);

                var pagedResult = await classesResponse;

                var model = new HomeworkPageViewModel
                {
                    Homeworks = pagedResult.Success ? pagedResult.Data.Data : new List<HomeworkViewModel>(),
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
                   "/Homework/Add"
               );
                var model = new HomeworkAddViewModel();
                var classes = await _classClient.GetAllAsync(false,await GetSessionId(),await GetCompanyId());
                if (id.HasValue && id.Value > 0)
                {
                    var response = await _homeworkClient.GetByIDAsync(id.Value);
                    if (response.Success)
                    {
                        model.Homeworks = response.Data;
                        model.EditHomeworks = response.Data;
                    }
                }
                else
                {
                    model.EditHomeworks = null;
                }
                model.Classes = classes.Data;
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        #region Homework API Proxy Endpoints

        [HttpGet]
        public async Task<IActionResult> GetSubjectGroups()
        {
            var res = await _subjectGroupClient.GetAllAsync();
            return Json(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetSubjectGroupByID(int id)
        {
            var res = await _subjectClient.GetSubjectGropBySubjectDropdownList(id);
            return Json(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetSubjects()
        {
            var request = new SubjectSearchRequest
            {
                PageNumber = 1,
                PageSize = 10,
                SearchKeyword = null,
                CompanyID = 1,//await GetCompanyId(),
                SessionID = 2//sessionId
            };
            var res = await _subjectClient.GetAllAsync(request);
            return Json(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetHomeworkByID(int id)
        {
            var res = await _homeworkClient.GetByIDAsync(id);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> UpsertHomework([FromBody] HomeworkUpsertRequest request)
        {
            var isCreate = request.HomeworkID <= 0;
            if (isCreate && !(await _menuPerm.Has(MenuPath, "Add")).Data    )
                return Json(new { success = false, message = "You do not have permission to add homework." });
            if (!isCreate && !(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit homework." });

            var res = await _homeworkClient.UpsertAsync(request);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteHomework([FromBody] List<int> ids)
        {
            try
            {
                if (!(await _menuPerm.Has(MenuPath, "Delete")).Data)
                    return Json(new { success = false, message = "You do not have permission to delete homework." });

                var res = await _homeworkClient.DeleteAsync(ids);
                return Json(res);
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

        [HttpPost]
        public async Task<IActionResult> ToggleHomeworkStatus([FromBody] StatusUpdateRequest request)
        {
            try
            {
                if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "You do not have permission to change homework status." });

                var res = await _homeworkClient.ToggleStatusAsync(request);
                return Json(res);
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

        #endregion
    }
}
