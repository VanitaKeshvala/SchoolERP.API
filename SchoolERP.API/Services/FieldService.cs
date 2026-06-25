using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SchoolERP.API.Data;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using System.ComponentModel.Design;
using System.Data;

namespace SchoolERP.API.Services
{
    /// <summary>
    /// This service handles the actual work of managing custom form fields and settings for automatically generating IDs (like Student Admission Numbers).
    /// </summary>
    public class FieldService: IFieldService
    {
        private readonly IConfiguration _configuration;
        public FieldService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        /// <summary>
        /// Retrieves a complete list of all custom fields for the current school and session from the database.
        /// </summary>
        public List<FieldModel> GetAllFields(int companyId, int sessionId, bool? isSystemField = null, string belongsTo = null)
        {
            using var conn = new SqlConnection(
            _configuration.GetConnectionString("DefaultConnection"));

            var param = new DynamicParameters();
            // Step 1: Pack the search criteria (School, Session, System-only, or Category).
            param.Add("@CompanyId", companyId);
            param.Add("@SessionId", sessionId);
            param.Add("@IsSystemField", isSystemField);
            param.Add("@BelongsTo", belongsTo);

            // Step 2: Ask the database for all matching fields using the 'GetAll' recipe.
            return conn.Query<FieldModel>(
                "sp_Mst_Fields_GetAll",
                param,
                commandType: CommandType.StoredProcedure)
                .ToList();
        }

        /// <summary>
        /// Retrieves a field record based on the specified field ID.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the field.
        /// </param>
        /// <returns>
        /// Returns the matching field record if found; otherwise null.
        /// </returns>
        public FieldModel? GetFieldByID(int id)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var param = new DynamicParameters();
            param.Add("@FieldId", id);

            return conn.QueryFirstOrDefault<FieldModel>(
                "sp_Mst_Fields_GetByID",
                param,
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Creates a new field or updates an existing field record.
        /// </summary>
        /// <param name="model">
        /// Contains field information such as name, type, display settings,
        /// required status, and company/session details.
        /// </param>
        /// <param name="userId">
        /// The ID of the user performing the operation.
        /// </param>
        /// <returns>
        /// Returns a tuple containing the operation status and result message.
        /// </returns>
        public (bool Success, string Message) UpsertField(
            FieldViewModel model,
            int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var param = new DynamicParameters();

                param.Add("@FieldId", model.FieldId);
                param.Add("@BelongsTo", model.BelongsTo);
                param.Add("@FieldName", model.FieldName);
                param.Add("@FieldType", model.FieldType);
                param.Add("@FieldValues", model.FieldValues);
                param.Add("@IsSystemField", model.IsSystemField);
                param.Add("@IsRequired", model.IsRequired);
                param.Add("@IsActive", model.IsActive);
                param.Add("@DisplayOrder", model.DisplayOrder);
                param.Add("@GridColumn", model.GridColumn);
                param.Add("@OnTable", model.OnTable);
                param.Add("@CompanyId", model.CompanyID);
                param.Add("@SessionId", model.SessionID);
                param.Add("@UserId", userId);

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Mst_Fields_Upsert",
                    param,
                    commandType: CommandType.StoredProcedure);

                if (result == null)
                    return (false, "No response received from database.");

                return (
                    Convert.ToInt32(result.Result) == 1,
                    Convert.ToString(result.Message) ?? string.Empty
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Deletes the specified field record from the database.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the field to be deleted.
        /// </param>
        /// <param name="userId">
        /// The ID of the user performing the delete operation.
        /// </param>
        /// <returns>
        /// Returns a tuple containing the operation status and result message.
        /// </returns>
        public (bool Success, string Message) DeleteField(List<int> id, int userId)
        {
            try
            {
                if (id == null || !id.Any())
                {
                    return (false, "No students selected for deletion.");
                }

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string fieldIds = string.Join(",", id);

                var param = new DynamicParameters();

                param.Add("@FieldId", fieldIds);
                param.Add("@UserId", userId);

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Mst_Fields_Delete",
                    param,
                    commandType: CommandType.StoredProcedure);

                if (result == null)
                    return (false, "No response received from database.");

                return (
                    Convert.ToInt32(result.Result) == 1,
                    Convert.ToString(result.Message) ?? string.Empty
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Activates or deactivates the specified field record.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the field.
        /// </param>
        /// <param name="isActive">
        /// Indicates whether the field should be active or inactive.
        /// </param>
        /// <param name="userId">
        /// The ID of the user performing the status change operation.
        /// </param>
        /// <returns>
        /// Returns a tuple containing the operation status and result message.
        /// </returns>
        public (bool Success, string Message) ToggleFieldStatus(
            int id,
            bool isActive,
            int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var param = new DynamicParameters();

                param.Add("@FieldId", id);
                param.Add("@IsActive", isActive);
                param.Add("@UserId", userId);

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Mst_Fields_ToggleStatus",
                    param,
                    commandType: CommandType.StoredProcedure);

                if (result == null)
                    return (false, "No response received from database.");

                return (
                    Convert.ToInt32(result.Result) == 1,
                    Convert.ToString(result.Message) ?? string.Empty
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Retrieves all ID auto-generation settings for the specified
        /// company and session.
        /// </summary>
        /// <param name="companyId">
        /// The company identifier.
        /// </param>
        /// <param name="sessionId">
        /// The session identifier.
        /// </param>
        /// <returns>
        /// Returns a list of ID auto-generation configuration settings.
        /// </returns>
        public async Task<List<IDAutoGenSettings>> GetIDAutoGenSettings(
            int companyId,
            int sessionId)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var param = new DynamicParameters();

            param.Add("@CompanyId", companyId);
            param.Add("@SessionId", sessionId);

            var result = await conn.QueryAsync<IDAutoGenSettings>(
                "sp_Settings_IDAutoGen_Get",
                param,
                commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        /// <summary>
        /// Creates or updates ID auto-generation settings for the specified entity.
        /// </summary>
        /// <param name="request">
        /// Contains ID generation configuration details such as prefix,
        /// digit count, start number, and fields to include.
        /// </param>
        /// <param name="userId">
        /// The ID of the user performing the save operation.
        /// </param>
        /// <returns>
        /// Returns a tuple indicating whether the operation was successful
        /// along with a status message.
        /// </returns>
        public (bool Success, string Message) SaveIDAutoGenSettings(
            IDAutoGenRequest request,
            int userId)
        {
            try
            {
                // Convert selected fields into a comma-separated string.
                var fields = request.FieldsToInclude != null
                    ? string.Join(",", request.FieldsToInclude)
                    : string.Empty;

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var param = new DynamicParameters();

                param.Add("@EntityType", request.EntityType);
                param.Add("@IsEnabled", request.IsEnabled);
                param.Add("@Prefix", request.Prefix);
                param.Add("@DigitCount", request.DigitCount);
                param.Add("@StartNo", request.StartNo);
                param.Add("@FieldsToInclude", fields);
                param.Add("@CompanyId", request.CompanyID);
                param.Add("@SessionID", request.SessionID);
                param.Add("@UserId", userId);

                conn.Execute(
                    "sp_Settings_IDAutoGen_Upsert",
                    param,
                    commandType: CommandType.StoredProcedure);

                return (true, "Settings saved successfully.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
