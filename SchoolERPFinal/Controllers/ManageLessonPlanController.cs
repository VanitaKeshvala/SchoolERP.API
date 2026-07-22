using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Helpers;
using SchoolERP.Net.Services.Clients;
using SchoolERP.Shared.Models;
using System.Text.Json;

namespace SchoolERP.Net.Controllers
{
    public class ManageLessonPlanController : BaseController
    {
        private readonly IManageLessonPlanClientService _client;
        private readonly IClassClientService _classClient;
        private readonly ISectionClientService _sectionClient;
        private readonly ISubjectClientService _subjectClient;
        private readonly ISubjectGroupClientService _subjectGroupClient;
        private readonly IAcademicsClientService _academicsClient;
        private readonly IHumanResourceClientService _hrClient;
        private readonly ISessionClientService _sessionClient;
        private readonly IUserMenuPermissionClientService _menuPerm;
        private readonly ICompanyClientService _companyService;
        private readonly ILessonPlanClientService _lessonPlanService;
        private readonly ITopicClientService _topicService;
        private readonly IConfiguration _configuration;
        private readonly IPhotoUploadService _photoService;
        private readonly IWebHostEnvironment _environment;
        private const string ClassMenuPath = "/Academics/Class";
        private const string SubjectMenuPath = "/Academics/Subject";
        private const string SubjectGroupMenuPath = "/Academics/SubjectGroup";
        private const string TimeTableMenuPath = "/Academics/AddTimeTable";

        public ManageLessonPlanController(
            IManageLessonPlanClientService client,
            IClassClientService classClient,
            ISectionClientService sectionClient,
            ISubjectClientService subjectClient,
            ISubjectGroupClientService subjectGroupClient,
            IAcademicsClientService academicsClient,
            IHumanResourceClientService hrClient,
            ISessionClientService sessionClient,
            IUserMenuPermissionClientService menuPerm,
            ILessonPlanClientService lessonPlanService,
            ITopicClientService topicService, 
            IConfiguration configuration,
            IPhotoUploadService photoService,
            IWebHostEnvironment environment, ICompanyClientService companyService, PermissionHelper permHelper) : base(permHelper)
        {
            _client = client;
            _classClient = classClient;
            _sectionClient = sectionClient;
            _subjectClient = subjectClient;
            _subjectGroupClient = subjectGroupClient;
            _academicsClient = academicsClient;
            _hrClient = hrClient;
            _sessionClient = sessionClient;
            _menuPerm = menuPerm;
            _companyService = companyService;
            _lessonPlanService = lessonPlanService;
            _topicService = topicService;
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
                var response = await _sessionClient.GetUserCurrentSessionAsync();
                return response?.Data ?? 0;
            }
            return CurrentSessionId;
        }
        private int? GetStaffID()
        {
            var staffClaim = User.FindFirst("StaffID")?.Value;
            return int.TryParse(staffClaim, out var staffId) ? staffId : null;
        }

        [HttpGet]
        public async Task<IActionResult>Index(int? staffId)
        {
            try
            {
                var sessionId = await GetSessionId();
                var companyId = await GetCompanyId();
                if(staffId ==null || staffId == 0) 
                {
                    staffId = GetStaffID();
                    staffId = (staffId ?? 0) <= 0 ? null : staffId;
                }
                

                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/ManageLessonPlan/Index"
               );


                var staff = await _hrClient.GetAllStaffAsync(companyId,sessionId, staffId);

                
                var model = new TeacherTimeTablePageViewModel
                {
                    Staff = staff.Success ? staff.Data : new List<HRStaffViewModel>(),
                    SelectedStaffId = staffId??0
                };
                if (staffId > 0)
                {
                    var req = new TimeTableSearchRequest
                    {
                        CompanyID = companyId,
                        SessionID = sessionId,
                        StaffID = staffId ?? null
                    };
                    var slots = await _academicsClient.GetTimeTableByStaffAsync(req);
                    model.TimeTableSlots = slots.Success ? slots.Data : new List<TimeTableViewModel>();
                }
                else 
                {
                    //THIS SECTION IF STUDENT LLOADING CHECK ONLY HER CLASS AND SECTION HOME WORK 
                    var role = User.FindFirst("UserTypeName")?.Value;
                    if (role.Trim() == "Student")
                    {
                        var req = new TimeTableSearchRequest
                        {
                            ClassID = int.Parse(User.FindFirst("ClassID")?.Value),
                            SectionID = int.Parse(User.FindFirst("SectionID")?.Value),
                            CompanyID = companyId,
                            SessionID = sessionId,
                        };
                        var slots = await _academicsClient.GetTimeTableByStaffAsync(req);
                        model.TimeTableSlots = slots.Success ? slots.Data : new List<TimeTableViewModel>();
                    }
                }
                    model.Permissions = perms;
                return View(model);
            }
            catch (Exception ex)
            {
                throw;
            }            
        }

        public async Task<IActionResult> Add(int? timeTableID,int? lessonPlanId)
        {

            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/ManageLessonPlan/Index"
               );
                var model = new ManageLessonPlanAddViewModel();
                var classes = await _classClient.GetAllAsync(false, await GetSessionId(), await GetCompanyId());
                var timeTable = await _academicsClient.GetTimeTableByIdAsync(await GetCompanyId(),await GetSessionId(), timeTableID.Value);
                if(timeTable != null) 
                {
                    model.TimeTable = timeTable.Data;
                    var rew = new LessonDropDwonReq
                    {
                        CompanyID = await GetCompanyId(),
                        SessionID = await GetSessionId(),
                        SectionID = timeTable.Data.SectionID,
                        ClassID = timeTable.Data.ClassID,
                        SubjectGroupID = timeTable.Data.SubjectGroupID,
                        SubjectID = timeTable.Data.SubjectID
                    };
                    var lesson = await _lessonPlanService.BindLessonDropDwonListAsync(rew);
                    model.BindLesson = lesson.Data;
                    model.ClassID = timeTable.Data.ClassID;
                    model.SectionID = timeTable.Data.SectionID;
                    model.SubjectGroupID = timeTable.Data.SubjectGroupID;
                    model.SubjectID = timeTable.Data.SubjectID;
                    model.TimeTableID = timeTableID;
                }

                if (lessonPlanId.HasValue && lessonPlanId.Value > 0)
                {
                    var response = await _client.GetByIDAsync(lessonPlanId.Value);
                    if (response.Success)
                    {
                        model.EditManageLessonPlan = response.Data;
                    }
                }
                else
                {
                    model.EditManageLessonPlan = null;
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

        [HttpGet]
        public async Task<IActionResult> BindTopicDropDwonList(int lessonMapId)
        {
            try
            {
                var res = await _topicService.BindTopicDropDwonListAsync(lessonMapId);
                return Json(res);
            }
            catch (Exception)
            {
                throw;
            }

        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 100_000_000)] // ~100MB, adjust to your MaxVideoFileSizeMB setting
        [RequestSizeLimit(100_000_000)]
        public async Task<IActionResult> UpsertLessonPlan([FromForm] ManageLessonPlanRequest request, IFormFile? avideoAttachment, IFormFile? attachmentFiles)
        {
            try
            {
                request.CompanyID = await GetCompanyId();
                request.SessionID = await GetSessionId();
                var isCreate = request.LessonPlanId <= 0;

                var res = await _client.UpsertAsync(request);

                if (res?.Data is JsonElement json)
                {
                    int lessonPlanId = json.GetProperty("data").GetInt32();

                    string? videoPath = null;
                    string? attachmentPath = null;

                    // ── Handle video upload ─────────────────────────────
                    if (avideoAttachment != null && avideoAttachment.Length > 0)
                    {
                        var videoResult = await _photoService.UploadVideoAsync(
                            avideoAttachment,
                            PhotoModule.LessonPlan,
                            FolderNameModule.Video,
                            lessonPlanId
                        );

                        if (!videoResult.Success)
                            return Json(new { success = false, message = videoResult.Message });

                        videoPath = videoResult.PhotoUrl;
                    }

                    // ── Handle document/photo attachment upload ─────────
                    if (attachmentFiles != null && attachmentFiles.Length > 0)
                    {
                        using var memoryStream = new MemoryStream();
                        await attachmentFiles.CopyToAsync(memoryStream);
                        byte[] fileBytes = memoryStream.ToArray();

                        var photoResult = await _photoService.SaveBase64PhotoAsync(
                            Convert.ToBase64String(fileBytes),
                            attachmentFiles.FileName ?? "photo.jpg",
                            PhotoModule.LessonPlan,
                            FolderNameModule.Images,
                            lessonPlanId
                        );

                        if (!photoResult.Success)
                            return Json(new { success = false, message = photoResult.Message });

                        attachmentPath = photoResult.PhotoUrl;
                    }

                    // ── Single shared upsert call, only fires if something was actually uploaded ──
                    if (videoPath != null || attachmentPath != null)
                    {
                        var attchment = new ManageLessonPlanAttachmentUpsertRequest
                        {
                            LessonPlanId = lessonPlanId,
                            CompanyID = request.CompanyID,
                            AttachmentPath = attachmentPath,
                            LectureVideoPath = videoPath
                        };

                        var attachmentRes = await _client.UpsertAttachmentAsync(attchment);
                    }
                }

                return Json(res);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }

        public async Task<IActionResult> LessonPlanDetail(int? id)
        {
            try
            {
                var perms = await GetPermissions(
                   "/Homework/LessonPlanDetail"
               );
                int? studentId = int.TryParse(User.FindFirst("StudentID")?.Value, out var sId)
                                ? sId
                                : (int?)null;
                var model = new ManageLessonDetailsPlanViewModel();
                var lessonPlan = await _client.GetLessonPlanDetailByIdAsync(id.Value);
                var commitList = await _client.GetAllCommentListAsync(await GetCompanyId(), await GetSessionId(), id.Value);
                if (lessonPlan != null)
                {
                    if (lessonPlan.Data != null)
                    {
                        model.LessonPlanDetails = lessonPlan.Data;
                    }
                }
                model.Comments = commitList.Data;
                model.Permissions = perms;
                model.HomeworkID = id;
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddLessonPlanComment([FromForm] LessonPlanCommentRequest request)
        {
            try
            {
                request.CompanyID = await GetCompanyId();
                request.SessionID = await GetSessionId();
                var role = User.FindFirst("UserTypeName")?.Value;
                if (role.Trim() == "Student") 
                {
                    var studentId =int.Parse(User.FindFirst("StudentID")?.Value);
                    request.CommenterType = role;
                    request.CommentedBy = studentId;
                }
                else 
                {
                    request.CommenterType = role;
                }
                    //STUDENT
                    var isCreate = request.LessonPlanId <= 0;
                var res = await _client.UpsertLessonPlanCommitAsync(request);
               
                return Json(res);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetLessonPlanComments(int lessonMapId)
        {
            try
            {
                var commitList = await _client.GetAllCommentListAsync(await GetCompanyId(), await GetSessionId(), lessonMapId);
                return Json(commitList);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }
        
    }
}
