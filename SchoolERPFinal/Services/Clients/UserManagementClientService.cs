using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class UserManagementClientService: BaseApiClient, IUserManagementClientService
    {
        public UserManagementClientService(HttpClient httpClient) : base(httpClient) { }

        public async Task<ApiResponse<List<UserPermissionViewModel>>> GetUserPermissions(int userId)
        {
            return await GetAsync<List<UserPermissionViewModel>>($"api/UserPermissions/GetUserPermissions/{userId}");
        }

        /// <summary>
        /// Retrieves all permissions.
        /// </summary>
        /// <returns>List of permissions.</returns>
        public Task<ApiResponse<List<MstPermissionViewModel>>> GetAllPermissionsAsync()
        {
            return GetAsync<List<MstPermissionViewModel>>(
                "api/Permission/GetAllPermissions");
        }

        /// <summary>
        /// Retrieves the details of a specific permission.
        /// </summary>
        /// <param name="permissionID">Permission identifier.</param>
        /// <returns>Permission details.</returns>
        public Task<ApiResponse<MstPermissionViewModel>> GetPermissionByIDAsync(
            int permissionID)
        {
            return GetAsync<MstPermissionViewModel>(
                $"api/UserManagementApi/GetPermissionByID?permissionID={permissionID}");
        }

        /// <summary>
        /// Creates or updates a permission.
        /// </summary>
        /// <param name="request">Permission details.</param>
        /// <returns>Operation result.</returns>
        public Task<ApiResponse<dynamic>> UpsertPermissionAsync(
            MstPermissionUpsertRequest request)
        {
            return PostAsync<dynamic>(
                "api/Permission/UpsertPermission",
                request);
        }

        /// <summary>
        /// Updates whether a permission is currently usable.
        /// </summary>
        /// <param name="permissionID">Permission identifier.</param>
        /// <param name="isActive">Active status.</param>
        /// <returns>Operation result.</returns>
        public Task<ApiResponse<SpResult>> TogglePermissionStatusAsync(StatusUpdateRequest request)
        {
            return PostAsync<SpResult>(
                $"api/Permission/TogglePermissionStatus",request);
        }

        /// <summary>
        /// Deletes a permission from the database.
        /// </summary>
        /// <param name="permissionID">Permission identifier.</param>
        /// <returns>Operation result.</returns>
        public Task<ApiResponse<dynamic>> DeletePermissionAsync(
            List<int> permissionID)
        {
            return PostAsync<dynamic>(
                "api/Permission/DeletePermission",
                permissionID);
        }
    }
}
