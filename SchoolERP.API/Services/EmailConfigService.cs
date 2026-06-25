using Dapper;
using Microsoft.Data.SqlClient;
using SchoolERP.API.Data;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using System.Data;

namespace SchoolERP.API.Services
{
    /// <summary>
    /// This service handles the actual work of managing email configuration, including saving and retrieving server settings from the database.
    /// </summary>
    public class EmailConfigService: IEmailConfigService
    {
        private readonly IConfiguration _configuration;
        public EmailConfigService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Retrieves the current email server settings from the database.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns>
        /// Returns the email configuration if found; otherwise, null.
        /// </returns>
        public MstEmailConfigViewModel? GetEmailConfig(int companyId)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.QueryFirstOrDefault<MstEmailConfigViewModel>(
                "sp_EmailConfig_Get",
                new
                {
                    CompanyId = companyId
                },
                commandType: CommandType.StoredProcedure);
        }
        /// <summary>
        /// Inserts or updates email configuration details.
        /// Calls sp_EmailConfig_Upsert and returns operation status with message.
        /// </summary>
        public (bool success, string message) UpsertEmailConfig(
            MstEmailConfigUpsertRequest request,
            int userId,
            int companyId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault(
                    "sp_EmailConfig_Upsert",
                    new
                    {
                        CompanyId = companyId,
                        EmailId = request.EmailId,
                        EmailType = request.EmailType,
                        SMTPServer = request.SMTPServer,
                        SMTPPort = request.SMTPPort,
                        SMTPUsername = request.SMTPUsername,
                        SMTPPassword = request.SMTPPassword,
                        SslTls = request.SslTls,
                        SMTPAuth = request.SMTPAuth,
                        IsActive = request.IsActive,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return (
                    Convert.ToInt32(result?.Result ?? 0) == 1,
                    Convert.ToString(result?.Message) ?? string.Empty
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

    }
}
