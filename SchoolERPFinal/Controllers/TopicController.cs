using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Helpers;
using SchoolERP.Net.Services.Clients;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Controllers
{
    public class TopicController : BaseController
    {
        private readonly ITopicClientService _topicClient;
        private readonly ILessonPlanClientService _lessonPlanClient;
        private readonly IClassClientService _classClient;
        private readonly ISubjectGroupClientService _subjectGroupClient;
        private readonly ISubjectClientService _subjectClient;
        private readonly IUserMenuPermissionClientService _menuPerm;
        private readonly ICompanyClientService _companyService;
        private readonly ISessionClientService _sessionService;
        private const string MenuPath = "/Homework/Add";
        private readonly IConfiguration _configuration;
        private readonly IPhotoUploadService _photoService;
        private readonly IWebHostEnvironment _environment;
        public TopicController(
            ITopicClientService topicClient,
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
            _topicClient = topicClient;
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
          int? subjectId,
          int? lessonID)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Topic/Index"
               );

                var request = new TopicSearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionID = sessionID ?? await GetSessionId(),
                    ClassID = classID ?? null,
                    SectionID = sectionId ?? null,
                    SubjectGroupID = subjectGroupId ?? null,
                    SubjectID = subjectId ?? null,
                    LessonID= lessonID ?? null
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
                var classesResponse = _topicClient.GetAllTopicWithPageAsync(request);

                var sessionTask = _sessionService.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();
                var classes = await _classClient.GetAllAsync(false, await GetSessionId(), await GetCompanyId());
                await Task.WhenAll(classesResponse, sessionTask, companiesTask);

                var pagedResult = await classesResponse;

                var model = new TopicPageViewModel
                {
                    Topic = pagedResult.Success ? pagedResult.Data.Data : new List<TopicViewModel>(),
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
                   "/Topic/Add"
               );
                var model = new TopicAddViewModel();
                var classes = await _classClient.GetAllAsync(false, await GetSessionId(), await GetCompanyId());

                if (id.HasValue && id.Value > 0)
                {
                    var topicMap = await _topicClient.GetAllMapTopicAsync(await GetCompanyId(), await GetSessionId(), id.Value);
                    var response = await _topicClient.GetByIDAsync(id.Value);
                    if (response.Success)
                    {
                        model.EditTopic = response.Data;
                    }
                    model.TopicMap = topicMap.Data;
                }
                else
                {
                    model.EditTopic = null;
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
        public async Task<IActionResult> UpsertTopic([FromForm] TopicModelRequest request)
        {
            try
            {
                request.CompanyID = await GetCompanyId();
                request.SessionID = await GetSessionId();
                var isCreate = request.TopicId <= 0;
                if (isCreate && !(await _menuPerm.Has(MenuPath, "Add")).Data)
                    return Json(new { success = false, message = "You do not have permission to add homework." });
                if (!isCreate && !(await _menuPerm.Has(MenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "You do not have permission to edit homework." });

                var res = await _topicClient.UpsertAsync(request);

                return Json(res);
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [HttpPost]
        public async Task<IActionResult> DeleteTopic([FromBody] List<int> ids)
        {
            try
            {
                if (!(await _menuPerm.Has(MenuPath, "Delete")).Data)
                    return Json(new { success = false, message = "You do not have permission to delete homework." });

                var res = await _topicClient.DeleteAsync(ids);
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
        public async Task<IActionResult> ToggleTopicStatus([FromBody] StatusUpdateRequest request)
        {
            try
            {
                if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "You do not have permission to change homework status." });

                var res = await _topicClient.ToggleStatusAsync(request);
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
        public async Task<IActionResult> BindLessonDropDwonList([FromBody] LessonDropDwonReq req)
        {
            try
            {
                req.CompanyID = await GetCompanyId();
                req.SessionID = await GetSessionId();
                var res = await _lessonPlanClient.BindLessonDropDwonListAsync(req);
                return Json(res);
            }
            catch (Exception)
            {
                throw;
            }
            
        }


        public async Task<IActionResult> ManageSyllabusStatus(int? pageIndex,
          int? pageSize,
          string? search,
          int? companyId,
          int? sessionID,
          int? classID,
          int? sectionId,
          int? subjectGroupId,
          int? subjectId,
          int? lessonID)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Topic/Index"
               );

                var request = new TopicSearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionID = sessionID ?? await GetSessionId(),
                    ClassID = classID ?? null,
                    SectionID = sectionId ?? null,
                    SubjectGroupID = subjectGroupId ?? null,
                    SubjectID = subjectId ?? null,
                    LessonID = lessonID ?? null
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
                var classesResponse = _topicClient.GetAllTopicSyllaBussStatusWithPageAsync(request);

                var sessionTask = _sessionService.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();
                var classes = await _classClient.GetAllAsync(false, await GetSessionId(), await GetCompanyId());
                await Task.WhenAll(classesResponse, sessionTask, companiesTask);

                var pagedResult = await classesResponse;

                var model = new LessonSyllabusStatusPageViewModel
                {
                    LessonSyllabusStatus = pagedResult.Success ? pagedResult.Data.Data : new List<LessonSyllabusStatusResponse>(),
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


        [HttpPost]
        public async Task<IActionResult> ToggleTopicCompleteStatus([FromBody] StatusUpdateRequest request)
        {
            try
            {
                if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "You do not have permission to change homework status." });

                var res = await _topicClient.ToggleTopicCompleteStatusAsync(request);
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
