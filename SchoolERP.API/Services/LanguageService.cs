using Dapper;
using Microsoft.Data.SqlClient;
using SchoolERP.API.Data;
using SchoolERP.API.Interfaces;
using SchoolERP.API.Models;
using System.Data;

namespace SchoolERP.API.Services
{
    /// <summary>
    /// This service handles the actual work of managing language settings, such as saving new languages and updating their details in the database.
    /// </summary>
    public class LanguageService : ILanguageService
    {
        private readonly IConfiguration _configuration;
        public LanguageService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Retrieves a list of all languages from the database.
        /// </summary>
        public List<MstLanguageViewModel> GetAllLanguages(bool includeDeleted = false)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.Query<MstLanguageViewModel>(
                "sp_Languages_GetAll",
                new { IncludeDeleted = includeDeleted },
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        /// <summary>
        /// Looks up the details of a specific language using its unique ID.
        /// </summary>
        public MstLanguageViewModel? GetLanguageByID(int languageId)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.QueryFirstOrDefault<MstLanguageViewModel>(
                "sp_Languages_GetByID",
                new { LanguageId = languageId },
                commandType: CommandType.StoredProcedure
            );
        }

        /// <summary>
        /// Saves or updates language information in the database, including the name,
        /// code, icon, RTL status, base language, and active status.
        /// </summary>
        public (bool success, string message) UpsertLanguage(MstLanguageUpsertRequest request, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<dynamic>(
                    "sp_Languages_Upsert",
                    new
                    {
                        request.LanguageId,
                        request.LanguageTitle,
                        request.LanguageCode,
                        request.LanguageIcon,
                        request.LanguageIsRtl,
                        request.LanguageBase,
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
        /// Deletes a language record from the database.
        /// </summary>
        public (bool success, string message) DeleteLanguage(List<int> languageId, int userId)
        {
            try
            {
                if (languageId == null || !languageId.Any())
                {
                    return (false, "No students selected for deletion.");
                }

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string languageIds = string.Join(",", languageId);
                var result = conn.QueryFirstOrDefault<dynamic>(
                    "sp_Languages_Delete",
                    new
                    {
                        LanguageId = languageIds,
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
        /// Updates whether a language is currently enabled for use in the application.
        /// </summary>
        
        public (bool success, string message) ToggleLanguageStatus(int languageId, bool isActive, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<dynamic>(
                    "sp_Languages_ToggleStatus",
                    new
                    {
                        LanguageId = languageId,
                        IsActive = isActive,
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
        /// A helper tool that converts raw database data about languages into a format that the application can easily understand.
        /// </summary>
        
    }
}
