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
    /// This service performs the actual work of managing company information, such as saving, updating, or deleting school records from the database.
    /// </summary>
    public class CompanyService: ICompanyService
    {
        private readonly IConfiguration _configuration;
        public CompanyService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Retrieves a complete list of all companies from the database.
        /// </summary>
        /// <param name="includeDeleted">Indicates whether deleted companies should be included.</param>
        /// <returns>A list of company records.</returns>
        public List<MstCompanyViewModel> GetAllCompanies(bool includeDeleted = false)
        {
            using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();
           // parameters.Add("@IncludeDeleted", includeDeleted);

            return conn.Query<MstCompanyViewModel>(
                "sp_Companies_GetAll",
                //parameters,
                commandType: CommandType.StoredProcedure)
                .ToList();
        }

        /// <summary>
        /// Retrieves all companies assigned to the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A list of companies assigned to the user.</returns>
        public List<MstCompanyViewModel> GetCompaniesByUserId(int userId)
        {
            using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

            var assignedCompanyIds = conn.Query<int>(
                "sp_UserCompanies_GetByUser",
                new { UserID = userId },
                commandType: CommandType.StoredProcedure)
                .ToHashSet();

            return GetAllCompanies(false)
                .Where(c => assignedCompanyIds.Contains(c.CompanyId))
                .ToList();
        }

        /// <summary>
        /// Retrieves company details by company ID.
        /// </summary>
        /// <param name="companyId">The unique identifier of the company.</param>
        /// <returns>The company details if found; otherwise, null.</returns>
        public MstCompanyViewModel? GetCompanyByID(int companyId)
        {
            using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

            return conn.QueryFirstOrDefault<MstCompanyViewModel>(
                "sp_Companies_GetByID",
                new { CompanyId = companyId },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Creates a new company or updates an existing company.
        /// </summary>
        /// <param name="request">Company information to save.</param>
        /// <param name="userId">Current user identifier.</param>
        /// <returns>Operation status and message.</returns>
        public (bool success, string message) UpsertCompany(MstCompanyUpsertRequest request, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                     _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Companies_Upsert",
                    new
                    {
                        request.CompanyId,
                        request.SchoolName,
                        request.SchoolCode,
                        request.ParentCompanyId,
                        request.Address,
                        request.Phone,
                        request.Email,
                        request.SessionId,
                        request.CurrencyId,
                        request.SessionStartMonth,
                        request.DateFormat,
                        request.Timezone,
                        request.StartDayOfWeek,
                        request.CurrencyFormat,
                        request.IsActive,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return (result?.Result == 1, result?.Message ?? string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Deletes a company from the system.
        /// </summary>
        /// <param name="companyId">Company identifier.</param>
        /// <param name="userId">Current user identifier.</param>
        /// <returns>Operation status and message.</returns>
        public (bool success, string message) DeleteCompany(List<int> companyId, int userId)
        {
            try
            {

                if (companyId == null || !companyId.Any())
                {
                    return (false, "No students selected for deletion.");
                }

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string companyIds = string.Join(",", companyId);

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Companies_Delete",
                    new
                    {
                        CompanyId = companyIds,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return (result?.Result == 1, result?.Message ?? string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Activates or deactivates a company.
        /// </summary>
        /// <param name="companyId">Company identifier.</param>
        /// <param name="isActive">Status to apply.</param>
        /// <param name="userId">Current user identifier.</param>
        /// <returns>Operation status and message.</returns>
        public (bool success, string message) ToggleStatus(StatusUpdateRequest request)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

               
                var parameters = new DynamicParameters();
                parameters.Add("@COMPANYID", request.Ids);
                parameters.Add("@ISACTIVE", request.IsActive);
                parameters.Add("@USERID", request.DoneBy);

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Companies_ToggleStatus",
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
        /// Updates the currently selected company for a user.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        /// <param name="companyId">Company identifier.</param>
        /// <returns>Operation status and message.</returns>
        public (bool success, string message) UpdateUserCurrentCompany(int userId, int companyId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Users_UpdateCurrentCompany",
                    new
                    {
                        UserID = userId,
                        DoneBy = userId,
                        CurrentCompanyId = companyId
                    },
                    commandType: CommandType.StoredProcedure);

                return (result?.Result == 1, result?.Message ?? string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Retrieves the currently selected company for a user.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        /// <returns>Company identifier if found; otherwise, null.</returns>
        public int? GetUserCurrentCompany(int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                   _configuration.GetConnectionString("DefaultConnection"));

                return conn.QueryFirstOrDefault<int?>(
                    "sp_Users_GetCurrentCompany",
                    new { UserID = userId },
                    commandType: CommandType.StoredProcedure);
            }
            catch
            {
                return null;
            }
        }

        
    }
}
