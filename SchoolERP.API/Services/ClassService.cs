using Dapper;
using Microsoft.Data.SqlClient;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using System.Collections.Generic;
using System.Data;
using static System.Collections.Specialized.BitVector32;

namespace SchoolERP.API.Services
{
    /// <summary>
    /// This service handles the actual work of managing school classes, such as saving, updating, or deleting class records in the database.
    /// </summary>
    public class ClassService: IClassService
    {
        private readonly IConfiguration _configuration;
        public ClassService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ─── CLASS ─────────────────────────────────────────────

        /// <summary>
        /// Retrieves all classes for the specified company and session.
        /// </summary>
        /// <param name="companyId">Company/School ID.</param>
        /// <param name="sessionId">Academic session ID.</param>
        /// <param name="includeDeleted">Whether to include deleted records.</param>
        /// <returns>List of classes.</returns>
        public List<MstClassViewModel> GetAllClasses(
            int companyId,
            int sessionId,
            bool includeDeleted = false)
        {
            using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();
            parameters.Add("@CompanyID", companyId);
            parameters.Add("@SessionID", sessionId);
            parameters.Add("@IncludeDeleted", includeDeleted);

            var result= conn.Query<MstClassViewModel>(
                "sp_Class_GetAll",
                parameters,
                commandType: CommandType.StoredProcedure
            ).ToList();

            // If SP returned no rows at all
            if (!result.Any()) return null;

            // If SP returned rows but RESULT != 1 (failure case)
            if (result.First().Result != 1) return null;

            return result;
        }

        /// <summary>
        /// Retrieves class details by its unique ID.
        /// </summary>
        /// <param name="classId">Class ID.</param>
        /// <returns>Class details if found; otherwise null.</returns>
        public MstClassViewModel? GetClassByID(int classId)
        {
            using var conn = new SqlConnection(
                     _configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();
            parameters.Add("@ClassID", classId);

            return conn.QueryFirstOrDefault<MstClassViewModel>(
                "sp_Class_GetByID",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        /// <summary>
        /// Creates a new class or updates an existing class.
        /// </summary>
        /// <param name="request">Class information including assigned sections.</param>
        /// <param name="companyId">Company/School ID.</param>
        /// <param name="sessionId">Academic session ID.</param>
        /// <param name="userId">Logged-in user ID.</param>
        /// <returns>Operation status and message.</returns>
        public (bool success, string message) UpsertClass(
         MstClassUpsertRequest request,
         int companyId,
         int sessionId,
         int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                      _configuration.GetConnectionString("DefaultConnection"));

                string sectionIds = request.SectionIds != null
                    ? string.Join(",", request.SectionIds)
                    : string.Empty;

                var parameters = new DynamicParameters();
                parameters.Add("@ClassID", request.ClassID);
                parameters.Add("@CompanyID", companyId);
                parameters.Add("@SessionID", sessionId);
                parameters.Add("@ClassName", request.ClassName);
                parameters.Add("@IsActive", request.IsActive);
                parameters.Add("@UserId", userId);
                parameters.Add("@SectionIds", sectionIds);

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Class_Upsert",
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
        /// Deletes a class record by its unique ID.
        /// </summary>
        /// <param name="classId">Class ID.</param>
        /// <param name="userId">Logged-in user ID.</param>
        /// <returns>Operation status and message.</returns>
        public (bool success, string message) DeleteClass(List<int> ids, int userId)
        {
            try
            {
                if (ids == null || !ids.Any())
                {
                    return (false, "No students selected for deletion.");
                }

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string classId = string.Join(",", ids);

                var parameters = new DynamicParameters();
                parameters.Add("@ClassID", classId);
                parameters.Add("@UserId", userId);
                var result = conn.QueryFirstOrDefault<SpResult>(
                   "sp_Class_Delete",
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
        /// Activates or deactivates a class.
        /// </summary>
        /// <param name="classId">Class ID.</param>
        /// <param name="isActive">Status to set.</param>
        /// <param name="userId">Logged-in user ID.</param>
        /// <returns>Operation status and message.</returns>
        public (bool success, string message) ToggleClassStatus(StatusUpdateRequest request)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var parameters = new DynamicParameters();
                parameters.Add("@ClassID", request.Ids);
                parameters.Add("@IsActive", request.IsActive);
                parameters.Add("@UserId", request.DoneBy);

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Class_ToggleStatus",
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
    }
}
