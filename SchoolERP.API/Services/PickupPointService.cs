using Dapper;
using Microsoft.Data.SqlClient;
using SchoolERP.API.Data;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using System.Data;

namespace SchoolERP.API.Services
{
    /// <summary>
    /// This service handles the actual work of managing transport pickup points, such as saving locations where the school bus stops.
    /// </summary>
    public class PickupPointService: IPickupPointService
    {
        private readonly IConfiguration _configuration;
        public PickupPointService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Retrieves all pickup points for the specified company and session.
        /// </summary>
        public List<PickupPointViewModel> GetAllPickupPoints(int companyId, int sessionId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                return conn.Query<PickupPointViewModel>(
                    "sp_Mst_PickupPoint_GetAll",
                    new
                    {
                        CompanyID = companyId,
                        SessionID = sessionId
                    },
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
            catch
            {
                return new List<PickupPointViewModel>();
            }
        }

        /// <summary>
        /// Retrieves pickup point details by pickup point ID.
        /// </summary>
        public PickupPointViewModel? GetPickupPointByID(int id)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.QueryFirstOrDefault<PickupPointViewModel>(
                "sp_Mst_PickupPoint_GetByID",
                new
                {
                    PickupPointID = id
                },
                commandType: CommandType.StoredProcedure
            );
        }

        /// <summary>
        /// Inserts or updates a pickup point record.
        /// </summary>
        public (bool Success, string Message) UpsertPickupPoint(
            PickupPointUpsertRequest req,
            int companyId,
            int sessionId,
            int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault(
                    "sp_Mst_PickupPoint_Upsert",
                    new
                    {
                        req.PickupPointID,
                        CompanyID = companyId,
                        SessionID = sessionId,
                        req.PickupPointName,
                        req.PickupPointLatitude,
                        req.PickupPointLongitude,
                        req.IsActive,
                        UserID = userId
                    },
                    commandType: CommandType.StoredProcedure
                );

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
        /// Deletes a pickup point record.
        /// </summary>
        public (bool Success, string Message) DeletePickupPoint(List<int> id, int userId)
        {
            try
            {
                if (id == null || !id.Any())
                {
                    return (false, "No students selected for deletion.");
                }

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string pickupPointIDs = string.Join(",", id);

                var result = conn.QueryFirstOrDefault(
                    "sp_Mst_PickupPoint_Delete",
                    new
                    {
                        PickupPointID = pickupPointIDs,
                        UserID = userId
                    },
                    commandType: CommandType.StoredProcedure
                );

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
        /// Activates or deactivates a pickup point.
        /// </summary>
        public (bool Success, string Message) TogglePickupPointStatus(
            int id,
            bool isActive,
            int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault(
                    "sp_Mst_PickupPoint_ToggleStatus",
                    new
                    {
                        PickupPointID = id,
                        IsActive = isActive,
                        UserID = userId
                    },
                    commandType: CommandType.StoredProcedure
                );

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
    }
}
