using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Helpers;
using SchoolERP.Net.Services.Clients;
using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;
using System.Text.Json;

namespace SchoolERP.Net.Controllers
{
    public class LessonPlanController : BaseController
    {
        private readonly ILessonPlanClientService _lessonPlanClient;
        private readonly IClassClientService _classClient;
        private readonly ISubjectGroupClientService _subjectGroupClient;
        private readonly ISubjectClientService _subjectClient;
        private readonly IUserMenuPermissionClientService _menuPerm;
        private readonly ICompanyClientService _companyService;
        private readonly ISessionClientService _sessionService;
        private const string MenuPath = "/LessonPlan/Add";
        private readonly IConfiguration _configuration;
        private readonly IPhotoUploadService _photoService;
        private readonly IWebHostEnvironment _environment;
        public LessonPlanController(
            ILessonPlanClientService lessonPlanClient,
            IClassClientService classClient,
            ISubjectGroupClientService subjectGroupClient,
            ISubjectClientService subjectClient,
            ICompanyClientService companyService,
            ISessionClientService sessionService,
            IUserMenuPermissionClientService menuPerm,
            IConfiguration configuration,
            IPhotoUploadService photoService,
            IWebHostEnvironment environment, PermissionHelper permHelper) : base(permHelper)
        {
            _lessonPlanClient = lessonPlanClient;
            _classClient = classClient;
            _subjectGroupClient = subjectGroupClient;
            _subjectClient = subjectClient;
            _menuPerm = menuPerm;
            _companyService = companyService;
            _sessionService = sessionService;
            _configuration = configuration;
            _photoService = photoService;
            _environment = environment;
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
          int? sessionID,
          int? classID,
          int? sectionId,
          int? subjectGroupId,
          int? subjectId)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/LessonPlan/Index"
               );

                var request = new LessonPlanSearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionID = sessionID ?? await GetSessionId(),
                    ClassID=classID ?? null,
                    SectionID = sectionId ?? null,
                    SubjectGroupID = subjectGroupId ?? null,
                    SubjectID = subjectId ?? null
                };

                //THIS SECTION IF STUDENT LLOADING CHECK ONLY HER CLASS AND SECTION HOME WORK 
                var role = User.FindFirst("UserTypeName")?.Value;
                if (role.Trim() == "Student")
                {
                    request.ClassID = int.Parse(User.FindFirst("ClassID")?.Value);
                    request.SectionID = int.Parse(User.FindFirst("SectionID")?.Value);
                    ViewBag.IsStudent = true;
                }

                var sessionId = await GetSessionId();
                var classesResponse = _lessonPlanClient.GetAllLessonPlanWithPageAsync(request);

                var sessionTask = _sessionService.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();
                var classes = await _classClient.GetAllAsync(false, await GetSessionId(), await GetCompanyId());
                await Task.WhenAll(classesResponse, sessionTask, companiesTask);

                var pagedResult = await classesResponse;

                var model = new LessonPlanPageViewModel
                {
                    LessonPlan = pagedResult.Success ? pagedResult.Data.Data : new List<LessonPlanModel>(),
                    Companies = (await companiesTask).Data ?? new(),
                    Sessions = (await sessionTask).Data ?? new(),
                    Classes = classes.Data,
                    TotalRecords = pagedResult.Data.TotalRecords,
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,
                    SearchTerm = search,
                    CompanyId = companyId,
                    SessionId = sessionId,
                    ClassID = classID,
                    SectionID = sectionId,
                    SubjectGroupID = subjectGroupId,
                    SubjectID = subjectId

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
                   "/LessonPlan"
               );
                var model = new LessonPlanAddViewModel();
                var classes = await _classClient.GetAllAsync(false, await GetSessionId(), await GetCompanyId());

                if (id.HasValue && id.Value > 0)
                {
                    var lessonMap = await _lessonPlanClient.GetAllMapLessonAsync(await GetCompanyId(),await GetSessionId(),id.Value);
                    var response = await _lessonPlanClient.GetByIDAsync(id.Value);
                    if (response.Success)
                    {
                        model.EditLessonPlan = response.Data;
                    }
                    model.LessonPlanMap = lessonMap.Data;
                }
                else
                {
                    model.EditLessonPlan = null;
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

        [HttpPost]
        public async Task<IActionResult> UpsertLessonPlan([FromForm] LessonPlanModelRequest request)
        {
            try
            {
                request.CompanyID = await GetCompanyId();
                request.SessionID = await GetSessionId();
                var isCreate = request.LessonId <= 0;
                if (isCreate && !(await _menuPerm.Has(MenuPath, "Add")).Data)
                    return Json(new { success = false, message = "You do not have permission to add homework." });
                if (!isCreate && !(await _menuPerm.Has(MenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "You do not have permission to edit homework." });

                var res = await _lessonPlanClient.UpsertAsync(request);
             
                return Json(res);
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [HttpPost]
        public async Task<IActionResult> DeleteLessonPlan([FromBody] List<int> ids)
        {
            try
            {
                if (!(await _menuPerm.Has(MenuPath, "Delete")).Data)
                    return Json(new { success = false, message = "You do not have permission to delete homework." });

                var res = await _lessonPlanClient.DeleteAsync(ids);
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
        public async Task<IActionResult> ToggleLessonPlankStatus([FromBody] StatusUpdateRequest request)
        {
            try
            {
                if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "You do not have permission to change homework status." });

                var res = await _lessonPlanClient.ToggleStatusAsync(request);
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


    }
}
