using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface IAttendanceClientService
    {
        Task<ApiResponse<StudentAttendanceHistoryViewModel>> GetStudentAttendanceHistoryAsync(
            int studentId,
            int year,
            int companyId);
        /// <summary>
        /// Retrieves the student attendance list.
        /// </summary>
        Task<ApiResponse<List<StudentAttendanceViewModel>>> GetStudentAttendanceList(
            int classId,
            int sectionId,
            DateTime date,
            int companyId);

        Task<ApiResponse<object>> SaveBulkAttendanceAsync(
            AttendanceUpsertRequest request);

        Task<ApiResponse<PagedResult<StudentLeaveViewModel>>> GetAllLeaveApplicationsWithPageAsync(StudentLeaveSearchRequest request);
        Task<ApiResponse<PagedResult<StudentAttendanceViewModel>>> GetAllStudentAttendanceWithPageAsync(StudentAttendanceSearchRequest request);
    }
}
