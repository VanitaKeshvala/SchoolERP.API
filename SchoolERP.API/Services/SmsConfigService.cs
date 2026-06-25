using Dapper;
using Microsoft.Data.SqlClient;
using SchoolERP.API.Data;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using System.Data;

namespace SchoolERP.API.Services
{
    /// <summary>
    /// This service handles the actual work of managing SMS configuration, including saving and retrieving gateway settings from the database.
    /// </summary>
    public class SmsConfigService: ISmsConfigService
    {
        private readonly IConfiguration _configuration;
        public SmsConfigService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Retrieves SMS configuration details for the specified company.
        /// </summary>
        public MstSmsConfigViewModel? GetSmsConfig(int companyId)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.QueryFirstOrDefault<MstSmsConfigViewModel>(
                "sp_SmsConfig_Get",
                new
                {
                    CompanyId = companyId
                },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Saves or updates SMS gateway settings (gateway name, API URL, API key, status, etc.)
        /// for the specified company and returns the operation result.
        /// </summary>
        public (bool success, string message) UpsertSmsConfig(
            MstSmsConfigUpsertRequest request,
            int userId,
            int companyId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<dynamic>(
                    "sp_SmsConfig_Upsert",
                    new
                    {
                        CompanyId = companyId,
                        SmsId = request.SmsId,
                        SmsGateway = request.SmsGateway,
                        SmsStatus = request.SmsStatus,
                        SmsApiUrl = request.SmsApiUrl,
                        SmsApiKey = request.SmsApiKey,
                        IsActive = request.IsActive,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return (
                    Convert.ToInt32(result?.Result ?? 0) == 1,
                    result?.Message?.ToString() ?? string.Empty
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
