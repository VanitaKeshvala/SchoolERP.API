using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Helpers;
using SchoolERP.Net.Services.Clients;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using System.Text.Json;
using static System.Collections.Specialized.BitVector32;

namespace SchoolERP.Net.Controllers
{
    public class DailyAssignmentController : BaseController
    {
        private readonly IDailyAssignmentClientService _dailyAssignmentClient;
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
        public DailyAssignmentController(
            IDailyAssignmentClientService dailyAssignmentClient,
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
            _dailyAssignmentClient = dailyAssignmentClient;
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
      int? classId,
      int? sectionId)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Homework/Index"
               );

                var request = new DailyAssignmentSearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionID = sessionID ?? await GetSessionId(),
                    StudentId = int.TryParse(User.FindFirst("StudentID")?.Value, out var id)
                                ? id
                                : (int?)null,
                    ClassID=classId??null,
                    SectionID=sectionId??null
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
                var classesResponse = _dailyAssignmentClient.GetAllDailyAssignmentWithPageAsync(request);
                
                var sessionTask = _sessionService.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();
                var classResponse = _classClient.GetAllAsync(false,request.SessionID,request.CompanyID);
                await Task.WhenAll(classesResponse, sessionTask, companiesTask, classResponse);

                var pagedResult = await classesResponse;

                var model = new DailyAssignmentPageViewModel
                {
                    DailyAssignment = pagedResult.Success ? pagedResult.Data.Data : new List<DailyAssignmentModel>(),
                    Companies = (await companiesTask).Data ?? new(),
                    Sessions = (await sessionTask).Data ?? new(),
                    Classes = (await classResponse).Data ?? new(),
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

                var model = new DailyAssignmentAddViewModel();
                int classID=0;
                int sectionID=0;
                var role = User.FindFirst("UserTypeName")?.Value;
                if (role.Trim() == "Student")
                {
                    classID = int.Parse(User.FindFirst("ClassID")?.Value);
                    sectionID = int.Parse(User.FindFirst("SectionID")?.Value);
                    ViewBag.IsStudent = true;
                    var subject = await _subjectGroupClient.GetAllSubjectByClassandSectionIdAsync(await GetCompanyId(), await GetSessionId(), classID, sectionID);
                    model.Subject = subject.Data;
                }



                if (id.HasValue && id.Value > 0)
                {
                    var response = await _dailyAssignmentClient.GetByIDAsync(id.Value);
                    if (response.Success)
                    {
                        model.DailyAssignment = response.Data;
                        model.EditDailyAssignment = response.Data;
                    }                    
                }
                else
                {
                    model.EditDailyAssignment = null;
                }
               
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception)
            {

                throw;
            }

        }


        [HttpPost]
        public async Task<IActionResult> UpsertDailyAssignment([FromForm] AssignmentUpsertRequest request, List<IFormFile>? attachmentFiles)
        {
            try
            {
                request.CompanyID = await GetCompanyId();
                request.SessionID = await GetSessionId();


                var role = User.FindFirst("UserTypeName")?.Value;
                if (role.Trim() == "Student")
                {
                    request.StudentID = int.Parse(User.FindFirst("StudentID")?.Value);
                    request.ClassID = int.Parse(User.FindFirst("ClassID")?.Value);
                    request.SectionID = int.Parse(User.FindFirst("SectionID")?.Value);
                }
                
              
                var isCreate = request.AssignmentID <= 0;
                if (isCreate && !(await _menuPerm.Has(MenuPath, "Add")).Data)
                    return Json(new { success = false, message = "You do not have permission to add homework." });
                if (!isCreate && !(await _menuPerm.Has(MenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "You do not have permission to edit homework." });

                var res = await _dailyAssignmentClient.UpsertAsync(request);
                if (res?.Data is JsonElement json)
                {
                    // On create, the SP now returns the new ID. On update, we already have it.
                    int assignmentId = json.GetProperty("data").GetInt32();

                    if (attachmentFiles != null && attachmentFiles.Count > 0)
                    {
                        var attachmentRows = new List<object>();
                        //int currentUserId = await GetCurrentUserId();

                        foreach (var file in attachmentFiles)
                        {
                            if (file == null || file.Length == 0) continue;

                            using var memoryStream = new MemoryStream();
                            await file.CopyToAsync(memoryStream);
                            byte[] fileBytes = memoryStream.ToArray();

                            var photoResult = await _photoService.SaveBase64PhotoAsync(
                                Convert.ToBase64String(fileBytes),
                                file.FileName,
                                PhotoModule.Homework,          // add this enum value if it doesn't exist yet
                                FolderNameModule.Documents,
                                assignmentId
                            );

                            attachmentRows.Add(new
                            {
                                AttachmentID = 0,               // 0 = new row, matches SP contract
                                AttachmentPath = photoResult.PhotoUrl,
                                AttachmentName = photoResult.FileName,
                                AttachmentType = file.ContentType,
                                IsActive = true,
                                IsDelete = false
                            });
                        }

                        if (attachmentRows.Count > 0)
                        {
                            var attachmentRequest = new AssignmentAttachmentUpsertRequest
                            {
                                AssignmentID = assignmentId,
                                CompanyID = request.CompanyID,
                                SessionID = request.SessionID,
                                AttachmentsJson = JsonSerializer.Serialize(attachmentRows)
                            };

                            var attachmentRes = await _dailyAssignmentClient.UpsertAttachmentAsync(attachmentRequest);


                        }
                    }
                }
                return Json(res);
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [HttpPost]
        public async Task<IActionResult> DeleteDailyAssignments([FromBody] List<int> ids)
        {
            try
            {
                if (!(await _menuPerm.Has(MenuPath, "Delete")).Data)
                    return Json(new { success = false, message = "You do not have permission to delete homework." });

                var res = await _dailyAssignmentClient.DeleteAsync(ids);
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
