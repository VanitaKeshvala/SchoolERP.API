using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using System.Security.Claims;

namespace SchoolERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StudentLeaveApiController : ControllerBase
    {
        private readonly IStudentLeaveService _studentLeaveService;
        private readonly ICompanyService _companyService;
        private readonly ISessionService _sessionService;
        public StudentLeaveApiController(IStudentLeaveService studentLeaveService,
            ICompanyService companyService,
            ISessionService sessionService)
        {
            _studentLeaveService = studentLeaveService;
            _companyService = companyService;
            _sessionService = sessionService;
        }
        private int GetUserId() => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("UserId")?.Value, out var id) ? id : 0;
        private int GetCompanyId() => _companyService.GetUserCurrentCompany(GetUserId()) ?? 0;
        private int GetSessionId() => _sessionService.GetUserCurrentSession(GetUserId()) ?? 0;
        /// <summary>
        /// Retrieves student leave applications.
        /// </summary>
        /// <param name="classId">Class identifier.</param>
        /// <param name="sectionId">Section identifier.</param>
        /// <param name="status">Leave status.</param>
        /// <returns>List of leave applications.</returns>
        [HttpGet]
        [Route("GetLeaveApplications")]
        public IActionResult GetLeaveApplications(
            int? classId,
            int? sectionId,
            int? status)
        {
            try
            {
                int companyId = GetCompanyId();

                var result = _studentLeaveService.GetLeaveApplications(
                    classId,
                    sectionId,
                    status,
                    companyId);

                return Ok(new ApiResponse<List<StudentLeaveViewModel>>
                {
                    Success = true,
                    Data = result,
                    Message = "Data retrieved successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<List<StudentLeaveViewModel>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates the status of a student leave application.
        /// </summary>
        /// <param name="leaveAppId">The leave application identifier.</param>
        /// <param name="status">The new leave status.</param>
        /// <returns>The operation result.</returns>
        [HttpPost]
        [Route("UpdateLeaveStatus")]
        public IActionResult UpdateLeaveStatus(int leaveAppId, int status)
        {
            try
            {
                int companyId = GetCompanyId();

                int userId = Convert.ToInt32(
                    User.FindFirst("UserID")?.Value ?? "0");

                var result = _studentLeaveService.UpdateLeaveStatus(
                    leaveAppId,
                    status,
                    companyId,
                    userId);

                return Ok(new ApiResponse<object>
                {
                    Success = result.Success,
                    Message = result.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Creates or updates a student leave application.
        /// </summary>
        /// <param name="request">Leave application details.</param>
        /// <returns>Operation result.</returns>
        [HttpPost]
        [Route("UpsertLeaveApplication")]
        public IActionResult UpsertLeaveApplication(
            [FromBody] StudentLeaveUpsertRequest request)
        {
            try
            {
                int companyId = GetCompanyId();
                int userId = GetUserId();

                var result = _studentLeaveService.UpsertLeaveApplication(
                    request,
                    companyId,
                    userId);

                return Ok(new ApiResponse<object>
                {
                    Success = result.Success,
                    Message = result.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Downloads the attachment associated with a leave application.
        /// </summary>
        /// <param name="leaveAppId">The leave application identifier.</param>
        /// <returns>Attachment file.</returns>
        [HttpGet]
        [Route("GetLeaveAttachment")]
        public IActionResult GetLeaveAttachment(int leaveAppId)
        {
            try
            {
                int companyId = GetCompanyId();

                var result = _studentLeaveService.GetLeaveAttachment(
                    leaveAppId,
                    companyId);

                if (result.Bytes == null)
                    return NotFound("Attachment not found.");

                return File(
                    result.Bytes,
                    result.ContentType ?? "application/octet-stream",
                    result.FileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
