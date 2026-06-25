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
    /// This service handles the actual work of managing account heads, such as saving, updating, or deleting head records in the database.
    /// </summary>
    public class AccountHeadService: IAccountHeadService
    {
        private readonly IConfiguration _configuration;
        public AccountHeadService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        /// <summary>
        /// Retrieves all account heads based on company, session, head type, and optional deleted filter.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="headType">The type of account head.</param>
        /// <param name="includeDeleted">Whether to include deleted records. Default is false.</param>
        /// <returns>List of <see cref="AccountHeadViewModel"/> matching the criteria.</returns>
        public List<AccountHeadViewModel> GetAllAccountHeads(int companyId, int sessionId, string headType, bool includeDeleted = false)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();
            parameters.Add("@CompanyID", companyId);
            parameters.Add("@SessionID", sessionId);
            parameters.Add("@HeadType", headType);
            parameters.Add("@IncludeDeleted", includeDeleted);

            return conn.Query<AccountHeadViewModel>(
                "sp_Mst_AccountHead_GetAll",
                parameters,
                commandType: CommandType.StoredProcedure
            ).ToList();
        }
        /// <summary>
        /// Retrieves a single account head record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique AccountHeadID to look up.</param>
        /// <returns>
        /// An <see cref="AccountHeadViewModel"/> if found; otherwise <c>null</c>.
        /// </returns>
        public AccountHeadViewModel? GetAccountHeadByID(int id)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.QueryFirstOrDefault<AccountHeadViewModel>(
                "sp_Mst_AccountHead_GetByID",
                new { AccountHeadID = id },
                commandType: CommandType.StoredProcedure
            );
        }

        /// <summary>
        /// Inserts or updates an account head record by executing the stored procedure
        /// <c>sp_Mst_AccountHead_Upsert</c>.
        /// </summary>
        /// <param name="req">The upsert request containing account head details.</param>
        /// <param name="companyId">The ID of the company.</param>
        /// <param name="sessionId">The current session ID.</param>
        /// <param name="userId">The ID of the user performing the operation.</param>
        /// <returns>
        /// A tuple where <c>Success</c> is <see langword="true"/> if the upsert succeeded,
        /// and <c>Message</c> contains the result message from the stored procedure.
        /// </returns>
        public (bool Success, string Message) UpsertAccountHead(
            AccountHeadUpsertRequest req, int companyId, int sessionId, int userId)
        {
            try
            {
                var p = new DynamicParameters();
                p.Add("@AccountHeadID", req.AccountHeadID);
                p.Add("@CompanyID", companyId);
                p.Add("@SessionID", sessionId);
                p.Add("@HeadName", req.HeadName);
                p.Add("@HeadDescription", req.HeadDescription);   // Dapper handles null → DBNull automatically
                p.Add("@HeadType", req.HeadType);
                p.Add("@IsActive", req.IsActive);
                p.Add("@UserId", userId);

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<(int Result, string Message)>(
                    "sp_Mst_AccountHead_Upsert", p,
                    commandType: CommandType.StoredProcedure);

                return (result.Result == 1, result.Message!);
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        /// <summary>
        /// Deletes an account head record by executing the stored procedure and returning success status with message.
        /// </summary>
        /// <param name="id">The AccountHead ID to delete.</param>
        /// <param name="userId">The ID of the user performing the deletion.</param>
        /// <returns>A tuple containing Success flag and Message from the stored procedure result.</returns>
        public (bool Success, string Message) DeleteAccountHead(List<int> id, int userId)
        {
            try
            {
                if (id == null || !id.Any())
                {
                    return (false, "No students selected for deletion.");
                }

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string accountHeadIDs = string.Join(",", id);

                var parameters = new DynamicParameters();
                parameters.Add("@AccountHeadID", accountHeadIDs);
                parameters.Add("@UserId", userId);

                var result = conn.QueryFirstOrDefault(
                    "sp_Mst_AccountHead_Delete",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return (Convert.ToInt32(result.Result) == 1, (string)result.Message);
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        /// <summary>
        /// Toggles the active/inactive status of an account head record.
        /// </summary>
        /// <param name="id">The unique identifier of the account head.</param>
        /// <param name="isActive">True to activate, false to deactivate the account head.</param>
        /// <param name="userId">The ID of the user performing the status change.</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        ///   <item><description><c>Success</c>: True if the operation succeeded, false otherwise.</description></item>
        ///   <item><description><c>Message</c>: A status message returned from the stored procedure.</description></item>
        /// </list>
        /// </returns>
        public (bool Success, string Message) ToggleAccountHeadStatus(int id, bool isActive, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var parameters = new DynamicParameters();
                parameters.Add("@AccountHeadID", id);
                parameters.Add("@IsActive", isActive);
                parameters.Add("@UserId", userId);

                // Dapper maps the first row automatically — no foreach or DataTable needed
                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Mst_AccountHead_ToggleStatus",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return result is not null
                    ? (result.Result == 1, result.Message ?? string.Empty)
                    : (false, "No response from stored procedure.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
      
    }
}
