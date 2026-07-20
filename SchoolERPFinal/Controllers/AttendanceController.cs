using Microsoft.AspNetCore.Mvc;
using SchoolERP.Shared.Models;
using SchoolERP.Net.Services;
using SchoolERP.Net.Services.Clients;
using SchoolERP.Net.Helpers;

namespace SchoolERP.Net.Controllers
{
    /// <summary>
    /// This controller manages student attendance and leave requests.
    /// It provides pages for marking attendance, viewing attendance reports, 
    /// and approving leave applications from students.
    /// </summary>
    public class AttendanceController : BaseController
    {
        private readonly IAttendanceClientService _service;
        private readonly IClassClientService _classService;
        private readonly ISectionClientService _sectionService;
        private readonly ICompanyClientService _companyService;
        private readonly ISessionClientService _sessionService;
        private readonly IStudentLeaveClientService _leaveService;
        private readonly IStudentInformationClientService _studentService;

        private readonly IConfiguration _configuration;
        private readonly IPhotoUploadService _photoService;
        private readonly IWebHostEnvironment _environment;
        public AttendanceController(
            IAttendanceClientService service,
            IClassClientService classService,
            ISectionClientService sectionService,
            ICompanyClientService companyService,
            ISessionClientService sessionService,
            IStudentLeaveClientService leaveService,
            IStudentInformationClientService studentService, PermissionHelper permHelper,
            IConfiguration configuration, IPhotoUploadService photoService, IWebHostEnvironment environment) : base(permHelper)
        {
            _service = service;
            _classService = classService;
            _sectionService = sectionService;
            _companyService = companyService;
            _sessionService = sessionService;
            _leaveService = leaveService;
            _studentService = studentService;
            _configuration = configuration;
            _photoService = photoService;
            _environment = environment;
        }

        private int GetUserId() => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("UserId")?.Value ?? "0");
        private int GetStaffID()
        {
            var staffClaim = User.FindFirst("StaffID")?.Value;
            return int.TryParse(staffClaim, out var staffId) ? staffId : 0;
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
            var response = await _sessionService.GetUserCurrentSessionAsync();
            return response?.Data ?? 0;
        }
        /// <summary>
        /// Shows the 'Attendance' marking page where teachers can mark students as Present, Absent, or Late.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Attendance(int? classId, int? sectionId, DateTime? date,
        int? pageIndex,
        int? pageSize,
        string? search,
        int? companyId,
        int? studentId)
        {

            var model = new StudentAttendancePageViewModel();
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Attendance/ApproveLeave"
               );

                var request = new StudentAttendanceSearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    ClassID = classId ?? null,
                    SectionID = sectionId ?? null,
                    StudentID = studentId,
                    AttendanceDate = date ?? DateTime.Now
                };
                var staffID = GetStaffID();
                var sessionId = await GetSessionId();
                var classesResponse = _service.GetAllStudentAttendanceWithPageAsync(request);
                var companiesTask = _companyService.GetAllAsync();
                var classList = await _classService.GetAllAsync(false,await GetSessionId(),await GetCompanyId(), staffID);
                await Task.WhenAll(classesResponse, companiesTask);

                var pagedResult = await classesResponse;

                model = new StudentAttendancePageViewModel
                {
                    StudentAttendanceModel = pagedResult.Success ? pagedResult.Data.Data : new List<StudentAttendanceViewModel>(),
                    Classes = classList.Data ?? new(),
                    Companies = (await companiesTask).Data ?? new(),
                    TotalRecords = pagedResult.Data.TotalRecords,
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,
                    SearchTerm = search,
                    CompanyId = companyId,
                    SectionID = sectionId,
                    ClassId = classId
                };
                model.Permissions = perms;
                return View(model);

            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAttendanceList(int classId, int sectionId, DateTime date)
        {
            var res =await _service.GetStudentAttendanceList(classId, sectionId, date, await GetCompanyId());
            return Json(new { res.Success, res.Data });
        }

        [HttpPost]
        public async Task<IActionResult> SaveAttendance([FromBody] AttendanceUpsertRequest req)
        {
            try
            {
                req.CompanyID = await GetCompanyId();
                var result = await _service.SaveBulkAttendanceAsync(req);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }            
        }

        [HttpGet]
        public async Task<IActionResult> GetSectionsByClass(int classId)
        {
            var staffId = GetStaffID();
            var data =(await _sectionService.GetByClassAsync(classId, staffId)).Data;
            return Json(new { success = true, data });
        }

        /// <summary>
        /// Shows the 'Attendance Report' page to see how many students were present on specific dates.
        /// </summary>
        
        [HttpGet]
        public async Task<IActionResult> Report(int? classId, int? sectionId, DateTime? date,
       int? pageIndex,
       int? pageSize,
       string? search,
       int? companyId,
       int? studentId)
        {

            var model = new StudentAttendancePageViewModel();
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Attendance/ApproveLeave"
               );

                var request = new StudentAttendanceSearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    ClassID = classId ?? null,
                    SectionID = sectionId ?? null,
                    StudentID = studentId,
                    AttendanceDate = date ?? DateTime.Now
                };
                var staffId = GetStaffID();
                var sessionId = await GetSessionId();
                var classesResponse = _service.GetAllStudentAttendanceWithPageAsync(request);
                var companiesTask = _companyService.GetAllAsync();
                var classList = await _classService.GetAllAsync(false,await GetSessionId(),await GetCompanyId(),staffId);
                await Task.WhenAll(classesResponse, companiesTask);

                var pagedResult = await classesResponse;

                model = new StudentAttendancePageViewModel
                {
                    StudentAttendanceModel = pagedResult.Success ? pagedResult.Data.Data : new List<StudentAttendanceViewModel>(),
                    Classes = classList.Data ?? new(),
                    Companies = (await companiesTask).Data ?? new(),
                    TotalRecords = pagedResult.Data.TotalRecords,
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,
                    SearchTerm = search,
                    CompanyId = companyId,
                    SectionID = sectionId,
                    ClassId = classId
                };
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
        /// Shows the 'Approve Leave' page where administrators can review and approve student leave requests.
        /// </summary>
        public async Task<IActionResult> ApproveLeave(int? pageIndex,
        int? pageSize,
        string? search,
        int? companyId,
        int? classId,
        int? sectionId,
        int? status)
        {
            var model = new StudentLeavePageViewModel();
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Attendance/ApproveLeave"
               );

                var request = new StudentLeaveSearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    ClassID =classId??null,
                    SectionID = sectionId ?? null,
                    Status=status
                };
                var staffId = GetStaffID();
                var sessionId = await GetSessionId();
                var classesResponse = _service.GetAllLeaveApplicationsWithPageAsync(request);
                var companiesTask = _companyService.GetAllAsync();
                var classList = await _classService.GetAllAsync(false,await GetSessionId(),await GetCompanyId(), staffId);
                await Task.WhenAll(classesResponse, companiesTask);

                var pagedResult = await classesResponse;

                model = new StudentLeavePageViewModel
                {
                    StudentLeaveModel = pagedResult.Success ? pagedResult.Data.Data : new List<StudentLeaveViewModel>(),
                    Classes = classList.Data ?? new(),
                    Companies = (await companiesTask).Data ?? new(),
                    TotalRecords = pagedResult.Data.TotalRecords,
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,
                    SearchTerm = search,
                    CompanyId = companyId,
                    SectionID =sectionId,
                    ClassId=classId
                };
                model.Permissions = perms;
                return View(model);

            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
            
        }


        //Add Leave Request
        public async Task<IActionResult> AddLeaveRequest(int? id)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Attendance/AddLeaveRequest"
               );
                var staffId = GetStaffID();
                var sessionId = await GetSessionId();
                var classResponce = await _classService.GetAllAsync(false, await GetSessionId(), await GetCompanyId(), staffId);
                var model = new StudentLeaveAddViewModel();


                if (id.HasValue && id.Value > 0)
                {
                    var response = await _leaveService.GetLeaveApplicationsById(id.Value,await GetCompanyId());
                    if (response.Success)
                    {                        
                        model.EditStudentLeave = response.Data;
                    }
                }
                else
                {
                    model.EditStudentLeave = null;
                }
                model.Classes = classResponce.Data;
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetLeaveList(int? classId, int? sectionId, int? status)
        {
            var response =await _leaveService.GetLeaveApplications(classId, sectionId, status);
            return Json(new { response.Success, response.Data });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateLeaveStatus([FromBody] LeaveStatusUpdateRequest req)
        {
            var result = await _leaveService.UpdateLeaveStatus(req.LeaveAppID, req.Status);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentsForLeave(int classId, int sectionId)
        {
            try
            {
                var reqModel = new StudentDropDwonBindRequestModel
                {
                    SessionID = await GetSessionId(),
                    CompanyID = await GetCompanyId(),
                    ClassID = classId,
                    SectionID = sectionId,
                    UserId = GetUserId()
                };
                var sessionId = await GetSessionId();
                var companyId = await GetSessionId();                
                var response = await _studentService.GetStudentBindAsync(reqModel);
                return Json(new { response.Success, response.Data });
            }
            catch (Exception ex)
            {
                throw;
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> UpsertLeave([FromForm] int LeaveAppID, [FromForm] int StudentID, [FromForm] DateTime FromDate, 
                                        [FromForm] DateTime ToDate, [FromForm] DateTime ApplyDate, [FromForm] string? Reason, 
                                        [FromForm] int Status, [FromForm] IFormFile? Attachment)
        {
            var req = new StudentLeaveUpsertRequest
            {
                LeaveAppID = LeaveAppID,
                StudentID = StudentID,
                FromDate = FromDate,
                ToDate = ToDate,
                ApplyDate = ApplyDate,
                Reason = Reason,
                Status = Status
            };           

            var result =await _leaveService.UpsertLeaveApplication(req);

            if (Attachment != null && Attachment.Length > 0)
            {
                long MaxFileSizeMB = _configuration.GetValue<long>("FileUploadSettings:MaxFileSizeMB");
                long maxBytes = MaxFileSizeMB * 1024L * 1024L;

                if (Attachment.Length > maxBytes)
                {
                    return Json(new { success = false, message = $"File size exceeds the {MaxFileSizeMB} MB limit." });
                }

                req.AttachmentName = Attachment.FileName;
                req.AttachmentType = Attachment.ContentType;
            }
            if (result.Data?.Result?.Result == 1) 
            {
                int? leaveAppID = result.Data.Result.LeaveAppID;
                if (Attachment != null && Attachment.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await Attachment.CopyToAsync(memoryStream);

                    byte[] fileBytes = memoryStream.ToArray();
                    PhotoUploadResult photoResult = new PhotoUploadResult();
                    if (req.AttachmentName != null)
                    {
                        photoResult = await _photoService.SaveBase64PhotoAsync(
                            Convert.ToBase64String(fileBytes),
                            req.AttachmentName ?? "photo.jpg",
                            PhotoModule.LeaveApp,
                            FolderNameModule.Documents,
                            leaveAppID.Value
                        );
                        var attchment = new LeaveApplicationAttachmentUpsertRequest
                        {
                            LeaveAppId = leaveAppID.Value,
                            Attachment = photoResult.PhotoUrl,
                            FileName = photoResult.FileName,
                            FileType = Attachment.ContentType

                        };
                        var attachment = await _leaveService.UpsertLeaveApplicationAttachmentFileAsync(attchment);
                    }
                }
            }
                return Json(new { success = result.Success, message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> DownloadAttachment(int id)
        {
            //var (bytes, fileName, contentType) = await _leaveService.GetLeaveAttachment(id);
           var r = await _leaveService.GetLeaveApplicationsById(id, await GetCompanyId());

            if (!r.Success || string.IsNullOrWhiteSpace(r.Data?.Attachment))
                return NotFound();

            // Full physical path
            var filePath = Path.Combine(
                _environment.WebRootPath,
                r.Data.Attachment.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
            );

            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found.");

            var fileName = Path.GetFileName(filePath);

            return PhysicalFile(
                filePath,
                "application/octet-stream",
                fileName);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLeaveApplication([FromBody] List<int> ids)
        {
            try
            {
                var res = await _leaveService.DeleteLeaveApplicationAsync(ids, await GetCompanyId());
                return Json(new { success = true, message = res.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            
        }
    }
}
