using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Interfaces;
using SchoolERP.API.Models;
using SchoolERP.API.Models.Common;
using System.Security.Claims;

namespace SchoolERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize]
    /// <summary>
    /// This is a technical 'API' controller that provides data to the website.
    /// It handles background requests like getting student lists, updating statuses, 
    /// and managing student categories without reloading the whole page.
    /// Think of it as a behind-the-scenes data provider.
    /// </summary>
    public class StudentInformationApiController : ControllerBase
    {
        private readonly IStudentInformationService _studentService;
        private readonly ICompanyService _companyService;
        private readonly ISessionService _sessionService;
        private readonly IAttendanceService _attendanceService;

        public StudentInformationApiController(IStudentInformationService studentService,
            ICompanyService companyService,
            ISessionService sessionService,
            IAttendanceService attendanceService)
        {
            _studentService = studentService;
            _companyService = companyService;
            _sessionService = sessionService;
            _attendanceService = attendanceService;
        }

        private int GetUserId() => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("UserId")?.Value, out var id) ? id : 0;
        private int GetCompanyId() => _companyService.GetUserCurrentCompany(GetUserId()) ?? 0;
        private int GetSessionId() => _sessionService.GetUserCurrentSession(GetUserId()) ?? 0;

        [HttpGet("GetStudentAttendanceHistory/{studentId}/{year}")]
        /// <summary>
        /// Provides the attendance record for a student (how many days present/absent).
        /// </summary>
        public IActionResult GetStudentAttendanceHistory(int studentId, int year)
        {
            try
            {
                var data = _attendanceService.GetStudentAttendanceHistory(studentId, year, GetCompanyId());
                return Ok(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpGet("GetAllDisableReasons")]
        /// <summary>
        /// Provides the list of all reasons why students are disabled.
        /// </summary>
        public IActionResult GetAllDisableReasons()
        {
            var data = _studentService.GetAllDisableReasons(GetCompanyId(), GetSessionId());
            return Ok(new { success = true, data });
        }

        [HttpPost("UpsertDisableReason")]
        public IActionResult UpsertDisableReason([FromBody] StudentDisableReasonUpsertRequest req)
        {
            var res = _studentService.UpsertDisableReason(req, GetCompanyId(), GetSessionId(), GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        [HttpPost("DeleteDisableReason")]
        public IActionResult DeleteDisableReason(List<int> ids)
        {
            var res = _studentService.DeleteDisableReason(ids, GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        [HttpGet("GetAllStudentHouses")]
        /// <summary>
        /// Provides the list of all student houses.
        /// </summary>
        public IActionResult GetAllStudentHouses()
        {
            var data = _studentService.GetAllStudentHouses(GetCompanyId(), GetSessionId());
            return Ok(new { success = true, data });
        }

        [HttpPost("UpsertStudentHouse")]
        public IActionResult UpsertStudentHouse([FromBody] StudentHouseUpsertRequest req)
        {
            var res = _studentService.UpsertStudentHouse(req, GetCompanyId(), GetSessionId(), GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        [HttpPost("DeleteStudentHouse")]
        public IActionResult DeleteStudentHouse(List<int> ids)
        {
            var res = _studentService.DeleteStudentHouse(ids, GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        [HttpGet("GetAllStudentCategories")]
        /// <summary>
        /// Provides the list of all student categories.
        /// </summary>
        public IActionResult GetAllStudentCategories()
        {
            var data = _studentService.GetAllStudentCategories(GetCompanyId(), GetSessionId());
            return Ok(new { success = true, data });
        }

        [HttpPost("UpsertStudentCategory")]
        public IActionResult UpsertStudentCategory([FromBody] StudentCategoryUpsertRequest req)
        {
            var res = _studentService.UpsertStudentCategory(req, GetCompanyId(), GetSessionId(), GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        [HttpPost("DeleteStudentCategory")]
        public IActionResult DeleteStudentCategory(List<int> ids)
        {
            var res = _studentService.DeleteStudentCategory(ids, GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        [HttpGet("GetStudentTimeline/{studentId}")]
        public IActionResult GetStudentTimeline(int studentId)
        {
            try
            {
                var data = _studentService.GetStudentTimeline(studentId);
                return Ok(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Database Error: " + ex.Message });
            }
        }

        [HttpPost("UpsertTimeline")]
        public IActionResult UpsertTimeline([FromBody] StudentTimelineUpsertRequest req)
        {
            try
            {
                var result = _studentService.UpsertStudentTimeline(req, GetCompanyId(), GetSessionId(), GetUserId());
                return Ok(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Database Error: " + ex.Message });
            }
        }

        [HttpPost("DeleteTimeline/{id}")]
        public IActionResult DeleteTimeline(int id)
        {
            try
            {
                var result = _studentService.DeleteStudentTimeline(id, GetUserId());
                return Ok(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Database Error: " + ex.Message });
            }
        }

        [HttpGet("DownloadTimelineDoc/{id}")]
        public IActionResult DownloadTimelineDoc(int id)
        {
            var (bytes, fileName, contentType) = _studentService.GetStudentTimelineDocument(id);
            if (bytes == null) return NotFound();
            return File(bytes, contentType, fileName);
        }
        [HttpPost("ToggleStatus")]
        /// <summary>
        /// Changes a student's status (e.g., from 'Active' to 'Disabled').
        /// </summary>
        public IActionResult ToggleStatus([FromBody] StudentStatusToggleRequest req)
        {
            var res = _studentService.ToggleStudentStatus(req, GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        [HttpGet("GetByID/{id}")]
        public IActionResult GetByID(int id)
        {
            try
            {
                var data = _studentService.GetStudentDetails(id, GetCompanyId(), GetSessionId());
                return Ok(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpGet("GetMultiClassStudents")]
        public IActionResult GetMultiClassStudents(int? classId, int? sectionId, string? searchTerm)
        {
            var data = _studentService.GetMultiClassStudents(GetCompanyId(), GetSessionId(), classId, sectionId, searchTerm);
            return Ok(new { success = true, data });
        }

        [HttpGet("GetStudentList")]
        public IActionResult GetStudentList(int? classId, int? sectionId, string? searchTerm)
        {
            var data = _studentService.GetStudentList(GetCompanyId(), GetSessionId(), classId, sectionId, searchTerm);
            return Ok(new { success = true, data });
        }

        [HttpPost("UpsertMultiClass")]
        public IActionResult UpsertMultiClass([FromBody] StudentMultiClassUpsertRequest req)
        {
            var res = _studentService.UpsertStudentMultiClass(req, GetCompanyId(), GetSessionId(), GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        [HttpPost("DeleteMultiClass/{id}")]
        public IActionResult DeleteMultiClass(int id)
        {
            var res = _studentService.DeleteStudentMultiClass(id, GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        /// <summary>
        /// Generates the next student roll number.
        /// </summary>
        /// <param name="companyId">Company ID.</param>
        /// <param name="sessionId">Session ID.</param>
        /// <param name="dynamicValues">Optional dynamic field values.</param>
        /// <returns>Generated student roll number.</returns>
        [HttpPost("GetNewStudentRollNo")]
        public IActionResult GetNewStudentRollNo(Dictionary<string, string>? dynamicValues = null)
        {
            try
            {
                var rollNo = _studentService.GetNewStudentRollNo(
                    GetCompanyId(), GetSessionId(),
                    dynamicValues);

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Roll number generated successfully.",
                    Data = rollNo
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = string.Empty
                });
            }
        }

        /// <summary>
        /// Generates the next available admission number.
        /// </summary>
        /// <returns>Next admission number.</returns>
        [HttpGet("GetNextAdmissionNo")]
        public IActionResult GetNextAdmissionNo()
        {
            try
            {
                var admissionNo = _studentService.GetNextSimpleAdmissionNo(
                    GetCompanyId());

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Admission number generated successfully.",
                    Data = admissionNo
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = string.Empty
                });
            }
        }

        /// <summary>
        /// Creates or updates a student admission record.
        /// </summary>
        /// <param name="request">Student admission details.</param>
        /// <returns>Operation result with Student ID.</returns>
        [HttpPost("UpsertStudentAdmission")]
        public IActionResult UpsertStudentAdmission(
            [FromBody] StudentAdmissionUpsertRequest request)
        {
            try
            {
                var result = _studentService.UpsertStudentAdmission(
                    request,
                    GetCompanyId(),
                    GetSessionId(),
                    GetUserId());

                return Ok(new ApiResponse<int>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.StudentID
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<int>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = 0
                });
            }
        }

        /// <summary>
        /// Deletes multiple students at once.
        /// </summary>
        /// <param name="studentIds">List of Student IDs.</param>
        /// <returns>Operation status and message.</returns>
        [HttpPost("BulkDeleteStudents")]
        public IActionResult BulkDeleteStudents([FromBody] List<int> studentIds)
        {
            try
            {
                var result = _studentService.BulkDeleteStudents(
                    studentIds,
                    GetUserId());

                return Ok(new ApiResponse<object>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        /// <summary>
        /// Retrieves the list of disabled students.
        /// </summary>
        /// <param name="classId">Class ID (optional).</param>
        /// <param name="sectionId">Section ID (optional).</param>
        /// <param name="searchTerm">Search term (optional).</param>
        /// <returns>List of disabled students.</returns>
        [HttpGet("GetDisabledStudentList")]
        public IActionResult GetDisabledStudentList(
            int? classId = null,
            int? sectionId = null,
            string? searchTerm = null)
        {
            try
            {
                var data = _studentService.GetDisabledStudentList(
                    GetCompanyId(),
                    GetSessionId(),
                    classId,
                    sectionId,
                    searchTerm);

                return Ok(new ApiResponse<List<StudentListViewModel>>
                {
                    Success = true,
                    Message = "Disabled student list retrieved successfully.",
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<List<StudentListViewModel>>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = new List<StudentListViewModel>()
                });
            }
        }




    }
}
