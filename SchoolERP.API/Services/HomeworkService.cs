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
            try
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
            catch (Exception ex)
            {
                throw;
            }
            
        }

        /// <summary>
        /// Retrieves homework details by homework ID.
        /// </summary>
        public HomeworkViewModel? GetByID(int id)
        {
            try
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
            catch (Exception ex)
            {

                throw;
            }
            
        }

        /// <summary>
        /// Saves a new homework assignment or updates an existing one.
        /// It records the subject, description, attachment, marks, and submission date.
        /// </summary>
        public async Task<ApiResponse> Upsert(
            HomeworkUpsertRequest request,
            int companyId,
            int sessionId,
            int userId)
        {
            var response = new ApiResponse();
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var parameters = new DynamicParameters();

                parameters.Add("@HOMEWORKID", request.HomeworkID);
                parameters.Add("@COMPANYID", companyId);
                parameters.Add("@SESSIONID", sessionId);
                parameters.Add("@CLASSID", request.ClassID);
                parameters.Add("@SECTIONID", request.SectionID);
                parameters.Add("@SUBJECTGROUPID", request.SubjectGroupID);
                parameters.Add("@SUBJECTID", request.SubjectID);
                parameters.Add("@HOMEWORKDATE", request.HomeworkDate);
                parameters.Add("@SUBMISSIONDATE", request.SubmissionDate);
                parameters.Add("@MAXMARKS", request.MaxMarks);
                parameters.Add("@ATTACHMENTPATH", request.AttachmentPath);
                parameters.Add("@DESCRIPTION", request.Description);
                parameters.Add("@ISACTIVE", request.IsActive);
                parameters.Add("@USERID", userId);

                var result = await conn.QueryFirstOrDefaultAsync<dynamic>(
                   "sp_Homework_Upsert",
                   parameters,
                   commandType: CommandType.StoredProcedure);

                if (result != null)
                {
                    response.Result = result.RESULT;
                    response.Message = result.MESSAGE;
                    response.TechnicalMessage = result.TECHNICALMESSAGE;
                }

                
            }
            catch (Exception ex)
            {
                response.Result = 0;
                response.Message = "Unable to save hostel type. Please try again.";
                response.TechnicalMessage = ex.Message;
            }
            return response;
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
        public (bool success, string message) Delete(List<int> ids, int userId)
        {
            try
            {
                if (ids == null || !ids.Any())
                {
                    return (false, "No homework selected for deletion.");
                }


                using var conn = new SqlConnection(
                   _configuration.GetConnectionString("DefaultConnection"));

                string homeworkId = string.Join(",", ids);

                var parameters = new DynamicParameters();
                parameters.Add("@HOMEWORKIDS", homeworkId);
                parameters.Add("@USERID", userId);
                var result = conn.QueryFirstOrDefault<SpResult>(
                   "sp_Homework_Delete",
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

        public (bool success, string message) ToggleStatus(StatusUpdateRequest request)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var parameters = new DynamicParameters();
                parameters.Add("@HOMEWORKIDS", request.Ids);
                parameters.Add("@IsActive", request.IsActive);
                parameters.Add("@UserId", request.DoneBy);

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Homework_ToggleStatus",
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

        public async Task<PagedResult<HomeworkViewModel>> GetAllHomeWorkWithPage(SearchRequest req)
        {
            var response = new PagedResult<HomeworkViewModel>();
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var param = new DynamicParameters();
                if (req.PageNumber == 0 && req.PageSize == 0)
                {
                    req.PageNumber = 1;
                    req.PageSize = 10;
                }


                param.Add("@COMPANYID", req.CompanyID);
                param.Add("@SESSIONID", req.SessionID);
                param.Add("@MODE", req.Mode);
                param.Add("@SEARCHKEYWORD", req.SearchKeyword);
                param.Add("@PAGENUMBER", req.PageNumber);
                param.Add("@PAGESIZE", req.PageSize);
                param.Add("@INCLUDEDELETED", 0);

                var result = (await conn.QueryAsync<HomeworkViewModel>(
                "SP_HOMEWORK_GETALLWITHPAGEINDEX",
                param,
                commandType: CommandType.StoredProcedure)).ToList();


                int res = result.FirstOrDefault()?.RESULT ?? 0;
                int totalRecords = result.FirstOrDefault()?.TOTALRECORDS ?? 0;
                int pageIndex = result.FirstOrDefault()?.CURRENTPAGE ?? 0;
                int pageSize = result.FirstOrDefault()?.PAGESIZE ?? 0;

                response = new PagedResult<HomeworkViewModel>
                {
                    Data = result,
                    TotalRecords = totalRecords,
                    PageNumber = pageIndex,
                    PageSize = pageSize
                };

                if (res == 0)
                {
                    response = new PagedResult<HomeworkViewModel>
                    {
                        Data = null,
                        TotalRecords = totalRecords,
                        PageNumber = pageIndex,
                        PageSize = pageSize
                    };
                }
                return response;

            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Unable to fetch homework list. Please try again.";
                response.Data = new List<HomeworkViewModel>();
                return response;
            }
        }

       
    }
}
