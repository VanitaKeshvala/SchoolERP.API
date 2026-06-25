using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This service handles the actual work of sending requests to the main system to manage user accounts in the database via an API.
    /// </summary>
    public class UserClientService : BaseApiClient, IUserClientService
    {
        public UserClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        /// <summary>
        /// Sends a request to the server to get a list of all users.
        /// </summary>
        public async Task<ApiResponse<List<UserViewModel>>> GetAllUsersAsync()
        {
            return await GetAsync<List<UserViewModel>>("api/user");
        }
        public async Task<ApiResponse<PagedResult<UserViewModel>>> GetAllUsersWithPaginationAsync(UserSearchRequest request)
        {
            return await PostAsync<PagedResult<UserViewModel>>("api/user", request);
        }
        /// <summary>
        /// Sends a request to the server to look up details for a specific user by its ID.
        /// </summary>
        public async Task<ApiResponse<UserViewModel>> GetUserByIdAsync(int id)
        {
            return await GetAsync<UserViewModel>($"api/user/{id}");
        }

        /// <summary>
        /// Sends a request to the server to get the list of roles a user belongs to.
        /// </summary>
        public async Task<ApiResponse<List<int>>> GetUserRoleIdsAsync(int id)
        {
            return await GetAsync<List<int>>($"api/user/{id}/roles");
        }

        /// <summary>
        /// Sends a request to the server to get active roles for selection menus.
        /// </summary>
        public async Task<ApiResponse<List<RoleViewModel>>> GetRolesDropdownAsync()
        {
            return await GetAsync<List<RoleViewModel>>("api/user/roles-dropdown");
        }

        /// <summary>
        /// Sends a request to the server to get active user types for selection menus.
        /// </summary>
        public async Task<ApiResponse<List<MstUserTypeViewModel>>> GetUserTypesDropdownAsync()
        {
            return await GetAsync<List<MstUserTypeViewModel>>("api/user/types-dropdown");
        }

        /// <summary>
        /// Sends user details to the server to be saved or updated.
        /// </summary>
        public async Task<ApiResponse<bool>> SaveUserAsync(UserUpsertRequest request)
        {
            return await PostAsync<bool>("api/user/save", request);
        }

        /// <summary>
        /// Sends a request to the server to update whether a user is currently allowed to log in.
        /// </summary>
        public async Task<ApiResponse<bool>> ToggleStatusAsync(int userId, bool isActive)
        {
            return await PostAsync<bool>($"api/user/toggle-status?userId={userId}&isActive={isActive}", null);
        }

        /// <summary>
        /// Sends a request to the server to unlock a user's account.
        /// </summary>
        public async Task<ApiResponse<bool>> UnlockUserAsync(int id)
        {
            return await PostAsync<bool>($"api/user/unlock/{id}", null);
        }

        /// <summary>
        /// Sends a request to the server to delete a user record.
        /// </summary>
        public async Task<ApiResponse<bool>> DeleteUserAsync(int id)
        {
            return await PostAsync<bool>($"api/user/delete/{id}", null);
        }

        public async Task<ApiResponse<bool>> IsUsernameUniqueAsync(string username, int userId = 0)
        {
            return await GetAsync<bool>($"api/user/check-username?username={System.Uri.EscapeDataString(username)}&userId={userId}");
        }

        public async Task<ApiResponse<UserWizardViewModel>> GetUserWizardDataAsync(int userId = 0, string roleIds = "")
        {
            return await GetAsync<UserWizardViewModel>($"api/user/wizard-data?userId={userId}&roleIds={roleIds}");
        }

        public async Task<ApiResponse<bool>> SaveUserWizardAsync(UserUpsertRequest request)
        {
            return await PostAsync<bool>("api/user/save-wizard", request);
        }

        public async Task<ApiResponse<bool>> BulkToggleStatusAsync(UserStatusUpdateRequest request)
        {
            return await PostAsync<bool>("api/user/bulk-toggle-status", request);
        }

        public async Task<ApiResponse<bool>> DeleteBulkUserAsync(string ids)
        {
            return await PostAsync<bool>($"api/user/delete-bulk?ids={Uri.EscapeDataString(ids)}",null!);
        }
    }
}
