using Dapper;
using Microsoft.Data.SqlClient;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using System.Data;

namespace SchoolERP.API.Services
{
    /// <summary>
    /// This service handles the actual work of managing account entries, such as saving, updating, or searching income and expense transactions in the database.
    /// </summary>
    public class AccountEntryService: IAccountEntryService
    {
        private readonly IConfiguration _configuration;
        public AccountEntryService(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        /// <summary>
        /// Retrieves all account entries for the specified company and session.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="entryType">The type of account entry.</param>
        /// <param name="includeDeleted">Whether to include deleted entries. Default is false.</param>
        /// <returns>A list of <see cref="AccountEntryViewModel"/> objects.</returns>
        public List<AccountEntryViewModel> GetAllAccountEntries(int companyId, int sessionId, string entryType, bool includeDeleted = false)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var parameters = new DynamicParameters();
                parameters.Add("@CompanyID", companyId);
                parameters.Add("@SessionID", sessionId);
                parameters.Add("@EntryType", entryType);
                parameters.Add("@IncludeDeleted", includeDeleted);

                return conn.Query<AccountEntryViewModel>(
                    "sp_Trn_AccountEntry_GetAll",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
            catch (Exception)
            {
                return new List<AccountEntryViewModel>();
            }
        }

        /// <summary>
        /// Searches account entries based on company, session, entry type, search type, and optional date range.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="entryType">The type of account entry.</param>
        /// <param name="searchType">The search type filter.</param>
        /// <param name="dateFrom">Optional start date in dd-MM-yyyy format.</param>
        /// <param name="dateTo">Optional end date in dd-MM-yyyy format.</param>
        /// <returns>A list of <see cref="AccountEntryViewModel"/> matching the search criteria.</returns>
        public List<AccountEntryViewModel> SearchAccountEntries(int companyId, int sessionId, string entryType, string searchType, string? dateFrom, string? dateTo)
        {
            try
            {
                DateTime? dFrom = null;
                DateTime? dTo = null;
                if (!string.IsNullOrEmpty(dateFrom)) dFrom = DateTime.ParseExact(dateFrom, "dd-MM-yyyy", null);
                if (!string.IsNullOrEmpty(dateTo)) dTo = DateTime.ParseExact(dateTo, "dd-MM-yyyy", null);

                var p = new
                {
                    CompanyID = companyId,
                    SessionID = sessionId,
                    EntryType = entryType,
                    SearchType = searchType,
                    DateFrom = dFrom,
                    DateTo = dTo
                };

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                return conn.Query<AccountEntryViewModel>("sp_Trn_AccountEntry_Search", p, commandType: CommandType.StoredProcedure)
                           .ToList();
            }
            catch (Exception) { /* Log here if needed */ }

            return new List<AccountEntryViewModel>();
        }

        /// <summary>
        /// Retrieves a single account entry by its ID.
        /// </summary>
        /// <param name="id">The AccountEntry ID to look up.</param>
        /// <returns>An <see cref="AccountEntryViewModel"/> if found; otherwise <c>null</c>.</returns>
        public AccountEntryViewModel? GetAccountEntryByID(int id)
        {
            using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));
            return conn.QueryFirstOrDefault<AccountEntryViewModel>(
                "sp_Trn_AccountEntry_GetByID",
                new { AccountEntryID = id },
                commandType: CommandType.StoredProcedure
            );
        }

        /// <summary>
        /// Inserts or updates an account entry record using the stored procedure <c>sp_Trn_AccountEntry_Upsert</c>.
        /// </summary>
        /// <param name="req">The account entry request model containing all entry details.</param>
        /// <param name="companyId">The ID of the company performing the operation.</param>
        /// <param name="sessionId">The current session ID.</param>
        /// <param name="userId">The ID of the user performing the operation.</param>
        /// <returns>
        /// A tuple where <c>Success</c> indicates whether the upsert was successful,
        /// and <c>Message</c> contains the result message from the stored procedure.
        /// </returns>
        public (bool Success, string Message) UpsertAccountEntry(AccountEntryUpsertRequest req, int companyId, int sessionId, int userId)
        {
            try
            {
                var p = new DynamicParameters();
                p.Add("@AccountEntryID", req.AccountEntryID);
                p.Add("@CompanyID", companyId);
                p.Add("@SessionID", sessionId);
                p.Add("@AccountHeadID", req.AccountHeadID);
                p.Add("@EntryType", req.EntryType);
                p.Add("@Name", req.Name);
                p.Add("@InvoiceNo", req.InvoiceNo);
                p.Add("@Date", req.Date);
                p.Add("@Amount", req.Amount);
                p.Add("@AttachDoc", req.AttachDoc, DbType.Binary);
                p.Add("@AttachDocType", req.AttachDocType);
                p.Add("@AttachDocName", req.AttachDocName);
                p.Add("@Description", req.Description);
                p.Add("@IsActive", req.IsActive);
                p.Add("@UserId", userId);

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Trn_AccountEntry_Upsert",
                    p,
                    commandType: CommandType.StoredProcedure
                );

                return (result?.Result == 1, result?.Message ?? string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }


        /// <summary>
        /// Deletes an account entry by executing the stored procedure sp_Trn_AccountEntry_Delete.
        /// </summary>
        /// <param name="id">The ID of the account entry to delete.</param>
        /// <param name="userId">The ID of the user performing the deletion.</param>
        /// <returns>A tuple indicating success status and a message from the stored procedure.</returns>
        public (bool Success, string Message) DeleteAccountEntry(List<int> id, int userId)
        {
            try
            {
                if (id == null || !id.Any())
                {
                    return (false, "No Income selected for deletion.");
                }

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string accountEntryID = string.Join(",", id);

                var parameters = new DynamicParameters();
                parameters.Add("@AccountEntryID", accountEntryID);
                parameters.Add("@UserId", userId);


                var result = conn.QueryFirstOrDefault<SpResult>(
                   "sp_Trn_AccountEntry_Delete",
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
        /// Toggles the active/inactive status of an account entry.
        /// </summary>
        /// <param name="id">The AccountEntry ID to toggle.</param>
        /// <param name="isActive">True to activate, false to deactivate.</param>
        /// <param name="userId">The ID of the user performing the action.</param>
        /// <returns>
        /// A tuple containing:
        /// <c>Success</c> — true if the stored procedure returned Result = 1; otherwise false.
        /// <c>Message</c> — the message returned by the stored procedure, or the exception message on failure.
        /// </returns>
        public (bool Success, string Message) ToggleAccountEntryStatus(int id, bool isActive, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault(
                    "sp_Trn_AccountEntry_ToggleStatus",
                    new { AccountEntryID = id, IsActive = isActive, UserId = userId },
                    commandType: CommandType.StoredProcedure);

                return (result?.Result == 1, result?.Message?.ToString() ?? string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

    }
}
