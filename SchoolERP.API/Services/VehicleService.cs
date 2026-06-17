using Dapper;
using Microsoft.Data.SqlClient;
using SchoolERP.API.Data;
using SchoolERP.API.Interfaces;
using SchoolERP.API.Models;
using System.Data;

namespace SchoolERP.API.Services
{
    /// <summary>
    /// This service handles the actual work of managing school vehicles, such as saving, updating, or deleting vehicle records in the database.
    /// </summary>
    public class VehicleService: IVehicleService
    {
        private readonly IConfiguration _configuration;
        public VehicleService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Retrieves all vehicle records for the specified company and session.
        /// </summary>
        public List<VehicleViewModel> GetAllVehicles(int companyId, int sessionId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                return conn.Query<VehicleViewModel>(
                    "sp_Mst_Vehicles_GetAll",
                    new
                    {
                        CompanyID = companyId,
                        SessionID = sessionId
                    },
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
            catch (Exception)
            {
                return new List<VehicleViewModel>();
            }
        }

        /// <summary>
        /// Looks up the details of a specific vehicle using its unique ID.
        /// </summary>
        public VehicleViewModel? GetVehicleByID(int id)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.QueryFirstOrDefault<VehicleViewModel>(
                "sp_Mst_Vehicles_GetByID",
                new
                {
                    VehicleID = id
                },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Saves a new vehicle or updates an existing vehicle record.
        /// </summary>
        public (bool Success, string Message) UpsertVehicle(
            VehicleUpsertRequest req,
            int companyId,
            int sessionId,
            int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<(int Result, string Message)>(
                    "sp_Mst_Vehicles_Upsert",
                    new
                    {
                        req.VehicleID,
                        CompanyID = companyId,
                        SessionID = sessionId,
                        req.VehicleNumber,
                        req.VehicleModel,
                        req.VehicleYearMade,
                        req.VehicleRegNo,
                        req.VehicleChasisNo,
                        req.VehicleMaxCapicity,
                        req.VehicleDriverName,
                        req.VehicleDriverLicense,
                        req.VehicleDriverContact,
                        req.VehicleDriverPhotoAttach,
                        req.VehicleDriverPhotoName,
                        req.VehicleDriverPhotoType,
                        req.VehicleNote,
                        req.IsActive,
                        UserID = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return (result.Result == 1, result.Message);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        /// <summary>
        /// Deletes a vehicle record from the system.
        /// </summary>
        public (bool Success, string Message) DeleteVehicle(List<int> id, int userId)
        {
            try
            {
                if (id == null || !id.Any())
                {
                    return (false, "No students selected for deletion.");
                }

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string vehicleIDs = string.Join(",", id);

                var result = conn.QueryFirstOrDefault<(int Result, string Message)>(
                    "sp_Mst_Vehicles_Delete",
                    new
                    {
                        VehicleID = vehicleIDs,
                        UserID = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return (result.Result == 1, result.Message);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        /// <summary>
        /// Updates the active or inactive status of a vehicle.
        /// </summary>
        public (bool Success, string Message) ToggleVehicleStatus(
            int id,
            bool isActive,
            int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<(int Result, string Message)>(
                    "sp_Mst_Vehicles_ToggleStatus",
                    new
                    {
                        VehicleID = id,
                        IsActive = isActive,
                        UserID = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return (result.Result == 1, result.Message);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
       
    }
}
