using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface IUserManagementClientService
    {
        Task<ApiResponse<List<UserPermissionViewModel>>> GetUserPermissions(int userId);
        /// <summary>
        /// Retrieves all permissions.
        /// </summary>
        /// <returns>List of permissions.</returns>
        Task<ApiResponse<List<MstPermissionViewModel>>> GetAllPermissionsAsync();

        /// <summary>
        /// Retrieves the details of a specific permission.
        /// </summary>
        /// <param name="permissionID">Permission identifier.</param>
        /// <returns>Permission details.</returns>
        Task<ApiResponse<MstPermissionViewModel>> GetPermissionByIDAsync(
            int permissionID);

        /// <summary>
        /// Creates or updates a permission.
        /// </summary>
        /// <param name="request">Permission details.</param>
        /// <returns>Operation result.</returns>
        Task<ApiResponse<dynamic>> UpsertPermissionAsync(
            MstPermissionUpsertRequest request);
        Task<ApiResponse<dynamic>> TogglePermissionStatusAsync(
            int permissionID,
            bool isActive);
        Task<ApiResponse<dynamic>> DeletePermissionAsync(
            List<int> permissionID);
    }
}
