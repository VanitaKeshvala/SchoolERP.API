using Dapper;
using Microsoft.Data.SqlClient;
using SchoolERP.API.Data;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using System.Data;
using System.Data.Common;

namespace SchoolERP.API.Services
{
    public class AlumniEventService: IAlumniEventService
    {
        private readonly IConfiguration _configuration;
        public AlumniEventService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<AlumniEventViewModel> GetEvents(string? searchText, int companyId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Action", "LIST");
            parameters.Add("@CompanyID", companyId);
            parameters.Add("@SearchText", searchText);

            using var conn = new SqlConnection(
                  _configuration.GetConnectionString("DefaultConnection")); // or however you get IDbConnection
            var list = conn.Query<AlumniEventViewModel>(
                "sp_AlumniEvents_CRUD",
                parameters,
                commandType: CommandType.StoredProcedure
            ).ToList();

            return list;
        }

        /// <summary>
        /// Inserts or updates an alumni event record in the database.
        /// Calls the stored procedure <c>sp_AlumniEvents_CRUD</c> with action "UPSERT".
        /// </summary>
        /// <param name="req">The request object containing all event details to upsert.</param>
        /// <param name="companyId">The ID of the company performing the operation.</param>
        /// <param name="userId">The ID of the user performing the operation.</param>
        /// <returns>
        /// A tuple where <c>Success</c> is <c>true</c> if the upsert succeeded (Result == 1),
        /// and <c>Message</c> contains the response message from the stored procedure.
        /// </returns>
        public (bool Success, string Message) UpsertEvent(AlumniEventUpsertRequest req, int companyId, int userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Action", "UPSERT");
            parameters.Add("@EventID", req.EventID);
            parameters.Add("@EventTitle", req.EventTitle);
            parameters.Add("@EventDescription", req.EventDescription);
            parameters.Add("@FromDate", req.FromDate);
            parameters.Add("@ToDate", req.ToDate);
            parameters.Add("@Location", req.Location);
            parameters.Add("@EventPhoto", req.EventPhoto, DbType.Binary);
            parameters.Add("@EventPhotoType", req.EventPhotoType);
            parameters.Add("@EventPhotoName", req.EventPhotoName);
            parameters.Add("@EventFor", req.EventFor);
            parameters.Add("@SessionID", req.SessionID);
            parameters.Add("@ClassID", req.ClassID);
            parameters.Add("@SectionIDs", req.SectionIDs);
            parameters.Add("@CompanyID", companyId);
            parameters.Add("@UserID", userId);

            using var conn = new SqlConnection(
                  _configuration.GetConnectionString("DefaultConnection"));  // or however you get IDbConnection
            var result = conn.QueryFirstOrDefault(
                "sp_AlumniEvents_CRUD",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            if (result != null)
            {
                return (Convert.ToInt32(result.Result) == 1, (string)result.Message);
            }

            return (false, "Operation failed");
        }

        /// <summary>
        /// Deletes an alumni event from the database for the specified company and user.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event to delete.</param>
        /// <param name="companyId">The unique identifier of the company associated with the event.</param>
        /// <param name="userId">The unique identifier of the user performing the deletion.</param>
        /// <returns>A tuple containing a Success flag (true if deleted successfully) and a Message describing the result.</returns>
        public (bool Success, string Message) DeleteEvent(int eventId, int companyId, int userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Action", "DELETE");
            parameters.Add("@EventID", eventId);
            parameters.Add("@UserID", userId);
            parameters.Add("@CompanyID", companyId);
            using var conn = new SqlConnection(
                  _configuration.GetConnectionString("DefaultConnection"));
            var result = conn.QueryFirstOrDefault<(int Result, string Message)>(
                "sp_AlumniEvents_CRUD",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            if (result != default)
            {
                return (result.Result == 1, result.Message);
            }

            return (false, "Deletion failed");
        }

        /// <summary>
        /// Toggles the active/inactive status of an alumni event.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event.</param>
        /// <param name="isActive">The new active status to set for the event.</param>
        /// <param name="companyId">The ID of the company associated with the event.</param>
        /// <param name="userId">The ID of the user performing the status toggle.</param>
        /// <returns>A tuple containing Success (bool) and Message (string) indicating the result.</returns>
        public (bool Success, string Message) ToggleEventStatus(int eventId, bool isActive, int companyId, int userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Action", "TOGGLE_STATUS");
            parameters.Add("@EventID", eventId);
            parameters.Add("@IsActive", isActive);
            parameters.Add("@CompanyID", companyId);
            parameters.Add("@UserID", userId);
            using var conn = new SqlConnection(
                  _configuration.GetConnectionString("DefaultConnection"));
            var result = conn.QueryFirstOrDefault(
                "sp_AlumniEvents_CRUD",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            if (result != null)
            {
                return (Convert.ToInt32(result.Result) == 1, (string)result.Message);
            }
            return (false, "Status update failed");
        }

        /// <summary>
        /// Retrieves the photo associated with a specific alumni event.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event.</param>
        /// <param name="companyId">The unique identifier of the company.</param>
        /// <returns>
        /// A tuple containing:
        /// - <c>Bytes</c>: The raw byte array of the event photo, or <c>null</c> if not found.
        /// - <c>FileName</c>: The file name of the photo, or <c>null</c> if not found.
        /// - <c>ContentType</c>: The MIME content type of the photo, or <c>null</c> if not found.
        /// </returns>
        public (byte[]? Bytes, string? FileName, string? ContentType) GetEventPhoto(int eventId, int companyId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Action", "GET_BY_ID");
            parameters.Add("@EventID", eventId);
            parameters.Add("@CompanyID", companyId);

            using var conn = new SqlConnection(
                  _configuration.GetConnectionString("DefaultConnection"));

            var row = conn.QueryFirstOrDefault(
                "sp_AlumniEvents_CRUD",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            if (row != null && row.EventPhoto != null)
            {
                return ((byte[])row.EventPhoto, (string?)row.EventPhotoName, (string?)row.EventPhotoType);
            }

            return (null, null, null);
        }
    }
}
