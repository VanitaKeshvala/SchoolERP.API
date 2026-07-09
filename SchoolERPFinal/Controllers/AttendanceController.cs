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

        public AttendanceController(
            IAttendanceClientService service,
            IClassClientService classService,
            ISectionClientService sectionService,
            ICompanyClientService companyService,
            ISessionClientService sessionService,
            IStudentLeaveClientService leaveService,
            IStudentInformationClientService studentService, PermissionHelper permHelper) : base(permHelper)
        {
            _service = service;
            _classService = classService;
            _sectionService = sectionService;
            _companyService = companyService;
            _sessionService = sessionService;
            _leaveService = leaveService;
            _studentService = studentService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("UserId")?.Value ?? "0");
        private async Task<int> GetCompanyId()
        {
            var response = await _companyService.GetUserCurrentCompanyAsync();
            return response?.Data ?? 0;
        }
        private async Task<int> GetSessionId()
        {
            var response = await _sessionService.GetUserCurrentSessionAsync();
            return response?.Data ?? 0;
        }
        /// <summary>
        /// Shows the 'Attendance' marking page where teachers can mark students as Present, Absent, or Late.
        /// </summary>
        public async Task<IActionResult> Attendance()
        {
            ViewBag.ClassList =(await _classService.GetAllAsync()).Data;
            return View();
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
            var result =await _service.SaveBulkAttendanceAsync(req);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> GetSectionsByClass(int classId)
        {
            var data =(await _sectionService.GetByClassAsync(classId)).Data;
            return Json(new { success = true, data });
        }

        /// <summary>
        /// Shows the 'Attendance Report' page to see how many students were present on specific dates.
        /// </summary>
        public async Task<IActionResult> Report()
        {
            ViewBag.ClassList = (await _classService.GetAllAsync()).Data;
            return View();
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
                   "/StudentInformation/DisabledStudents"
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

                var sessionId = await GetSessionId();
                var classesResponse = _service.GetAllLeaveApplicationsWithPageAsync(request);
                var companiesTask = _companyService.GetAllAsync();
                var classList = await _classService.GetAllAsync();
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
            var response = await _studentService.GetStudentListAsync(classId, sectionId, null);
            return Json(new { response.Success, response.Data });
        }

        [HttpPost]
        public async Task<IActionResult> UpsertLeave([FromForm] int LeaveAppID, [FromForm] int StudentID, [FromForm] DateTime FromDate, 
                                        [FromForm] DateTime ToDate, [FromForm] DateTime ApplyDate, [FromForm] string? Reason, 
                                        [FromForm] int Status, IFormFile? Attachment)
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

            if (Attachment != null)
            {
                using (var ms = new MemoryStream())
                {
                    Attachment.CopyTo(ms);
                    req.Attachment = ms.ToArray();
                    req.AttachmentName = Attachment.FileName;
                    req.AttachmentType = Attachment.ContentType;
                }
            }

            var result =await _leaveService.UpsertLeaveApplication(req);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> DownloadAttachment(int id)
        {
            //var (bytes, fileName, contentType) = await _leaveService.GetLeaveAttachment(id);
            var response = await _leaveService
         .GetLeaveAttachment(id);

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var bytes = await response.Content.ReadAsByteArrayAsync();

            var contentType =
                response.Content.Headers.ContentType?.MediaType
                ?? "application/octet-stream";

            var fileName =
                response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                ?? "Attachment";
            return File(bytes, contentType ?? "application/octet-stream", fileName ?? "attachment");
        }
    }
}
