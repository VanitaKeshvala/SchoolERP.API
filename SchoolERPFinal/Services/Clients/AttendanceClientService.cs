using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class AttendanceClientService:BaseApiClient, IAttendanceClientService
    {
        public AttendanceClientService(HttpClient httpClient) : base(httpClient)
        {
        }
        /// <summary>
        /// Retrieves attendance history for the specified student from the API.
        /// </summary>
        /// <param name="studentId">
        /// The unique identifier of the student.
        /// </param>
        /// <param name="year">
        /// The year for which attendance history is required.
        /// </param>
        /// <param name="companyId">
        /// The unique identifier of the company/school.
        /// </param>
        /// <returns>
        /// Returns attendance summaries and daily attendance details.
        /// </returns>
        public async Task<ApiResponse<StudentAttendanceHistoryViewModel>> GetStudentAttendanceHistoryAsync(
            int studentId,
            int year,
            int companyId)
        {
            return await GetAsync<StudentAttendanceHistoryViewModel>(
                $"api/AttendanceApi/GetStudentAttendanceHistory?studentId={studentId}&year={year}&companyId={companyId}");
        }

        /// <summary>
        /// Retrieves the student attendance list.
        /// </summary>
        public async Task<ApiResponse<List<StudentAttendanceViewModel>>> GetStudentAttendanceList(
            int classId,
            int sectionId,
            DateTime date,
            int companyId)
        {
            return await GetAsync<List<StudentAttendanceViewModel>>(
                $"api/AttendanceApi/GetStudentAttendanceList?" +
                $"classId={classId}" +
                $"&sectionId={sectionId}" +
                $"&date={date:yyyy-MM-dd}" +
                $"&companyId={companyId}");
        }

        /// <summary>
        /// Saves attendance records for multiple students.
        /// </summary>
        /// <param name="request">Attendance details.</param>
        /// <returns>API response.</returns>
        public async Task<ApiResponse<object>> SaveBulkAttendanceAsync(
            AttendanceUpsertRequest request)
        {
            return await PostAsync<object>(
                "api/AttendanceApi/SaveBulkAttendance",
                request);
        }

        public async Task<ApiResponse<PagedResult<StudentLeaveViewModel>>> GetAllLeaveApplicationsWithPageAsync(StudentLeaveSearchRequest request)
        {
            return await PostAsync<PagedResult<StudentLeaveViewModel>>($"api/StudentLeaveApi/GetAllLeaveApplicationsWithPage", request);
        }

        public async Task<ApiResponse<PagedResult<StudentAttendanceViewModel>>> GetAllStudentAttendanceWithPageAsync(StudentAttendanceSearchRequest request)
        {
            return await PostAsync<PagedResult<StudentAttendanceViewModel>>($"api/AttendanceApi/GetAllStudentAttendanceWithPage", request);
        }

        public async Task<ApiResponse<PagedResult<StudentAttendanceViewModel>>> GetAllStudentAttendanceReportWithPageAsync(StudentAttendanceSearchRequest request)
        {
            return await PostAsync<PagedResult<StudentAttendanceViewModel>>($"api/AttendanceApi/GetAllStudentAttendanceReportWithPage", request);
        }
    }
}
