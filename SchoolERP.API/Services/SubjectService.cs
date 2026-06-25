using Dapper;
using Microsoft.Data.SqlClient;
using SchoolERP.API.Data;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using System.Data;
using static System.Collections.Specialized.BitVector32;

namespace SchoolERP.API.Services
{
    /// <summary>
    /// This service handles the actual work of managing academic subjects, such as saving, updating, or deleting subject records in the database.
    /// </summary>
    public class SubjectService: ISubjectService
    {
        private readonly IConfiguration _configuration;
        public SubjectService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Retrieves all subjects for the specified company and session.
        /// Optionally includes deleted subjects.
        /// </summary>
        public List<MstSubjectViewModel> GetAllSubjects(int companyId, int sessionId, bool includeDeleted = false)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var result= conn.Query<MstSubjectViewModel>(
                "sp_Subject_GetAll",
                new
                {
                    CompanyID = companyId,
                    SessionID = sessionId,
                    IncludeDeleted = includeDeleted
                },
                commandType: CommandType.StoredProcedure
            ).ToList();

            // If SP returned no rows at all
            if (!result.Any()) return null;

            // If SP returned rows but RESULT != 1 (failure case)
            if (result.First().Result != 1) return null;

            return result;
        }

        /// <summary>
        /// Retrieves a subject by its unique identifier.
        /// </summary>
        public MstSubjectViewModel? GetSubjectByID(int subjectId)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return conn.QueryFirstOrDefault<MstSubjectViewModel>(
                "sp_Subject_GetByID",
                new
                {
                    SubjectID = subjectId
                },
                commandType: CommandType.StoredProcedure
            );
        }

        /// <summary>
        /// Inserts or updates a subject record.
        /// </summary>
        public (bool success, string message) UpsertSubject(
            MstSubjectUpsertRequest request,
            int companyId,
            int sessionId,
            int userId)
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Subject_Upsert",
                    new
                    {
                        request.SubjectID,
                        CompanyID = companyId,
                        SessionID = sessionId,
                        request.SubjectName,
                        request.SubjectType,
                        request.SubjectCode,
                        request.IsActive,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return (
                    Convert.ToInt32(result?.Result) == 1,
                    Convert.ToString(result?.Message) ?? ""
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Deletes a subject record.
        /// </summary>
        public (bool success, string message) DeleteSubject(List<int> subjectId, int userId)
        {
            try
            {
                if (subjectId == null || !subjectId.Any())
                {
                    return (false, "No students selected for deletion.");
                }

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string id = string.Join(",", subjectId);
                
                var parameters = new DynamicParameters();
                parameters.Add("@SUBJECTID", id);
                parameters.Add("@USERID", userId);
                var result = conn.QueryFirstOrDefault<SpResult>(
                  "sp_Subject_Delete",
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
        /// Changes the active status of a subject.
        /// </summary>
        public (bool success, string message) ToggleSubjectStatus(StatusUpdateRequest request)
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var parameters = new DynamicParameters();
                parameters.Add("@SUBJECTID", request.Ids);
                parameters.Add("@ISACTIVE", request.IsActive);
                parameters.Add("@USERID", request.DoneBy);

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Subject_ToggleStatus",
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
