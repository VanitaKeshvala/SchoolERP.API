using Dapper;
using Microsoft.Data.SqlClient;
using SchoolERP.API.Data;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using System.Data;

namespace SchoolERP.API.Services
{
    /// <summary>
    /// This service manages the academic schedule and assignments.
    /// It handles the school timetable (when and where classes happen) and 
    /// keeps track of which teacher is assigned to which class.
    /// </summary>
    public class AcademicsService: IAcademicsService
    {
        private readonly IConfiguration _configuration;
        public AcademicsService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Retrieves the timetable records for a specific class and section.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="sessionId">The academic session identifier.</param>
        /// <param name="classId">The class identifier.</param>
        /// <param name="sectionId">The section identifier.</param>
        /// <returns>A list of <see cref="TimeTableViewModel"/> for the given class and section.</returns>
        public List<TimeTableViewModel> GetTimeTableByClass(int companyId, int sessionId, int classId, int sectionId)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();
            parameters.Add("@CompanyID", companyId);
            parameters.Add("@SessionID", sessionId);
            parameters.Add("@ClassID", classId);
            parameters.Add("@SectionID", sectionId);

            return conn.Query<TimeTableViewModel>(
                "SP_ACD_TIMETABLE_GETBYCLASS",
                parameters,
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        /// <summary>
        /// Retrieves the timetable entries for a specific staff member filtered by company and session.
        /// </summary>
        /// <param name="companyId">The ID of the company.</param>
        /// <param name="sessionId">The ID of the academic session.</param>
        /// <param name="staffId">The ID of the staff member.</param>
        /// <returns>A list of <see cref="TimeTableViewModel"/> representing the staff's timetable.</returns>
        public List<TimeTableViewModel> GetTimeTableByStaff(int companyId, int sessionId, int staffId)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();
            parameters.Add("@CompanyID", companyId);
            parameters.Add("@SessionID", sessionId);
            parameters.Add("@StaffID", staffId);

            return conn.Query<TimeTableViewModel>(
                    "SP_ACD_TIMETABLE_GETBYSTAFF",
                    parameters,
                    commandType: CommandType.StoredProcedure)
                .ToList();
        }

        /// <summary>
        /// Inserts or updates a timetable record by executing the SP_ACD_TIMETABLE_UPSERT stored procedure.
        /// </summary>
        /// <param name="req">The timetable upsert request containing timetable details.</param>
        /// <param name="companyId">The ID of the company.</param>
        /// <param name="sessionId">The ID of the academic session.</param>
        /// <param name="userId">The ID of the user performing the operation.</param>
        /// <returns>
        /// A tuple where <c>success</c> is <c>true</c> if the upsert succeeded,
        /// and <c>message</c> contains the result message from the stored procedure.
        /// </returns>
        public (bool success, string message) UpsertTimeTable(
            TimeTableUpsertRequest req, int companyId, int sessionId, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var parameters = new DynamicParameters();
                parameters.Add("@TimeTableID", req.TimeTableID);
                parameters.Add("@CompanyID", companyId);
                parameters.Add("@SessionID", sessionId);
                parameters.Add("@ClassID", req.ClassID);
                parameters.Add("@SectionID", req.SectionID);
                parameters.Add("@SubjectID", req.SubjectID);
                parameters.Add("@StaffID", req.StaffID);
                parameters.Add("@Day", req.Day);
                parameters.Add("@StartTime", req.StartTime);
                parameters.Add("@EndTime", req.EndTime);
                parameters.Add("@RoomNo", req.RoomNo);   // Dapper handles null → DBNull automatically
                parameters.Add("@UserId", userId);

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "SP_ACD_TIMETABLE_UPSERT",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return result is not null
                    ? (result.Result > 0, result.Message ?? string.Empty)
                    : (false, "No response from stored procedure.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Deletes a timetable slot by executing the stored procedure SP_ACD_TIMETABLE_DELETE.
        /// </summary>
        /// <param name="id">The ID of the timetable slot to delete (mapped to @TimeTableID).</param>
        /// <param name="userId">The ID of the user performing the deletion (mapped to @UserId).</param>
        /// <returns>
        /// A tuple where <c>success</c> is <c>true</c> if Result > 0, and <c>message</c> contains the SP response message.
        /// On exception, returns <c>(false, exception message)</c>.
        /// </returns>
        public (bool success, string message) DeleteTimeTableSlot(int id, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault(
                    "SP_ACD_TIMETABLE_DELETE",
                    new { TimeTableID = id, UserId = userId },
                    commandType: CommandType.StoredProcedure);

                return (Convert.ToInt32(result.Result) > 0, (string)result.Message);
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        /// <summary>
        /// Retrieves all class teachers for the specified company and session.
        /// </summary>
        /// <param name="companyId">The ID of the company.</param>
        /// <param name="sessionId">The ID of the academic session.</param>
        /// <returns>A list of <see cref="ClassTeacherViewModel"/> representing all class teachers.</returns>
        public List<ClassTeacherViewModel> GetAllClassTeachers(int companyId, int sessionId, List<int> classId=null, List<int> sectionId=null)
        {
            try
            {
                using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

                string classIds = null;
                string sectionIds = null;
                if (classId != null && classId.Count != 0)
                {
                    classIds = string.Join(",", classId);
                }
                if (sectionId != null && sectionId.Count != 0)
                {
                    sectionIds = string.Join(",", sectionId);
                }

                var parameters = new DynamicParameters();
                parameters.Add("@CompanyID", companyId);
                parameters.Add("@SessionID", sessionId);
                parameters.Add("@CLASSIDS", classIds);
                parameters.Add("@SECTIONIDS", sectionIds);

                return conn.Query<ClassTeacherViewModel>(
                    "sp_Academics_ClassTeacher_GetAll",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
            
        }

        /// <summary>
        /// Upserts a class teacher assignment by executing the stored procedure
        /// <c>sp_Academics_ClassTeacher_Upsert</c>.
        /// </summary>
        /// <param name="req">The upsert request containing ClassID, SectionID, and StaffIDs.</param>
        /// <param name="companyId">The ID of the company.</param>
        /// <param name="sessionId">The ID of the academic session.</param>
        /// <param name="userId">The ID of the user performing the operation.</param>
        /// <returns>
        /// A tuple where <c>success</c> is <c>true</c> if the operation affected rows,
        /// and <c>message</c> contains the result message from the stored procedure.
        /// </returns>
        public (bool success, string message) UpsertClassTeacher(
            ClassTeacherUpsertRequest req, int companyId, int sessionId, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var parameters = new DynamicParameters();
                parameters.Add("@CompanyID", companyId);
                parameters.Add("@SessionID", sessionId);
                parameters.Add("@ClassID", req.ClassID);
                parameters.Add("@SectionID", req.SectionID);
                parameters.Add("@StaffIDs", string.Join(",", req.StaffIDs));
                parameters.Add("@UserId", userId);

                // Dapper auto-maps SP result columns → anonymous type — no foreach needed
                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Academics_ClassTeacher_Upsert",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                if (result == null)
                    return (false, "No response from stored procedure.");

                return result is not null
                   ? (result.Result > 0, result.Message ?? string.Empty)
                   : (false, "No response from stored procedure.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
       
        /// <summary>
        /// Deletes a class teacher record by executing a stored procedure.
        /// </summary>
        /// <param name="id">The ClassTeacher ID to delete.</param>
        /// <param name="userId">The ID of the user performing the deletion.</param>
        /// <returns>A tuple indicating success status and a message.</returns>
        public (bool success, string message) DeleteClassTeacher(List<int> id, int userId)
        {
            try
            {
                if (id == null || !id.Any())
                {
                    return (false, "No Class Teacher selected for deletion.");
                }

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string classTeacherID = string.Join(",", id);
                var parameters = new DynamicParameters();
                parameters.Add("@ClassTeacherID", classTeacherID);
                parameters.Add("@UserId", userId);


                var result = conn.QueryFirstOrDefault<SpResult>(
                   "sp_Academics_ClassTeacher_Delete",
                   parameters,
                   commandType: CommandType.StoredProcedure);

                return (
                    result?.Result == 1,
                    result?.Message ?? "Operation completed."
                );

            }
            catch (Exception ex) { return (false, ex.Message); }
        }
       
        /// <summary>
        /// Retrieves a list of students eligible for promotion based on company, session, class, and section.
        /// </summary>
        /// <param name="companyId">The ID of the company.</param>
        /// <param name="sessionId">The ID of the academic session.</param>
        /// <param name="classId">The ID of the class.</param>
        /// <param name="sectionId">The ID of the section.</param>
        /// <returns>A list of <see cref="StudentPromotionViewModel"/> representing students eligible for promotion.</returns>
        public List<StudentPromotionViewModel> GetStudentsForPromotion(int companyId, int sessionId, int classId, int sectionId)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var parameters = new
            {
                CompanyID = companyId,
                SessionID = sessionId,
                ClassID = classId,
                SectionID = sectionId
            };

            return conn.Query<StudentPromotionViewModel>(
                "sp_Academics_Students_GetForPromotion",
                parameters,
                commandType: CommandType.StoredProcedure
            ).ToList();
        }
       
        /// <summary>
        /// Promotes a list of students to the next session/class/section using a stored procedure.
        /// Processes each student individually and returns an aggregated success/failure summary.
        /// </summary>
        /// <param name="req">Promotion request containing student list and target session, class, section.</param>
        /// <param name="companyId">The company ID for tenant scoping.</param>
        /// <param name="userId">The ID of the user performing the promotion.</param>
        /// <returns>A tuple indicating overall success and a descriptive summary message.</returns>
        public (bool success, string message) PromoteStudents(PromotionRequest req, int companyId, int userId)
        {
            int successCount = 0;
            string lastError = "";

            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            foreach (var student in req.Students)
            {
                try
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@StudentID", student.StudentID);
                    parameters.Add("@CompanyID", companyId);
                    parameters.Add("@NextSessionID", req.NextSessionID);
                    parameters.Add("@NextClassID", req.NextClassID);
                    parameters.Add("@NextSectionID", req.NextSectionID);
                    parameters.Add("@Result", student.Result);
                    parameters.Add("@NextStatus", student.NextStatus);
                    parameters.Add("@UserId", userId);

                    // Dapper auto-maps the SP result row → anonymous type; no foreach/DataTable needed
                    var result = conn.QueryFirstOrDefault(
                        "sp_Academics_Student_Promote_Single",
                        parameters,
                        commandType: CommandType.StoredProcedure);

                    if (result != null && (int)result.Result > 0)
                        successCount++;
                    else
                        lastError = result?.Message ?? "Unknown error";
                }
                catch (Exception ex)
                {
                    lastError = ex.Message;
                }
            }

            // ── Summary ──────────────────────────────────────────────────────────────
            int total = req.Students.Count;

            if (successCount == total)
                return (true, $"All {successCount} student(s) promoted successfully.");

            if (successCount > 0)
                return (true, $"{successCount} of {total} student(s) promoted. Last error: {lastError}");

            return (false, $"Promotion failed for all {total} student(s). Error: {lastError}");
        }

        public async Task<PagedResult<ClassTeacherViewModel>> GetAllClassTeachersWithPage(AcademicsSearchRequest req)
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var param = new DynamicParameters();
                if (req.PageNumber == 0 && req.PageSize == 0)
                {
                    req.PageNumber = 1;
                    req.PageSize = 10;
                }


                param.Add("@CompanyID", req.CompanyID);
                param.Add("@SessionID", req.SessionID);
                param.Add("@CLASSIDS", req.ClassIDs);
                param.Add("@SECTIONIDS", req.SectionID);
                param.Add("@SearchKeyword", req.SearchKeyword);                
                param.Add("@PageNumber", req.PageNumber);
                param.Add("@PageSize", req.PageSize);

                var result = (await conn.QueryAsync<ClassTeacherViewModel>(
                "sp_Academics_ClassTeacher_GetAllPAGEINDEX",
                param,
                commandType: CommandType.StoredProcedure)).ToList();


                int res = result.FirstOrDefault()?.Result ?? 0;
                int totalRecords = result.FirstOrDefault()?.TOTALRECORDS ?? 0;
                int pageIndex = result.FirstOrDefault()?.CURRENTPAGE ?? 0;
                int pageSize = result.FirstOrDefault()?.PageSize ?? 0;

                var userModel = new PagedResult<ClassTeacherViewModel>
                {
                    Data = result,
                    TotalRecords = totalRecords,
                    PageNumber = pageIndex,
                    PageSize = pageSize
                };

                if (res == 0)
                {
                    userModel = new PagedResult<ClassTeacherViewModel>
                    {
                        Data = null,
                        TotalRecords = totalRecords,
                        PageNumber = pageIndex,
                        PageSize = pageSize
                    };
                }
                return userModel;

            }
            catch (Exception ex)
            {
                throw;
            }

        }
    }
}
