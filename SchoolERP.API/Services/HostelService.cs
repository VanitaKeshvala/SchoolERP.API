using Dapper;
using Microsoft.Data.SqlClient;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using System.Data;

namespace SchoolERP.API.Services
{
    /// <summary>
    /// This service handles the actual work of managing hostel facilities, such as saving, updating, or deleting hostel buildings and room records in the database.
    /// </summary>
    public class HostelService: IHostelService
    {
        private readonly IConfiguration _configuration;
        public HostelService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ─── ROOM TYPE ──────────────────────────────────────────

        /// <summary>
        /// Retrieves all room types for the specified company and session.
        /// </summary>
        /// <param name="companyId">Company/School ID.</param>
        /// <param name="sessionId">Academic session ID.</param>
        /// <param name="includeDeleted">Whether to include deleted records.</param>
        /// <returns>List of room types.</returns>
        public List<RoomTypeViewModel> GetAllRoomTypes(int companyId, int sessionId, bool includeDeleted = false)
        {
            using var conn = new SqlConnection(
                     _configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();
            parameters.Add("@CompanyID", companyId);
            parameters.Add("@SessionID", sessionId);
            parameters.Add("@IncludeDeleted", includeDeleted);

            var result= conn.Query<RoomTypeViewModel>(
                "sp_Mst_RoomType_GetAll",
                parameters,
                commandType: CommandType.StoredProcedure
            ).ToList();

            // If SP returned no rows at all
            if (!result.Any()) return null;

            // If SP returned rows but RESULT != 1 (failure case)
            if (result.First().Result != 1) return null;

            return result;
        }

        /// <summary>
        /// Retrieves room type details by its unique ID.
        /// </summary>
        /// <param name="id">Room Type ID.</param>
        /// <returns>Room type details if found; otherwise null.</returns>
        public RoomTypeViewModel? GetRoomTypeByID(int id)
        {
            using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();
            parameters.Add("@RoomTypeID", id);

            var result= conn.QueryFirstOrDefault<RoomTypeViewModel>(
                "sp_Mst_RoomType_GetByID",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            // If SP returned no rows at all
            if (result == null) return null;

            // If SP returned rows but RESULT != 1 (failure case)
            if (result.Result != 1) return null;

            return result;
        }

        /// <summary>
        /// Creates a new room type or updates an existing room type.
        /// </summary>
        /// <param name="req">Room type information.</param>
        /// <param name="companyId">Company/School ID.</param>
        /// <param name="sessionId">Academic session ID.</param>
        /// <param name="userId">Logged-in user ID.</param>
        /// <returns>Operation status and message.</returns>
        public (bool Success, string Message) UpsertRoomType(
            RoomTypeUpsertRequest req,
            int companyId,
            int sessionId,
            int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var parameters = new DynamicParameters();
                parameters.Add("@RoomTypeID", req.RoomTypeID);
                parameters.Add("@CompanyID", companyId);
                parameters.Add("@SessionID", sessionId);
                parameters.Add("@RoomTypeTitle", req.RoomTypeTitle);
                parameters.Add("@RoomTypeDescription", req.RoomTypeDescription);
                parameters.Add("@IsActive", req.IsActive);
                parameters.Add("@UserId", userId);

                var result = conn.QueryFirstOrDefault<dynamic>(
                    "sp_Mst_RoomType_Upsert",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return (
                    Convert.ToInt32(result?.Result) == 1,
                    Convert.ToString(result?.Message) ?? "Operation completed."
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Deletes a room type record by its unique ID.
        /// </summary>
        /// <param name="id">Room Type ID.</param>
        /// <param name="userId">Logged-in user ID.</param>
        /// <returns>Operation status and message.</returns>
        public (bool Success, string Message) DeleteRoomType(List<int> id, int userId)
        {
            try
            {
                if (id == null || !id.Any())
                {
                    return (false, "No students selected for deletion.");
                }

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string roomTypeIDs = string.Join(",", id);

                var parameters = new DynamicParameters();
                parameters.Add("@RoomTypeID", roomTypeIDs);
                parameters.Add("@UserId", userId);

                var result = conn.QueryFirstOrDefault<dynamic>(
                    "sp_Mst_RoomType_Delete",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return (
                    Convert.ToInt32(result?.Result) == 1,
                    Convert.ToString(result?.Message) ?? "Operation completed."
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Activates or deactivates a room type.
        /// </summary>
        /// <param name="id">Room Type ID.</param>
        /// <param name="isActive">Status to set.</param>
        /// <param name="userId">Logged-in user ID.</param>
        /// <returns>Operation status and message.</returns>
        public (bool Success, string Message) ToggleRoomTypeStatus(int id, bool isActive, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var parameters = new DynamicParameters();
                parameters.Add("@RoomTypeID", id);
                parameters.Add("@IsActive", isActive);
                parameters.Add("@UserId", userId);

                var result = conn.QueryFirstOrDefault<dynamic>(
                    "sp_Mst_RoomType_ToggleStatus",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return (
                    Convert.ToInt32(result?.Result) == 1,
                    Convert.ToString(result?.Message) ?? "Operation completed."
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // ─── HOSTEL ─────────────────────────────────────────────

        /// <summary>
        /// Retrieves all hostels for the specified company and session.
        /// </summary>
        /// <param name="companyId">Company/School ID.</param>
        /// <param name="sessionId">Academic session ID.</param>
        /// <param name="includeDeleted">Whether to include deleted records.</param>
        /// <returns>List of hostels.</returns>
        public List<HostelViewModel> GetAllHostels(int companyId, int sessionId, bool includeDeleted = false)
        {
            using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();
            parameters.Add("@CompanyID", companyId);
            parameters.Add("@SessionID", sessionId);
            parameters.Add("@IncludeDeleted", includeDeleted);

            var result= conn.Query<HostelViewModel>(
                "sp_Mst_Hostel_GetAll",
                parameters,
                commandType: CommandType.StoredProcedure
            ).ToList();

            // If SP returned no rows at all
            if (!result.Any()) return null;

            // If SP returned rows but RESULT != 1 (failure case)
            if (result.First().Result != 1) return null;

            return result;
        }

        /// <summary>
        /// Retrieves hostel details by its unique ID.
        /// </summary>
        /// <param name="id">Hostel ID.</param>
        /// <returns>Hostel details if found; otherwise null.</returns>
        public HostelViewModel? GetHostelByID(int id)
        {
            using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();
            parameters.Add("@HostelID", id);

            return conn.QueryFirstOrDefault<HostelViewModel>(
                "sp_Mst_Hostel_GetByID",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        // ─── HOSTEL ─────────────────────────────────────────────

        /// <summary>
        /// Creates a new hostel or updates an existing hostel record.
        /// </summary>
        /// <param name="req">Hostel information to save.</param>
        /// <param name="companyId">Company/School ID.</param>
        /// <param name="sessionId">Academic session ID.</param>
        /// <param name="userId">Logged-in user ID.</param>
        /// <returns>Operation status and message.</returns>
        public (bool Success, string Message) UpsertHostel(
            HostelUpsertRequest req,
            int companyId,
            int sessionId,
            int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var parameters = new DynamicParameters();
                parameters.Add("@HostelID", req.HostelID);
                parameters.Add("@CompanyID", companyId);
                parameters.Add("@SessionID", sessionId);
                parameters.Add("@HostelName", req.HostelName);
                parameters.Add("@RoomTypeID", req.RoomTypeID);
                parameters.Add("@HostelAddress", req.HostelAddress);
                parameters.Add("@HostelIntake", req.HostelIntake);
                parameters.Add("@HostelDescription", req.HostelDescription);
                parameters.Add("@IsActive", req.IsActive);
                parameters.Add("@UserId", userId);

                var result = conn.QueryFirstOrDefault<dynamic>(
                    "sp_Mst_Hostel_Upsert",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return (
                    Convert.ToInt32(result?.Result) == 1,
                    Convert.ToString(result?.Message) ?? "Operation completed."
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Deletes a hostel record by its unique ID.
        /// </summary>
        /// <param name="id">Hostel ID.</param>
        /// <param name="userId">Logged-in user ID.</param>
        /// <returns>Operation status and message.</returns>
        public (bool Success, string Message) DeleteHostel(List<int> id, int userId)
        {
            try
            {
                if (id == null || !id.Any())
                {
                    return (false, "No students selected for deletion.");
                }

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string hostelIDs = string.Join(",", id);

                var parameters = new DynamicParameters();
                parameters.Add("@HostelID", hostelIDs);
                parameters.Add("@UserId", userId);

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Mst_Hostel_Delete",
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
        /// Activates or deactivates a hostel record.
        /// </summary>
        /// <param name="id">Hostel ID.</param>
        /// <param name="isActive">Status to set.</param>
        /// <param name="userId">Logged-in user ID.</param>
        /// <returns>Operation status and message.</returns>
        public (bool Success, string Message) ToggleHostelStatus(int id, bool isActive, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var parameters = new DynamicParameters();
                parameters.Add("@HostelID", id);
                parameters.Add("@IsActive", isActive);
                parameters.Add("@UserId", userId);

                var result = conn.QueryFirstOrDefault<dynamic>(
                    "sp_Mst_Hostel_ToggleStatus",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return (
                    Convert.ToInt32(result?.Result) == 1,
                    Convert.ToString(result?.Message) ?? "Operation completed."
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // ─── HOSTEL ROOM ────────────────────────────────────────

        /// <summary>
        /// Retrieves all hostel rooms for the specified company and session.
        /// </summary>
        /// <param name="companyId">Company/School ID.</param>
        /// <param name="sessionId">Academic session ID.</param>
        /// <param name="includeDeleted">Whether to include deleted records.</param>
        /// <returns>List of hostel rooms.</returns>
        public List<HostelRoomViewModel> GetAllHostelRooms(
            int companyId,
            int sessionId,
            bool includeDeleted = false)
        {
            using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();
            parameters.Add("@CompanyID", companyId);
            parameters.Add("@SessionID", sessionId);
            parameters.Add("@IncludeDeleted", includeDeleted);

            var result= conn.Query<HostelRoomViewModel>(
                "sp_Mst_HostelRoom_GetAll",
                parameters,
                commandType: CommandType.StoredProcedure
            ).ToList();

            // If SP returned no rows at all
            if (!result.Any()) return null;

            // If SP returned rows but RESULT != 1 (failure case)
            if (result.First().Result != 1) return null;

            return result;
        }

        /// <summary>
        /// Retrieves hostel room details by its unique ID.
        /// </summary>
        /// <param name="id">Room ID.</param>
        /// <returns>Hostel room details if found; otherwise null.</returns>
        public HostelRoomViewModel? GetHostelRoomByID(int id)
        {
            using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();
            parameters.Add("@RoomId", id);

            return conn.QueryFirstOrDefault<HostelRoomViewModel>(
                "sp_Mst_HostelRoom_GetByID",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        /// <summary>
        /// Creates a new hostel room or updates an existing hostel room.
        /// </summary>
        /// <param name="req">Hostel room information.</param>
        /// <param name="companyId">Company/School ID.</param>
        /// <param name="sessionId">Academic session ID.</param>
        /// <param name="userId">Logged-in user ID.</param>
        /// <returns>Operation status and message.</returns>
        public (bool Success, string Message) UpsertHostelRoom(
            HostelRoomUpsertRequest req,
            int companyId,
            int sessionId,
            int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var parameters = new DynamicParameters();
                parameters.Add("@RoomId", req.RoomId);
                parameters.Add("@HostelID", req.HostelID);
                parameters.Add("@CompanyID", companyId);
                parameters.Add("@SessionID", sessionId);
                parameters.Add("@RoomTypeID", req.RoomTypeID);
                parameters.Add("@RoomTitle", req.RoomTitle);
                parameters.Add("@NoOfBed", req.NoOfBed);
                parameters.Add("@CostPerBed", req.CostPerBed);
                parameters.Add("@RoomDescription", req.RoomDescription);
                parameters.Add("@IsActive", req.IsActive);
                parameters.Add("@UserId", userId);

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Mst_HostelRoom_Upsert",
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
        /// Deletes a hostel room record by its unique ID.
        /// </summary>
        /// <param name="id">Room ID.</param>
        /// <param name="userId">Logged-in user ID.</param>
        /// <returns>Operation status and message.</returns>
        public (bool Success, string Message) DeleteHostelRoom(List<int> id, int userId)
        {
            try
            {
                if (id == null || !id.Any())
                {
                    return (false, "No students selected for deletion.");
                }

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string roomIds = string.Join(",", id);

                var parameters = new DynamicParameters();
                parameters.Add("@RoomId", roomIds);
                parameters.Add("@UserId", userId);

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Mst_HostelRoom_Delete",
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
        /// Activates or deactivates a hostel room.
        /// </summary>
        /// <param name="id">Room ID.</param>
        /// <param name="isActive">Status to set.</param>
        /// <param name="userId">Logged-in user ID.</param>
        /// <returns>Operation status and message.</returns>
        public (bool Success, string Message) ToggleHostelRoomStatus(
            int id,
            bool isActive,
            int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var parameters = new DynamicParameters();
                parameters.Add("@RoomId", id);
                parameters.Add("@IsActive", isActive);
                parameters.Add("@UserId", userId);

                var result = conn.QueryFirstOrDefault<dynamic>(
                    "sp_Mst_HostelRoom_ToggleStatus",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return (
                    Convert.ToInt32(result?.Result) == 1,
                    Convert.ToString(result?.Message) ?? "Operation completed."
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

    }
}
