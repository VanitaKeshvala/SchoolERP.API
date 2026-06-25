using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This service handles the actual work of sending requests to the main system to manage roles and permissions in the database via an API.
    /// </summary>
    public class RoleClientService : BaseApiClient, IRoleClientService
    {
        public RoleClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        /// <summary>
        /// Sends a request to the server to get a list of all roles.
        /// </summary>
        public async Task<ApiResponse<List<MstRoleViewModel>>> GetAllRolesAsync()
        {
            return await GetAsync<List<MstRoleViewModel>>("api/RoleApi");
        }

        /// <summary>
        /// Sends a request to the server to look up details for a specific role by its ID.
        /// </summary>
        public async Task<ApiResponse<MstRoleViewModel>> GetRoleByIdAsync(int id)
        {
            return await GetAsync<MstRoleViewModel>($"api/RoleApi/{id}");
        }

        /// <summary>
        /// Sends role details to the server to be saved or updated.
        /// </summary>
        public async Task<ApiResponse<int>> SaveRoleAsync(MstRoleUpsertRequest request)
        {
            return await PostAsync<int>("api/RoleApi/save", request);
        }

        /// <summary>
        /// Sends a request to the server to update whether a role is active.
        /// </summary>
        public async Task<ApiResponse<bool>> ToggleStatusAsync(int roleId, bool isActive)
        {
            return await PostAsync<bool>($"api/RoleApi/toggle-status?roleId={roleId}&isActive={isActive}", null);
        }

        /// <summary>
        /// Sends a request to the server to fetch the permission matrix for a specific role.
        /// </summary>
        public async Task<ApiResponse<List<RoleMenuPermissionViewModel>>> GetPermissionsAsync(int roleId)
        {
            return await GetAsync<List<RoleMenuPermissionViewModel>>($"api/RoleApi/{roleId}/permissions");
        }

        /// <summary>
        /// Sends a request to the server to save the permission settings for a role.
        /// </summary>
        public async Task<ApiResponse<bool>> SavePermissionsAsync(MstRolePermissionSaveRequest request)
        {
            return await PostAsync<bool>("api/RoleApi/save-permissions", request);
        }

        /// <summary>
        /// Sends a request to the server to delete a role record.
        /// </summary>
        public async Task<ApiResponse<bool>> DeleteRoleAsync(List<int> roleId)
        {
            return await PostAsync<bool>($"api/RoleApi",roleId);
        }
    }
}
