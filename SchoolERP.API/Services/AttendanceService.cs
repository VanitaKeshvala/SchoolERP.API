using Dapper;
using Microsoft.Data.SqlClient;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using System.Data;
using System.Text.Json;

namespace SchoolERP.API.Services
{
    /// <summary>
    /// This service handles student attendance. 
    /// It allows teachers to mark attendance for a whole class and 
    /// keeps track of individual student's attendance history over time.
    /// </summary>
    public class AttendanceService: IAttendanceService
    {
        private readonly IConfiguration _configuration;
        public AttendanceService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ─── STUDENT ATTENDANCE ─────────────────────────────────

        /// <summary>
        /// Retrieves the attendance list for students in a specific class and section on a given date.
        /// </summary>
        /// <param name="classId">Class ID.</param>
        /// <param name="sectionId">Section ID.</param>
        /// <param name="date">Attendance date.</param>
        /// <param name="companyId">Company/School ID.</param>
        /// <returns>List of student attendance records.</returns>
        public List<StudentAttendanceViewModel> GetStudentAttendanceList(
            int classId,
            int sectionId,
            DateTime date,
            int companyId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var parameters = new DynamicParameters();
                parameters.Add("@Action", "LIST");
                parameters.Add("@ClassID", classId);
                parameters.Add("@SectionID", sectionId);
                parameters.Add("@AttendanceDate", date);
                parameters.Add("@CompanyID", companyId);

                return conn.Query<StudentAttendanceViewModel>(
                    "sp_Attendance_Student_CRUD",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
            catch
            {
                return new List<StudentAttendanceViewModel>();
            }
        }

        /// <summary>
        /// Saves attendance records for all students in a class at once.
        /// </summary>
        /// <param name="req">Attendance information.</param>
        /// <param name="companyId">Company/School ID.</param>
        /// <param name="userId">Logged-in user ID.</param>
        /// <returns>Operation status and message.</returns>
        public (bool Success, string Message) SaveBulkAttendance(
            AttendanceUpsertRequest req,
            int companyId,
            int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string jsonData = JsonSerializer.Serialize(req.AttendanceData);

                var parameters = new DynamicParameters();
                parameters.Add("@Action", "SAVE_BULK");
                parameters.Add("@ClassID", req.ClassID);
                parameters.Add("@SectionID", req.SectionID);
                parameters.Add("@AttendanceDate", req.AttendanceDate);
                parameters.Add("@CompanyID", companyId);
                parameters.Add("@UserID", userId);
                parameters.Add("@JsonData", jsonData);

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Attendance_Student_CRUD",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return (
                    result?.Result == 1,
                    result?.Message ?? "Operation completed."
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Retrieves attendance history and monthly summaries for a student.
        /// </summary>
        /// <param name="studentId">Student ID.</param>
        /// <param name="year">Academic year.</param>
        /// <param name="companyId">Company/School ID.</param>
        /// <returns>Attendance history information.</returns>
        public StudentAttendanceHistoryViewModel GetStudentAttendanceHistory(
            int studentId,
            int year,
            int companyId)
        {
            var result = new StudentAttendanceHistoryViewModel();

            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var parameters = new DynamicParameters();
                parameters.Add("@Action", "GET_HISTORY");
                parameters.Add("@StudentID", studentId);
                parameters.Add("@Year", year);
                parameters.Add("@CompanyID", companyId);

                using var multi = conn.QueryMultiple(
                    "sp_Attendance_Student_CRUD",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                result.Summaries = multi.Read<StudentAttendanceSummary>().ToList();
                result.Days = multi.Read<StudentAttendanceDayStatus>().ToList();
            }
            catch
            {
                // Ignore exception and return empty result
            }

            return result;
        }
    }
}
