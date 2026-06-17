using Dapper;
using Microsoft.Data.SqlClient;
using SchoolERP.API.Data;
using SchoolERP.API.Interfaces;
using SchoolERP.API.Models;
using SchoolERP.API.Models.Common;
using System.Data;

namespace SchoolERP.API.Services
{
    /// <summary>
    /// Service responsible for retrieving user permission information
    /// from the database using Dapper and stored procedures.
    /// </summary>
    public class UserManagementService: IUserManagementService
    {
        private readonly SqlHelper _sqlHelper;
        private readonly IConfiguration _configuration;
        public UserManagementService(SqlHelper sqlHelper, IConfiguration configuration)
        {
            _sqlHelper = sqlHelper;
            _configuration = configuration;
        }


        /// <summary>
        /// Retrieves all permissions assigned to the specified user.
        /// </summary>
        /// <param name="userId">
        /// The unique identifier of the user.
        /// </param>
        /// <returns>
        /// A list of user permissions associated with the specified user.
        /// </returns>
        public async Task<List<UserPermissionViewModel>> GetUserPermissionsAsync(int userId)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var list = await conn.QueryAsync<UserPermissionViewModel>(
                "sp_User_GetPermissions",
                new { UserID = userId },
                commandType: CommandType.StoredProcedure);
            return list.ToList();
        }
        /// <summary>
        /// Retrieves user permissions with optional filtering by
        /// menu URL prefix and permission name.
        /// </summary>
        /// <returns>
        /// A filtered list of user permissions.
        /// </returns>
        public async Task<List<UserPermissionViewModel>> GetUserPermissions(int userId, string? menuUrlPrefix=null, string? permissionName = null)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var param = new DynamicParameters();

            param.Add("@USERID", userId);
            param.Add("@PermissionName", permissionName);
            param.Add("@MenuKey", menuUrlPrefix);

            var result = await conn.QueryAsync<UserPermissionViewModel>(
            "sp_User_GetPermissions",
            param,
            commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        /// <summary>
        /// Retrieves a list of all user types from the database.
        /// </summary>
        /// <returns>A list of <see cref="MstUserTypeViewModel"/> containing user type details.</returns>
        public List<MstUserTypeViewModel> GetAllUserTypes()
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.Query<MstUserTypeViewModel>(
                "sp_UserTypes_GetAll",
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        /// <summary>
        /// Looks up a specific user category's details from the database.
        /// </summary>
        /// <param name="userTypeID">The unique identifier of the user type.</param>
        /// <returns>
        /// A <see cref="MstUserTypeViewModel"/> containing the user type details if found; otherwise, <c>null</c>.
        /// </returns>
        public MstUserTypeViewModel? GetUserTypeByID(int userTypeID)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.QueryFirstOrDefault<MstUserTypeViewModel>(
                "sp_UserTypes_GetByID",
                new { UserTypeID = userTypeID },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Saves a new user type or updates an existing user type in the database.
        /// Executes the <c>sp_UserTypes_Upsert</c> stored procedure and returns the operation result.
        /// </summary>
        /// <param name="request">The user type details to insert or update.</param>
        /// <param name="userId">The ID of the user performing the operation.</param>
        /// <param name="ipAddress">The IP address of the user performing the operation.</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        /// <item><description><c>success</c> - Indicates whether the operation was successful.</description></item>
        /// <item><description><c>message</c> - The result message returned by the stored procedure.</description></item>
        /// </list>
        /// </returns>
        public (bool success, string message) UpsertUserType(MstUserTypeUpsertRequest request, int userId, string ipAddress)
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<(int Result, string Message)>(
                    "sp_UserTypes_Upsert",
                    new
                    {
                        request.UserTypeID,
                        request.TypeCode,
                        request.TypeName,
                        request.IsActive,
                        UserID = userId,
                        IPAddress = ipAddress
                    },
                    commandType: CommandType.StoredProcedure);

                return (result.Result == 1, result.Message ?? "");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Updates whether a user category is active or inactive.
        /// </summary>
        /// <param name="userTypeID">The unique identifier of the user type.</param>
        /// <param name="isActive">The status to set (true = active, false = inactive).</param>
        /// <param name="userId">The ID of the user performing the action.</param>
        /// <param name="ipAddress">The IP address from which the request originated.</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        /// <item><description><c>success</c> - Indicates whether the operation was successful.</description></item>
        /// <item><description><c>message</c> - The result message returned by the database.</description></item>
        /// </list>
        /// </returns>
        public (bool success, string message) ToggleUserTypeStatus(int userTypeID, bool isActive, int userId, string ipAddress)
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault(
                    "sp_UserTypes_ToggleStatus",
                    new
                    {
                        UserTypeID = userTypeID,
                        IsActive = isActive,
                        UserID = userId,
                        IPAddress = ipAddress
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

        /// <summary>
        /// Retrieves a list of all roles from the database.
        /// </summary>
        /// <returns>A collection of role details.</returns>
        public List<MstRoleViewModel> GetAllRoles()
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                return conn.Query<MstRoleViewModel>(
                    "sp_Roles_GetAll",
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Looks up a specific role's details from the database.
        /// </summary>
        /// <param name="roleID">The unique identifier of the role.</param>
        /// <returns>
        /// A <see cref="MstRoleViewModel"/> containing the role details if found; otherwise, <c>null</c>.
        /// </returns>
        public MstRoleViewModel? GetRoleByID(int roleID)
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                return conn.QueryFirstOrDefault<MstRoleViewModel>(
                    "sp_Roles_GetByID",
                    new
                    {
                        RoleID = roleID
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Saves or updates a role in the database.
        /// </summary>
        public async Task<(bool success, string message, int roleId)> UpsertRole(MstRoleUpsertRequest request, int userId, string ipAddress)
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                var result = await conn.QueryFirstOrDefaultAsync<RoleUpsertResult>(
                    "sp_Roles_Upsert",
                    new
                    {
                        request.RoleID,
                        request.RoleName,
                        request.RoleDesc,
                        request.IsActive,
                        UserID = userId,
                        IPAddress = ipAddress
                    },
                    commandType: CommandType.StoredProcedure);

                if (result == null)
                    return (false, "No responsive mapping detected from database trace", 0);

                bool ok = Convert.ToInt32(result.Result) == 1;
                string msg = Convert.ToString(result.Message) ?? string.Empty;

                int savedRoleId = 0;
                try
                {
                    savedRoleId = result.RoleID != null
                        ? Convert.ToInt32(result.RoleID)
                        : (request.RoleID > 0 ? request.RoleID : 0);
                }
                catch
                {
                    savedRoleId = request.RoleID > 0 ? request.RoleID : 0;
                }

                return (ok, msg, savedRoleId);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, 0);
            }
        }

        /// <summary>
        /// Updates the active/inactive status of a role.
        /// </summary>
        /// <param name="roleID">The unique identifier of the role.</param>
        /// <param name="isActive">The status to set for the role.</param>
        /// <param name="userId">The identifier of the user performing the action.</param>
        /// <param name="ipAddress">The IP address of the user performing the action.</param>
        /// <returns>
        /// A tuple containing:
        /// - success: Indicates whether the operation was successful.
        /// - message: The response message returned by the stored procedure.
        /// </returns>
        public (bool success, string message) ToggleRoleStatus(int roleID, bool isActive, int userId, string ipAddress)
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Roles_ToggleStatus",
                    new
                    {
                        RoleID = roleID,
                        IsActive = isActive,
                        UserID = userId,
                        IPAddress = ipAddress
                    },
                    commandType: CommandType.StoredProcedure);

                if (result == null)
                    return (false, "No responsive mapping detected from database trace");

                return (result.Result == 1, result.Message ?? string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Fetches the full list of allowed actions for a role from the database.
        /// </summary>
        public List<RoleMenuPermissionViewModel> GetPermissionsMatrix(int roleId)
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var list = conn.Query<RoleMenuPermissionViewModel>(
                    "sp_Roles_GetPermissionsMatrix",
                    new { RoleID = roleId },
                    commandType: CommandType.StoredProcedure)
                    .ToList();

                return list ?? new List<RoleMenuPermissionViewModel>();
            }
            catch
            {
                return new List<RoleMenuPermissionViewModel>();
            }
        }

        /// <summary>
        /// Saves the mapping of actions to roles in the database.
        /// </summary>
        public (bool success, string message) SaveRolePermissions(MstRolePermissionSaveRequest request, int adminId, string ipAddress)
        {
            try
            {
                string pairs = string.Join(",",
                    request.SelectedPermissions.Select(p => $"{p.MenuID}:{p.PermissionID}"));

                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Roles_SavePermissions",
                    new
                    {
                        request.RoleID,
                        MenuPermissionPairs = pairs,
                        UserId = adminId,
                        IPAddress = ipAddress
                    },
                    commandType: CommandType.StoredProcedure);

                if (result == null)
                    return (false, "No response from database");

                return (result.Result == 1, result.Message ?? string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Deletes a role from the database.
        /// Executes the sp_Roles_Delete stored procedure and returns the operation result.
        /// </summary>
        /// <param name="roleID">The unique identifier of the role to delete.</param>
        /// <param name="userId">The ID of the user performing the delete operation.</param>
        /// <param name="ipAddress">The IP address of the user performing the operation.</param>
        /// <returns>
        /// A tuple containing:
        /// - success: Indicates whether the delete operation was successful.
        /// - message: The result message returned by the stored procedure.
        /// </returns>
        public (bool success, string message) DeleteRole(List<int> roleID, int userId, string ipAddress)
        {
            try
            {
                if (roleID == null || !roleID.Any())
                {
                    return (false, "No students selected for deletion.");
                }

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string roleIDs = string.Join(",", roleID);

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Roles_Delete",
                    new
                    {
                        RoleID = roleIDs,
                        UserID = userId,
                        IPAddress = ipAddress
                    },
                    commandType: CommandType.StoredProcedure);

                if (result == null)
                    return (false, "No responsive mapping detected from database trace");

                return (result.Result == 1, result.Message ?? string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }



        /// <summary>
        /// Retrieves a list of all possible actions/permissions from the database.
        /// </summary>
        public List<MstPermissionViewModel> GetAllPermissions()
        {
            var list = new List<MstPermissionViewModel>();
            var dt = _sqlHelper.ExecuteQuery("sp_Permissions_GetAll", null!);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new MstPermissionViewModel
                {
                    PermissionID = Convert.ToInt32(row["PermissionID"]),
                    PermissionName = row["PermissionName"].ToString() ?? "",
                    DisplayLabel = row["DisplayLabel"]?.ToString(),
                    IsActive = Convert.ToBoolean(row["IsActive"]),
                    CreatedOn = Convert.ToDateTime(row["CreatedOn"])
                });
            }
            return list;
        }

        /// <summary>
        /// Looks up a specific permission's details from the database.
        /// </summary>
        public MstPermissionViewModel? GetPermissionByID(int permissionID)
        {
            var dt = _sqlHelper.ExecuteQuery("sp_Permissions_GetByID", new[] { new SqlParameter("@PermissionID", permissionID) });
            if (dt.Rows.Count == 0) return null;
            var row = dt.Rows[0];
            return new MstPermissionViewModel
            {
                PermissionID = Convert.ToInt32(row["PermissionID"]),
                PermissionName = row["PermissionName"].ToString() ?? "",
                DisplayLabel = row["DisplayLabel"]?.ToString(),
                IsActive = Convert.ToBoolean(row["IsActive"]),
                CreatedOn = Convert.ToDateTime(row["CreatedOn"])
            };
        }

        /// <summary>
        /// Saves or updates a permission in the database.
        /// </summary>
        public (bool success, string message) UpsertPermission(MstPermissionUpsertRequest request, int userId, string ipAddress)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@PermissionID", request.PermissionID),
                    new SqlParameter("@PermissionName", request.PermissionName),
                    new SqlParameter("@DisplayLabel", (object?)request.DisplayLabel ?? DBNull.Value),
                    new SqlParameter("@IsActive", request.IsActive),
                    new SqlParameter("@UserID", userId),
                    new SqlParameter("@IPAddress", ipAddress)
                };
                var dt = _sqlHelper.ExecuteQuery("sp_Permissions_Upsert", parameters);
                return (Convert.ToInt32(dt.Rows[0]["Result"]) == 1, dt.Rows[0]["Message"].ToString() ?? "");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        /// <summary>
        /// Updates whether a permission is currently usable.
        /// </summary>
        public (bool success, string message) TogglePermissionStatus(int permissionID, bool isActive, int userId, string ipAddress)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@PermissionID", permissionID),
                    new SqlParameter("@IsActive", isActive),
                    new SqlParameter("@UserID", userId),
                    new SqlParameter("@IPAddress", ipAddress)
                };
                var dt = _sqlHelper.ExecuteQuery("sp_Permissions_ToggleStatus", parameters);
                return (Convert.ToInt32(dt.Rows[0]["Result"]) == 1, dt.Rows[0]["Message"].ToString() ?? "");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        /// <summary>
        /// Deletes a permission from the database.
        /// </summary>
        public (bool success, string message) DeletePermission(List<int> permissionID, int userId, string ipAddress)
        {
            try
            {
                if (permissionID == null || !permissionID.Any())
                {
                    return (false, "No students selected for deletion.");
                }
                string permissionIDs = string.Join(",", permissionID);
                var parameters = new[]
                {
                    new SqlParameter("@PermissionID", permissionIDs),
                    new SqlParameter("@UserID", userId),
                    new SqlParameter("@IPAddress", ipAddress)
                };
                var dt = _sqlHelper.ExecuteQuery("sp_Permissions_Delete", parameters);
                return (Convert.ToInt32(dt.Rows[0]["Result"]) == 1, dt.Rows[0]["Message"].ToString() ?? "");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

    }
}
