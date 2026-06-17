using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Interfaces;
using SchoolERP.API.Models;
using SchoolERP.API.Models.Common;

namespace SchoolERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AttendanceApiController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        public AttendanceApiController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        /// <summary>
        /// Retrieves the attendance history of a specific student for the specified year.
        /// Returns monthly attendance summaries and daily attendance status details.
        /// </summary>
        /// <param name="studentId">
        /// The unique identifier of the student.
        /// </param>
        /// <param name="year">
        /// The academic year for which attendance history is required.
        /// </param>
        /// <param name="companyId">
        /// The unique identifier of the company/school.
        /// </param>
        /// <returns>
        /// Returns attendance summaries and daily attendance records.
        /// </returns>
        [HttpGet("GetStudentAttendanceHistory")]
        public IActionResult GetStudentAttendanceHistory(
            int studentId,
            int year,
            int companyId)
        {
            var result = _attendanceService.GetStudentAttendanceHistory(
                studentId,
                year,
                companyId);

            return Ok(new ApiResponse<StudentAttendanceHistoryViewModel>
            {
                Success = true,
                Message = "Attendance history retrieved successfully.",
                Data = result
            });
        }

        /// <summary>
        /// Retrieves the student attendance list.
        /// </summary>
        [HttpGet]
        [Route("GetStudentAttendanceList")]
        public IActionResult GetStudentAttendanceList(
            int classId,
            int sectionId,
            DateTime date,
            int companyId)
        {
            try
            {
                var result = _attendanceService.GetStudentAttendanceList(
                    classId,
                    sectionId,
                    date,
                    companyId);

                return Ok(ApiResponse<List<StudentAttendanceViewModel>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<List<StudentAttendanceViewModel>>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        /// <summary>
        /// Saves attendance records for multiple students.
        /// </summary>
        /// <param name="request">Attendance details.</param>
        /// <returns>Success or failure response.</returns>
        [HttpPost]
        [Route("SaveBulkAttendance")]
        public IActionResult SaveBulkAttendance([FromBody] AttendanceUpsertRequest request)
        {
            try
            {
                int companyId = Convert.ToInt32(User.FindFirst("CompanyID")?.Value ?? "0");
                int userId = Convert.ToInt32(User.FindFirst("UserID")?.Value ?? "0");

                var result = _attendanceService.SaveBulkAttendance(
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
    }
}
