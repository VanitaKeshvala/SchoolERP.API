using Dapper;
using Microsoft.Data.SqlClient;
using SchoolERP.API.Interfaces;
using SchoolERP.API.Models;
using SchoolERP.API.Models.Common;
using System.Data;

namespace SchoolERP.API.Services
{
    /// <summary>
    /// This service handles the actual work of managing class sections, such as saving, updating, or deleting section records in the database.
    /// </summary>
    public class SectionService : ISectionService
    {
        private readonly IConfiguration _configuration;
        public SectionService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ─── SECTION ────────────────────────────────────────────

        /// <summary>
        /// Retrieves all sections for the specified company and session.
        /// </summary>
        /// <param name="companyId">Company/School ID.</param>
        /// <param name="sessionId">Academic session ID.</param>
        /// <param name="includeDeleted">Whether to include deleted records.</param>
        /// <returns>List of sections.</returns>
        public List<MstSectionViewModel> GetAllSections(
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

            return conn.Query<MstSectionViewModel>(
                "sp_Sections_GetAll",
                parameters,
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        /// <summary>
        /// Retrieves all sections assigned to a specific class.
        /// </summary>
        /// <param name="classId">Class ID.</param>
        /// <returns>List of sections for the specified class.</returns>
        public List<MstSectionViewModel> GetSectionsByClass(int classId)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();
            parameters.Add("@ClassID", classId);

            return conn.Query<MstSectionViewModel>(
                "sp_Section_GetByClass",
                parameters,
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        /// <summary>
        /// Retrieves section details by its unique ID.
        /// </summary>
        /// <param name="sectionId">Section ID.</param>
        /// <returns>Section details if found; otherwise null.</returns>
        public MstSectionViewModel? GetSectionByID(int sectionId)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();
            parameters.Add("@SectionID", sectionId);

            return conn.QueryFirstOrDefault<MstSectionViewModel>(
                "sp_Sections_GetByID",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        /// <summary>
        /// Creates a new section or updates an existing section.
        /// </summary>
        /// <param name="request">Section information.</param>
        /// <param name="companyId">Company/School ID.</param>
        /// <param name="sessionId">Academic session ID.</param>
        /// <param name="userId">Logged-in user ID.</param>
        /// <returns>Operation status and message.</returns>
        public (bool success, string message) UpsertSection(
            MstSectionUpsertRequest request,
            int companyId,
            int sessionId,
            int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var parameters = new DynamicParameters();
                parameters.Add("@SectionID", request.SectionID);
                parameters.Add("@CompanyID", companyId);
                parameters.Add("@SessionID", sessionId);
                parameters.Add("@SectionName", request.SectionName);
                parameters.Add("@IsActive", request.IsActive);
                parameters.Add("@UserId", userId);

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Sections_Upsert",
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
        /// Deletes a section record by its unique ID.
        /// </summary>
        /// <param name="sectionId">Section ID.</param>
        /// <param name="userId">Logged-in user ID.</param>
        /// <returns>Operation status and message.</returns>
        public (bool success, string message) DeleteSection(
            List<int> sectionId,
            int userId)
        {
            try
            {
                if (sectionId == null || !sectionId.Any())
                {
                    return (false, "No students selected for deletion.");
                }

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string id = string.Join(",", sectionId);

                var parameters = new DynamicParameters();
                parameters.Add("@SectionID", id);
                parameters.Add("@UserId", userId);

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Sections_Delete",
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
        /// Activates or deactivates a section.
        /// </summary>
        /// <param name="sectionId">Section ID.</param>
        /// <param name="isActive">Status to set.</param>
        /// <param name="userId">Logged-in user ID.</param>
        /// <returns>Operation status and message.</returns>
        public (bool success, string message) ToggleSectionStatus(
            int sectionId,
            bool isActive,
            int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var parameters = new DynamicParameters();
                parameters.Add("@SectionID", sectionId);
                parameters.Add("@IsActive", isActive);
                parameters.Add("@UserId", userId);

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Sections_ToggleStatus",
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
