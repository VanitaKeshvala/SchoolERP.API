using Dapper;
using Microsoft.Data.SqlClient;
using SchoolERP.API.Data;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using System.Data;

namespace SchoolERP.API.Services
{
    /// <summary>
    /// This service manages student homework assignments.
    /// It allows teachers to create homework tasks, set deadlines, 
    /// and keep track of when homework should be submitted.
    /// </summary>
    public class HomeworkService: IHomeworkService
    {
        private readonly IConfiguration _configuration;
        public HomeworkService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Retrieves all homework records for the specified company and session.
        /// Optionally includes deleted records.
        /// </summary>
        public List<HomeworkViewModel> GetAll(int companyId, int sessionId, bool includeDeleted = false)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.Query<HomeworkViewModel>(
                "sp_Homework_GetAll",
                new
                {
                    CompanyID = companyId,
                    SessionID = sessionId,
                    IncludeDeleted = includeDeleted
                },
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        /// <summary>
        /// Retrieves homework details by homework ID.
        /// </summary>
        public HomeworkViewModel? GetByID(int id)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.QueryFirstOrDefault<HomeworkViewModel>(
                "sp_Homework_GetByID",
                new
                {
                    HomeworkID = id
                },
                commandType: CommandType.StoredProcedure
            );
        }

        /// <summary>
        /// Saves a new homework assignment or updates an existing one.
        /// It records the subject, description, attachment, marks, and submission date.
        /// </summary>
        public (bool success, string message) Upsert(
            HomeworkUpsertRequest request,
            int companyId,
            int sessionId,
            int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault(
                    "sp_Homework_Upsert",
                    new
                    {
                        request.HomeworkID,
                        CompanyID = companyId,
                        SessionID = sessionId,
                        request.ClassID,
                        request.SectionID,
                        request.SubjectGroupID,
                        request.SubjectID,
                        request.HomeworkDate,
                        request.SubmissionDate,
                        request.MaxMarks,
                        request.AttachmentPath,
                        request.Description,
                        request.IsActive,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return (
                    Convert.ToInt32(result?.Result) == 1,
                    Convert.ToString(result?.Message) ?? string.Empty
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Deletes a homework record by Homework ID.
        /// </summary>
        /// <param name="id">Homework ID.</param>
        /// <param name="userId">User performing the delete action.</param>
        /// <returns>
        /// Tuple containing:
        /// success - Indicates whether the operation was successful.
        /// message - Status message returned from the stored procedure.
        /// </returns>
        public (bool success, string message) Delete(int id, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault(
                    "sp_Homework_Delete",
                    new
                    {
                        HomeworkID = id,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return (
                    Convert.ToInt32(result?.Result ?? 0) == 1,
                    result?.Message?.ToString() ?? ""
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Activates or deactivates a homework record.
        /// </summary>
        /// <param name="id">Homework ID.</param>
        /// <param name="isActive">Status to be set.</param>
        /// <param name="userId">User performing the action.</param>
        /// <returns>
        /// Tuple containing:
        /// success - Indicates whether the operation was successful.
        /// message - Status message returned from the stored procedure.
        /// </returns>
        public (bool success, string message) ToggleStatus(int id, bool isActive, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault(
                    "sp_Homework_ToggleStatus",
                    new
                    {
                        HomeworkID = id,
                        IsActive = isActive,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return (
                    Convert.ToInt32(result?.Result ?? 0) == 1,
                    result?.Message?.ToString() ?? ""
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
       
    }
}
