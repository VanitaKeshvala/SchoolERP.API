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
    /// This service handles the actual work of managing Front Office operations, such as saving visit purposes, complaints, postal records, and call logs in the database.
    /// </summary>
    public class FrontOfficeService : IFrontOfficeService
    {
        private readonly IConfiguration _configuration;
        public FrontOfficeService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ─── HELPERS ────────────────────────────────────────────
        private static T? SafeGet<T>(DataRow row, string col)
        {
            if (!row.Table.Columns.Contains(col) || row[col] == DBNull.Value) return default;
            return (T)Convert.ChangeType(row[col], typeof(T));
        }

        // ─── PURPOSE ────────────────────────────────────────────
        /// <summary>
        /// Retrieves all follow-up purposes for the specified company and session.
        /// </summary>
        /// <param name="companyId">Company ID.</param>
        /// <param name="sessionId">Session ID.</param>
        /// <param name="includeDeleted">Whether deleted records should be included.</param>
        /// <returns>List of purpose records.</returns>
        public List<MstFOPurposeViewModel> GetAllPurposes(int companyId, int sessionId, bool includeDeleted = false)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var param = new
            {
                CompanyID = companyId,
                SessionID = sessionId,
                IncludeDeleted = includeDeleted
            };

            return conn.Query<MstFOPurposeViewModel>(
                "sp_FO_Purpose_GetAll",
                param,
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        /// <summary>
        /// Retrieves a purpose record by its ID.
        /// </summary>
        /// <param name="id">Purpose ID.</param>
        /// <returns>
        /// Returns the purpose details if found; otherwise null.
        /// </returns>
        public MstFOPurposeViewModel? GetPurposeByID(int id)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.QueryFirstOrDefault<MstFOPurposeViewModel>(
                "sp_FO_Purpose_GetByID",
                new { PurposeID = id },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Inserts or updates a Fee Office Purpose record.
        /// </summary>
        /// <param name="req">Purpose details.</param>
        /// <param name="companyId">Company ID.</param>
        /// <param name="sessionId">Session ID.</param>
        /// <param name="userId">Logged-in User ID.</param>
        /// <returns>
        /// Returns:
        /// Item1 = Success status.
        /// Item2 = Success/Error message.
        /// </returns>
        public (bool, string) UpsertPurpose(MstFOPurposeUpsertRequest req, int companyId, int sessionId, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault(
                    "sp_FO_Purpose_Upsert",
                    new
                    {
                        req.PurposeID,
                        CompanyID = companyId,
                        SessionID = sessionId,
                        req.Name,
                        req.Description,
                        req.IsActive,
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
        /// <summary>
        /// Deletes a purpose record by PurposeID.
        /// </summary>
        /// <param name="id">Purpose ID to delete.</param>
        /// <param name="userId">User performing the delete operation.</param>
        /// <returns>
        /// Item1 = Success status.
        /// Item2 = Result message from stored procedure.
        /// </returns>
        public (bool, string) DeletePurpose(List<int> id, int userId)
        {
            try
            {
                if (id == null || !id.Any())
                {
                    return (false, "No students selected for deletion.");
                }

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string purposeID = string.Join(",", id);

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_FO_Purpose_Delete",
                    new
                    {
                        PurposeID = purposeID,
                        UserId = userId
                    },
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
        /// Toggles the active/inactive status of a purpose.
        /// </summary>
        /// <param name="id">Purpose ID.</param>
        /// <param name="isActive">Status to set.</param>
        /// <param name="userId">User performing the action.</param>
        /// <returns>
        /// Item1 = Success flag.
        /// Item2 = Result message.
        /// </returns>
        public (bool, string) TogglePurposeStatus(int id, bool isActive, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<(int Result, string Message)>(
                    "sp_FO_Purpose_ToggleStatus",
                    new
                    {
                        PurposeID = id,
                        IsActive = isActive,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return (result.Result == 1, result.Message);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        

        // ─── COMPLAINT TYPE ─────────────────────────────────────
        /// <summary>
        /// Retrieves all complaint types for the specified company and session.
        /// </summary>
        /// <param name="companyId">Company ID.</param>
        /// <param name="sessionId">Session ID.</param>
        /// <param name="includeDeleted">Whether deleted records should be included.</param>
        /// <returns>List of complaint types.</returns>
        public List<MstFOComplaintTypeViewModel> GetAllComplaintTypes(int companyId, int sessionId, bool includeDeleted = false)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var parameters = new
            {
                CompanyID = companyId,
                SessionID = sessionId,
                IncludeDeleted = includeDeleted
            };

            return conn.Query<MstFOComplaintTypeViewModel>(
                "sp_FO_ComplaintType_GetAll",
                parameters,
                commandType: CommandType.StoredProcedure)
                .ToList();
        }
        /// <summary>
        /// Retrieves complaint type details by Complaint Type ID.
        /// </summary>
        /// <param name="id">Complaint Type ID.</param>
        /// <returns>
        /// Returns complaint type details if found; otherwise null.
        /// </returns>
        public MstFOComplaintTypeViewModel? GetComplaintTypeByID(int id)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.QueryFirstOrDefault<MstFOComplaintTypeViewModel>(
                "sp_FO_ComplaintType_GetByID",
                new { ComplaintTypeID = id },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Inserts a new complaint type or updates an existing complaint type.
        /// Executes the stored procedure <c>sp_FO_ComplaintType_Upsert</c>
        /// and returns the operation status and message.
        /// </summary>
        /// <param name="req">Complaint type details.</param>
        /// <param name="companyId">Company identifier.</param>
        /// <param name="sessionId">Session identifier.</param>
        /// <param name="userId">Current user identifier.</param>
        /// <returns>
        /// Tuple containing:
        /// <list type="bullet">
        /// <item><description><c>true</c> if operation succeeds; otherwise <c>false</c>.</description></item>
        /// <item><description>Success or error message.</description></item>
        /// </list>
        /// </returns>
        public (bool, string) UpsertComplaintType(
            MstFOComplaintTypeUpsertRequest req,
            int companyId,
            int sessionId,
            int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<dynamic>(
                    "sp_FO_ComplaintType_Upsert",
                    new
                    {
                        ComplaintTypeID = req.ComplaintTypeID,
                        CompanyID = companyId,
                        SessionID = sessionId,
                        Name = req.Name,
                        Description = req.Description,
                        IsActive = req.IsActive,
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

        /// <summary>
        /// Deletes a complaint type by its ID.
        /// Calls the database stored procedure and returns the operation result and message.
        /// </summary>
        /// <param name="id">Complaint Type ID.</param>
        /// <param name="userId">User performing the delete operation.</param>
        /// <returns>
        /// Item1: True if deleted successfully; otherwise False.
        /// Item2: Success or error message.
        /// </returns>
        public (bool, string) DeleteComplaintType(List<int> id, int userId)
        {
            try
            {
                if (id == null || !id.Any())
                {
                    return (false, "No complaint type selected for deletion.");
                }

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string complaintTypeIDs = string.Join(",", id);

                //var result = conn.QueryFirstOrDefault(
                //    "sp_FO_ComplaintType_Delete",
                //    new
                //    {
                //        ComplaintTypeID = complaintTypeIDs,
                //        UserId = userId
                //    },
                //commandType: CommandType.StoredProcedure);

                var parameters = new DynamicParameters();
                parameters.Add("@COMPLAINTTYPEID", complaintTypeIDs);
                parameters.Add("@USERID", userId);
                var result = conn.QueryFirstOrDefault<SpResult>(
                   "sp_FO_ComplaintType_Delete",
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
        /// Toggles the active/inactive status of a complaint type.
        /// </summary>
        /// <param name="id">Complaint Type ID.</param>
        /// <param name="isActive">Status to set (true = Active, false = Inactive).</param>
        /// <param name="userId">User performing the action.</param>
        /// <returns>
        /// Returns a tuple containing:
        /// Item1 = Success status.
        /// Item2 = Result message.
        /// </returns>
        public (bool, string) ToggleComplaintTypeStatus(int id, bool isActive, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault(
                    "sp_FO_ComplaintType_ToggleStatus",
                    new
                    {
                        ComplaintTypeID = id,
                        IsActive = isActive,
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

        

        // ─── SOURCE ─────────────────────────────────────────────
        /// <summary>
        /// Retrieves all Front Office Sources for the specified company and session.
        /// </summary>
        /// <param name="companyId">Company ID.</param>
        /// <param name="sessionId">Session ID.</param>
        /// <param name="includeDeleted">Whether to include deleted records.</param>
        /// <returns>List of source records.</returns>
        public List<MstFOSourceViewModel> GetAllSources(int companyId, int sessionId, bool includeDeleted = false)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.Query<MstFOSourceViewModel>(
                "sp_FO_Source_GetAll",
                new
                {
                    CompanyID = companyId,
                    SessionID = sessionId,
                    IncludeDeleted = includeDeleted
                },
                commandType: CommandType.StoredProcedure
            ).ToList();
        }
        /// <summary>
        /// Looks up the details of a specific inquiry source using its unique ID.
        /// </summary>
        /// <summary>
        /// Retrieves source details by Source ID.
        /// </summary>
        /// <param name="id">Source ID.</param>
        /// <returns>Source details if found; otherwise null.</returns>
        public MstFOSourceViewModel? GetSourceByID(int id)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.QueryFirstOrDefault<MstFOSourceViewModel>(
                "sp_FO_Source_GetByID",
                new { SourceID = id },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Inserts or updates a Front Office Source record.
        /// Calls sp_FO_Source_Upsert and returns the operation status and message.
        /// </summary>
        /// <param name="req">Source details to save.</param>
        /// <param name="companyId">Company identifier.</param>
        /// <param name="sessionId">Session identifier.</param>
        /// <param name="userId">Current user identifier.</param>
        /// <returns>
        /// Item1 = Success status.
        /// Item2 = Response message.
        /// </returns>
        public (bool, string) UpsertSource(MstFOSourceUpsertRequest req, int companyId, int sessionId, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault(
                    "sp_FO_Source_Upsert",
                    new
                    {
                        req.SourceID,
                        CompanyID = companyId,
                        SessionID = sessionId,
                        req.Name,
                        Description = req.Description,
                        req.IsActive,
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
        /// <summary>
        /// Deletes a source record by Source ID.
        /// </summary>
        /// <param name="id">Source ID.</param>
        /// <param name="userId">User ID performing the delete.</param>
        /// <returns>
        /// Returns:
        /// Item1 = Success status.
        /// Item2 = Result message.
        /// </returns>
        public (bool, string) DeleteSource(List<int> id, int userId)
        {
            try
            {
                if (id == null || !id.Any())
                {
                    return (false, "No students selected for deletion.");
                }

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string sourceIDs = string.Join(",", id);

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_FO_Source_Delete",
                    new
                    {
                        SourceID = sourceIDs,
                        UserId = userId
                    },
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
        /// Toggles the active/inactive status of a source.
        /// Calls sp_FO_Source_ToggleStatus and returns the result status and message.
        /// </summary>
        /// <param name="id">Source ID.</param>
        /// <param name="isActive">Status to set (true = Active, false = Inactive).</param>
        /// <param name="userId">User performing the action.</param>
        /// <returns>
        /// Item1 = Success flag.
        /// Item2 = Result message.
        /// </returns>
        public (bool, string) ToggleSourceStatus(int id, bool isActive, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<(int Result, string Message)>(
                    "sp_FO_Source_ToggleStatus",
                    new
                    {
                        SourceID = id,
                        IsActive = isActive,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return (result.Result == 1, result.Message ?? string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
       

        // ─── REFERENCE ──────────────────────────────────────────
        /// <summary>
        /// Retrieves all FO references for the specified company and session.
        /// Optionally includes deleted records.
        /// </summary>
        /// <param name="companyId">Company identifier.</param>
        /// <param name="sessionId">Session identifier.</param>
        /// <param name="includeDeleted">Whether deleted records should be included.</param>
        /// <returns>List of reference records.</returns>
        public List<MstFOReferenceViewModel> GetAllReferences(
            int companyId,
            int sessionId,
            bool includeDeleted = false)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.Query<MstFOReferenceViewModel>(
                "sp_FO_Reference_GetAll",
                new
                {
                    CompanyID = companyId,
                    SessionID = sessionId,
                    IncludeDeleted = includeDeleted
                },
                commandType: CommandType.StoredProcedure
            ).ToList();
        }
        /// <summary>
        /// Retrieves reference details by Reference ID.
        /// </summary>
        /// <param name="id">Reference ID.</param>
        /// <returns>Reference details if found; otherwise null.</returns>
        public MstFOReferenceViewModel? GetReferenceByID(int id)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.QueryFirstOrDefault<MstFOReferenceViewModel>(
                "sp_FO_Reference_GetByID",
                new { ReferenceID = id },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Inserts or updates a Front Office Reference record.
        /// Calls sp_FO_Reference_Upsert and returns the operation status and message.
        /// </summary>
        /// <param name="req">Reference details.</param>
        /// <param name="companyId">Company ID.</param>
        /// <param name="sessionId">Session ID.</param>
        /// <param name="userId">Logged-in User ID.</param>
        /// <returns>
        /// Item1: Success status.
        /// Item2: Result message.
        /// </returns>
        public (bool, string) UpsertReference(MstFOReferenceUpsertRequest req, int companyId, int sessionId, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<(int Result, string Message)>(
                    "sp_FO_Reference_Upsert",
                    new
                    {
                        req.ReferenceID,
                        CompanyID = companyId,
                        SessionID = sessionId,
                        req.Name,
                        Description = req.Description,
                        req.IsActive,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return (result.Result == 1, result.Message ?? string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        /// <summary>
        /// Deletes a reference record by Reference ID.
        /// Calls stored procedure: sp_FO_Reference_Delete.
        /// </summary>
        /// <param name="id">Reference ID to delete.</param>
        /// <param name="userId">User performing the delete operation.</param>
        /// <returns>
        /// Item1 = Success status (true/false)
        /// Item2 = Result message
        /// </returns>
        public (bool, string) DeleteReference(List<int> id, int userId)
        {
            try
            {
                if (id == null || !id.Any())
                {
                    return (false, "No students selected for deletion.");
                }

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string referenceIDs = string.Join(",", id);

                var parameters = new DynamicParameters();
                parameters.Add("@REFERENCEID", referenceIDs);
                parameters.Add("@USERID", userId);
                var result = conn.QueryFirstOrDefault<SpResult>(
                   "sp_FO_Reference_Delete",
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
        /// Toggles the active/inactive status of a reference.
        /// Calls the stored procedure and returns the operation result and message.
        /// </summary>
        /// <param name="id">Reference ID.</param>
        /// <param name="isActive">Status to set (true = Active, false = Inactive).</param>
        /// <param name="userId">User performing the action.</param>
        /// <returns>
        /// Item1: Success status.
        /// Item2: Result message.
        /// </returns>
        public (bool, string) ToggleReferenceStatus(int id, bool isActive, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<(int Result, string Message)>(
                    "sp_FO_Reference_ToggleStatus",
                    new
                    {
                        ReferenceID = id,
                        IsActive = isActive,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return ((result.Result == 1), result.Message ?? string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        
        // ─── COMPLAINT ──────────────────────────────────────────
        /// <summary>
        /// Retrieves all complaints for the specified company and session.
        /// </summary>
        /// <param name="companyId">Company ID.</param>
        /// <param name="sessionId">Session ID.</param>
        /// <param name="includeDeleted">Whether to include deleted complaints.</param>
        /// <returns>List of complaints.</returns>
        public List<FOComplaintViewModel> GetAllComplaints(int companyId, int sessionId, bool includeDeleted = false)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.Query<FOComplaintViewModel>(
                "sp_FO_Complaint_GetAll",
                new
                {
                    CompanyID = companyId,
                    SessionID = sessionId,
                    IncludeDeleted = includeDeleted
                },
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        /// <summary>
        /// Retrieves a complaint by its ID.
        /// </summary>
        /// <param name="id">Complaint ID.</param>
        /// <returns>Complaint details if found; otherwise null.</returns>
        public FOComplaintViewModel? GetComplaintByID(int id)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.QueryFirstOrDefault<FOComplaintViewModel>(
                "sp_FO_Complaint_GetByID",
                new { ComplaintID = id },
                commandType: CommandType.StoredProcedure);
        }
        /// <summary>
        /// Inserts or updates a complaint record.
        /// </summary>
        /// <param name="req">Complaint details.</param>
        /// <param name="companyId">Company ID.</param>
        /// <param name="sessionId">Session ID.</param>
        /// <param name="userId">Current user ID.</param>
        /// <returns>
        /// Tuple containing:
        /// Item1 = Success status.
        /// Item2 = Result message.
        /// </returns>
        public (bool, string) UpsertComplaint(FOComplaintUpsertRequest req, int companyId, int sessionId, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<(int Result, string Message)>(
                    "sp_FO_Complaint_Upsert",
                    new
                    {
                        req.ComplaintID,
                        CompanyID = companyId,
                        SessionID = sessionId,
                        req.ComplaintTypeID,
                        req.SourceID,
                        req.ComplaintBy,
                        req.Phone,
                        req.Email,
                        req.ComplaintDate,
                        req.Description,
                        req.ActionTaken,
                        req.AssignedTo,
                        req.Note,
                        req.Attachment,
                        req.IsActive,
                        UserId = userId
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
        /// Deletes a complaint record by Complaint ID.
        /// Calls stored procedure <c>sp_FO_Complaint_Delete</c>
        /// and returns the operation status and message.
        /// </summary>
        /// <param name="id">Complaint ID.</param>
        /// <param name="userId">User performing the delete operation.</param>
        /// <returns>
        /// Tuple containing:
        /// <list type="bullet">
        /// <item><description><c>bool</c> - Success status.</description></item>
        /// <item><description><c>string</c> - Result message.</description></item>
        /// </list>
        /// </returns>
        public (bool, string) DeleteComplaint(List<int> id, int userId)
        {
            try
            {
                if (id == null || !id.Any())
                {
                    return (false, "No students selected for deletion.");
                }

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string complaintIDs = string.Join(",", id);

                var result = conn.QueryFirstOrDefault<(int Result, string Message)>(
                    "sp_FO_Complaint_Delete",
                    new
                    {
                        ComplaintID = complaintIDs,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return (result.Result == 1, result.Message ?? string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        /// <summary>
        /// Toggles the active/inactive status of a complaint.
        /// Calls the sp_FO_Complaint_ToggleStatus stored procedure and returns
        /// the operation result along with the corresponding message.
        /// </summary>
        /// <param name="id">Complaint ID.</param>
        /// <param name="isActive">Status to set (true = Active, false = Inactive).</param>
        /// <param name="userId">User performing the action.</param>
        /// <returns>
        /// Tuple containing:
        /// Item1 = Success status.
        /// Item2 = Result message.
        /// </returns>
        public (bool, string) ToggleComplaintStatus(int id, bool isActive, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<(int Result, string Message)>(
                    "sp_FO_Complaint_ToggleStatus",
                    new
                    {
                        ComplaintID = id,
                        IsActive = isActive,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return (result.Result == 1, result.Message ?? string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

       
        // ─── POSTAL RECEIVE ─────────────────────────────────────
        /// <summary>
        /// Retrieves all postal receive records for the specified company and session.
        /// </summary>
        /// <param name="companyId">Company identifier.</param>
        /// <param name="sessionId">Session identifier.</param>
        /// <param name="includeDeleted">Indicates whether deleted records should be included.</param>
        /// <returns>List of postal receive details.</returns>
        public List<FOPostalReceiveViewModel> GetAllPostalReceives(int companyId, int sessionId, bool includeDeleted = false)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.Query<FOPostalReceiveViewModel>(
                "sp_FO_PostalReceive_GetAll",
                new
                {
                    CompanyID = companyId,
                    SessionID = sessionId,
                    IncludeDeleted = includeDeleted
                },
                commandType: CommandType.StoredProcedure)
                .ToList();
        }
        /// <summary>
        /// Retrieves a postal receive record by its unique identifier.
        /// </summary>
        /// <param name="id">Postal receive ID.</param>
        /// <returns>
        /// Returns the postal receive details if found; otherwise, null.
        /// </returns>
        public FOPostalReceiveViewModel? GetPostalReceiveByID(int id)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.QueryFirstOrDefault<FOPostalReceiveViewModel>(
                "sp_FO_PostalReceive_GetByID",
                new { PostalReceiveID = id },
                commandType: CommandType.StoredProcedure);
        }
        /// <summary>
        /// Inserts or updates a postal receive record and returns the operation result.
        /// </summary>
        /// <param name="req">Postal receive details.</param>
        /// <param name="companyId">Company identifier.</param>
        /// <param name="sessionId">Session identifier.</param>
        /// <param name="userId">Current user identifier.</param>
        /// <returns>
        /// Returns a tuple containing operation status and message.
        /// </returns>
        public (bool, string) UpsertPostalReceive(
            FOPostalReceiveUpsertRequest req,
            int companyId,
            int sessionId,
            int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<dynamic>(
                    "sp_FO_PostalReceive_Upsert",
                    new
                    {
                        req.PostalReceiveID,
                        CompanyID = companyId,
                        SessionID = sessionId,
                        req.FromTitle,
                        req.ToTitle,
                        req.ReferenceNo,
                        req.Address,
                        req.Note,
                        req.Date,
                        req.Attachment,
                        req.FileName,
                        req.FileType,
                        req.IsActive,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return (
                    Convert.ToInt32(result?.Result) == 1,
                    Convert.ToString(result?.Message) ?? string.Empty
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        /// <summary>
        /// Deletes a postal receive record from the system.
        /// </summary>
        public (bool, string) DeletePostalReceive(List<int> id, int userId)
        {
            try
            {
                if (id == null || !id.Any())
                {
                    return (false, "No students selected for deletion.");
                }

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string postalReceiveIDs = string.Join(",", id);

                var result = conn.QueryFirstOrDefault(
                    "sp_FO_PostalReceive_Delete",
                    new
                    {
                        PostalReceiveID = postalReceiveIDs,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure);

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
        /// Updates whether a postal receive record is active or inactive.
        /// </summary>
        public (bool, string) TogglePostalReceiveStatus(int id, bool isActive, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault(
                    "sp_FO_PostalReceive_ToggleStatus",
                    new
                    {
                        PostalReceiveID = id,
                        IsActive = isActive,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure);

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
       
        // ─── POSTAL DISPATCH ────────────────────────────────────
        /// <summary>
        /// Retrieves all postal dispatch records for the specified company and session.
        /// </summary>
        /// <param name="companyId">Company ID.</param>
        /// <param name="sessionId">Session ID.</param>
        /// <param name="includeDeleted">Whether deleted records should be included.</param>
        /// <returns>List of postal dispatch records.</returns>
        public List<FOPostalDispatchViewModel> GetAllPostalDispatches(
            int companyId,
            int sessionId,
            bool includeDeleted = false)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var parameters = new
            {
                CompanyID = companyId,
                SessionID = sessionId,
                IncludeDeleted = includeDeleted
            };

            return conn.Query<FOPostalDispatchViewModel>(
                "sp_FO_PostalDispatch_GetAll",
                parameters,
                commandType: CommandType.StoredProcedure)
                .ToList();
        }
        /// <summary>
        /// Retrieves postal dispatch details by PostalDispatchID.
        /// </summary>
        /// <param name="id">Postal Dispatch ID.</param>
        /// <returns>
        /// Returns the postal dispatch details if found; otherwise null.
        /// </returns>
        public FOPostalDispatchViewModel? GetPostalDispatchByID(int id)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.QueryFirstOrDefault<FOPostalDispatchViewModel>(
                "sp_FO_PostalDispatch_GetByID",
                new { PostalDispatchID = id },
                commandType: CommandType.StoredProcedure);
        }
        /// <summary>
        /// Inserts or updates postal dispatch details.
        /// Calls sp_FO_PostalDispatch_Upsert and returns operation status with message.
        /// </summary>
        /// <param name="req">Postal dispatch request details.</param>
        /// <param name="companyId">Company ID.</param>
        /// <param name="sessionId">Session ID.</param>
        /// <param name="userId">Current user ID.</param>
        /// <returns>
        /// Item1 = Success status.
        /// Item2 = Result message.
        /// </returns>
        public (bool, string) UpsertPostalDispatch(
            FOPostalDispatchUpsertRequest req,
            int companyId,
            int sessionId,
            int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<(int Result, string Message)>(
                    "sp_FO_PostalDispatch_Upsert",
                    new
                    {
                        req.PostalDispatchID,
                        CompanyID = companyId,
                        SessionID = sessionId,
                        req.ToTitle,
                        req.FromTitle,
                        req.ReferenceNo,
                        req.Address,
                        req.Note,
                        req.Date,
                        req.Attachment,
                        req.FileName,
                        req.FileType,
                        req.IsActive,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return (result.Result == 1, result.Message ?? string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        /// <summary>
        /// Deletes a postal dispatch record.
        /// </summary>
        /// <param name="id">Postal Dispatch ID.</param>
        /// <param name="userId">User ID performing the delete operation.</param>
        /// <returns>
        /// Item1 = Success status.
        /// Item2 = Result message.
        /// </returns>
        public (bool, string) DeletePostalDispatch(List<int> id, int userId)
        {
            try
            {
                if (id == null || !id.Any())
                {
                    return (false, "No students selected for deletion.");
                }

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string postalDispatchIDs = string.Join(",", id);

                var result = conn.QueryFirstOrDefault<(int Result, string Message)>(
                    "sp_FO_PostalDispatch_Delete",
                    new
                    {
                        PostalDispatchID = postalDispatchIDs,
                        UserId = userId
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
        /// Toggles the active/inactive status of a postal dispatch record.
        /// </summary>
        /// <param name="id">Postal Dispatch ID.</param>
        /// <param name="isActive">Status to set.</param>
        /// <param name="userId">User performing the action.</param>
        /// <returns>
        /// Item1 = Success status.
        /// Item2 = Result message.
        /// </returns>
        public (bool, string) TogglePostalDispatchStatus(int id, bool isActive, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<(int Result, string Message)>(
                    "sp_FO_PostalDispatch_ToggleStatus",
                    new
                    {
                        PostalDispatchID = id,
                        IsActive = isActive,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return (result.Result == 1, result.Message);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
       
        // ─── PHONE CALL LOG ─────────────────────────────────────

        /// <summary>
        /// Retrieves all phone call logs for the specified company and session.
        /// </summary>
        /// <param name="companyId">Company ID.</param>
        /// <param name="sessionId">Session ID.</param>
        /// <param name="includeDeleted">Include deleted records.</param>
        /// <returns>List of phone call logs.</returns>
        public List<FOPhoneCallLogViewModel> GetAllPhoneCallLogs(int companyId, int sessionId, bool includeDeleted = false)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return conn.Query<FOPhoneCallLogViewModel>(
                "sp_FO_PhoneCallLog_GetAll",
                new
                {
                    CompanyID = companyId,
                    SessionID = sessionId,
                    IncludeDeleted = includeDeleted
                },
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        /// <summary>
        /// Retrieves a phone call log by its ID.
        /// </summary>
        /// <param name="id">Phone call log ID.</param>
        /// <returns>Phone call log details if found; otherwise null.</returns>
        public FOPhoneCallLogViewModel? GetPhoneCallLogByID(int id)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return conn.QueryFirstOrDefault<FOPhoneCallLogViewModel>(
                "sp_FO_PhoneCallLog_GetByID",
                new
                {
                    PhoneCallLogID = id
                },
                commandType: CommandType.StoredProcedure
            );
        }

        /// <summary>
        /// Creates or updates a phone call log.
        /// </summary>
        /// <param name="req">Phone call log request.</param>
        /// <param name="companyId">Company ID.</param>
        /// <param name="sessionId">Session ID.</param>
        /// <param name="userId">User ID.</param>
        /// <returns>Success status and message.</returns>
        public (bool, string) UpsertPhoneCallLog(FOPhoneCallLogUpsertRequest req, int companyId, int sessionId, int userId)
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault(
                    "sp_FO_PhoneCallLog_Upsert",
                    new
                    {
                        req.PhoneCallLogID,
                        CompanyID = companyId,
                        SessionID = sessionId,
                        req.Name,
                        req.Phone,
                        req.Date,
                        req.Description,
                        req.NextFollowUpDate,
                        req.CallDuration,
                        req.Note,
                        req.CallType,
                        req.IsActive,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure
                );

                return ((int)result.Result == 1, (string)result.Message);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Deletes a phone call log.
        /// </summary>
        /// <param name="id">Phone call log ID.</param>
        /// <param name="userId">User ID.</param>
        /// <returns>Success status and message.</returns>
        public (bool, string) DeletePhoneCallLog(List<int> id, int userId)
        {
            try
            {
                if (id == null || !id.Any())
                {
                    return (false, "No students selected for deletion.");
                }

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string phoneCallLogIDs = string.Join(",", id);

                var result = conn.QueryFirstOrDefault(
                    "sp_FO_PhoneCallLog_Delete",
                    new
                    {
                        PhoneCallLogID = phoneCallLogIDs,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure
                );

                return ((int)result.Result == 1, (string)result.Message);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Activates or deactivates a phone call log.
        /// </summary>
        /// <param name="id">Phone call log ID.</param>
        /// <param name="isActive">Status value.</param>
        /// <param name="userId">User ID.</param>
        /// <returns>Success status and message.</returns>
        public (bool success, string message) TogglePhoneCallLogStatus(int id, bool isActive, int userId)
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault(
                    "sp_FO_PhoneCallLog_ToggleStatus",
                    new
                    {
                        PhoneCallLogID = id,
                        IsActive = isActive,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure
                );

                return ((int)result.Result == 1, (string)result.Message);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        
        // ─── VISITOR BOOK ───────────────────────────────────────
        /// <summary>
        /// Retrieves all visitors for the specified company and session.
        /// </summary>
        public List<FOVisitorBookViewModel> GetAllVisitors(int companyId, int sessionId, bool includeDeleted = false)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return conn.Query<FOVisitorBookViewModel>(
                "sp_FO_VisitorBook_GetAll",
                new
                {
                    CompanyID = companyId,
                    SessionID = sessionId,
                    IncludeDeleted = includeDeleted
                },
                commandType: CommandType.StoredProcedure
            ).ToList();
        }
        /// <summary>
        /// Retrieves visitor details by visitor ID.
        /// </summary>
        public FOVisitorBookViewModel? GetVisitorByID(int id)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            return conn.QueryFirstOrDefault<FOVisitorBookViewModel>(
                "sp_FO_VisitorBook_GetByID",
                new
                {
                    VisitorBookID = id
                },
                commandType: CommandType.StoredProcedure
            );
        }
        /// <summary>
        /// Creates or updates visitor information.
        /// </summary>
        public (bool success, string message) UpsertVisitor(
            FOVisitorBookUpsertRequest req,
            int companyId,
            int sessionId,
            int userId)
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<dynamic>(
                    "sp_FO_VisitorBook_Upsert",
                    new
                    {
                        req.VisitorBookID,
                        CompanyID = companyId,
                        SessionID = sessionId,
                        req.PurposeID,
                        req.Name,
                        req.Phone,
                        req.IDCard,
                        req.NoOfPersons,
                        req.Date,
                        req.InTime,
                        req.OutTime,
                        req.Note,
                        req.Attachment,
                        req.FileName,
                        req.FileType,
                        req.IsActive,
                        req.MeetingWith,
                        req.StudentID,
                        req.StaffID,
                        UserId = userId
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
        /// Deletes a visitor record.
        /// </summary>
        public (bool success, string message) DeleteVisitor(List<int> id, int userId)
        {
            try
            {
                if (id == null || !id.Any())
                {
                    return (false, "No students selected for deletion.");
                }

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string visitorBookIDs = string.Join(",", id);

                var result = conn.QueryFirstOrDefault<dynamic>(
                    "sp_FO_VisitorBook_Delete",
                    new
                    {
                        VisitorBookID = visitorBookIDs,
                        UserId = userId
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
        /// Activates or deactivates a visitor record.
        /// </summary>
        public (bool success, string message) ToggleVisitorStatus(int id, bool isActive, int userId)
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<dynamic>(
                    "sp_FO_VisitorBook_ToggleStatus",
                    new
                    {
                        VisitorBookID = id,
                        IsActive = isActive,
                        UserId = userId
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
        
        // --- Admission Inquiry ---

        /// <summary>
        /// Retrieves all admission inquiries based on filter criteria.
        /// </summary>
        /// <param name="companyId">Company identifier.</param>
        /// <param name="sessionId">Session identifier.</param>
        /// <param name="fromDate">Optional from date filter.</param>
        /// <param name="toDate">Optional to date filter.</param>
        /// <param name="sourceId">Source identifier.</param>
        /// <param name="classId">Class identifier.</param>
        /// <param name="status">Inquiry status.</param>
        /// <returns>List of admission inquiries.</returns>
        public List<FOAdmissionInquiryViewModel> GetAllAdmissionInquiries(
            int companyId,
            int sessionId,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int sourceId = 0,
            int classId = 0,
            string? status = null)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.Query<FOAdmissionInquiryViewModel>(
                "SP_FO_ADMISSION_INQUIRY_GETALL",
                new
                {
                    CompanyID = companyId,
                    SessionID = sessionId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    SourceID = sourceId,
                    ClassID = classId,
                    Status = status
                },
                commandType: CommandType.StoredProcedure)
                .ToList();
        }
        /// <summary>
        /// Retrieves admission inquiry details by inquiry ID.
        /// </summary>
        /// <param name="id">Inquiry identifier.</param>
        /// <returns>Admission inquiry details with follow-up history.</returns>
        public FOAdmissionInquiryViewModel? GetAdmissionInquiryByID(int id)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            using var multi = conn.QueryMultiple(
                "SP_FO_ADMISSION_INQUIRY_GETBYID",
                new { InquiryID = id },
                commandType: CommandType.StoredProcedure);

            var model = multi.Read<FOAdmissionInquiryViewModel>()
                             .FirstOrDefault();

            if (model != null)
            {
                model.FollowUps = multi.Read<FOInquiryFollowUpViewModel>()
                                       .ToList();
            }

            return model;
        }
        /// <summary>
        /// Inserts or updates an admission inquiry.
        /// </summary>
        /// <param name="req">Admission inquiry details.</param>
        /// <param name="companyId">Company identifier.</param>
        /// <param name="sessionId">Session identifier.</param>
        /// <param name="userId">Current user identifier.</param>
        /// <returns>Operation status and message.</returns>
        public (bool success, string message) UpsertAdmissionInquiry(
            FOAdmissionInquiryUpsertRequest req,
            int companyId,
            int sessionId,
            int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<dynamic>(
                    "SP_FO_ADMISSION_INQUIRY_UPSERT",
                    new
                    {
                        req.InquiryID,
                        CompanyID = companyId,
                        SessionID = sessionId,
                        req.Name,
                        req.Phone,
                        req.Email,
                        req.Address,
                        req.Description,
                        req.Note,
                        req.Date,
                        req.NextFollowUpDate,
                        req.AssignedTo,
                        req.ReferenceID,
                        req.SourceID,
                        req.ClassID,
                        req.NoOfChild,
                        req.Status,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure);

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
        /// Deletes an admission inquiry.
        /// </summary>
        /// <param name="id">Inquiry identifier.</param>
        /// <param name="userId">Current user identifier.</param>
        /// <returns>Operation status and message.</returns>
        public (bool success, string message) DeleteAdmissionInquiry(List<int> id, int userId)
        {
            try
            {
                if (id == null || !id.Any())
                {
                    return (false, "No students selected for deletion.");
                }

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string inquiryIDs = string.Join(",", id);

                var result = conn.QueryFirstOrDefault<dynamic>(
                    "SP_FO_ADMISSION_INQUIRY_DELETE",
                    new
                    {
                        InquiryID = inquiryIDs,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure);

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
        /// Saves inquiry follow-up details.
        /// </summary>
        /// <param name="req">Follow-up information.</param>
        /// <param name="userId">Current user identifier.</param>
        /// <returns>Operation status and message.</returns>
        public (bool success, string message) SaveInquiryFollowUp(
            FOInquiryFollowUpSaveRequest req,
            int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<dynamic>(
                    "SP_FO_INQUIRY_FOLLOWUP_SAVE",
                    new
                    {
                        req.InquiryID,
                        req.FollowUpDate,
                        req.NextFollowUpDate,
                        req.Response,
                        req.Note,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure);

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
