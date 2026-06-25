using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This interface defines the rules for how the application communicates with the main system to manage user roles and permissions.
    /// </summary>
    public interface IRoleClientService
    {
        /// <summary>
        /// Asks the main system for a list of all defined roles.
        /// </summary>
        Task<ApiResponse<List<MstRoleViewModel>>> GetAllRolesAsync();

        /// <summary>
        /// Asks the main system for details about a specific role using its ID.
        /// </summary>
        Task<ApiResponse<MstRoleViewModel>> GetRoleByIdAsync(int id);

        /// <summary>
        /// Sends information to the main system to save or update a role.
        /// </summary>
        Task<ApiResponse<int>> SaveRoleAsync(MstRoleUpsertRequest request);

        /// <summary>
        /// Tells the main system to turn a role on or off.
        /// </summary>
        Task<ApiResponse<bool>> ToggleStatusAsync(int roleId, bool isActive);

        /// <summary>
        /// Asks the main system for the list of actions allowed for a specific role.
        /// </summary>
        Task<ApiResponse<List<RoleMenuPermissionViewModel>>> GetPermissionsAsync(int roleId);

        /// <summary>
        /// Sends the chosen list of allowed actions for a role to the main system to be saved.
        /// </summary>
        Task<ApiResponse<bool>> SavePermissionsAsync(MstRolePermissionSaveRequest request);

        /// <summary>
        /// Tells the main system to remove a specific role.
        /// </summary>
        Task<ApiResponse<bool>> DeleteRoleAsync(List<int> roleId);
    }
}
