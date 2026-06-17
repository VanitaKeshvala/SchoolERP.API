using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class UserMenuPermissionClientService : BaseApiClient, IUserMenuPermissionClientService
    {
        public UserMenuPermissionClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        /// <summary>
        /// Retrieves the ID of the currently logged-in user.
        /// </summary>
        /// <returns>
        /// An API response containing the current user ID.
        /// </returns>
        public async Task<ApiResponse<int>> GetCurrentUserIdAsync()
        {
            return await GetAsync<int>("api/Permission/current-user-id");
        }

        /// <summary>
        /// Checks whether the currently logged-in user has the specified permission.
        /// </summary>
        /// <param name="menuUrlPrefix">
        /// The menu URL prefix used to identify the module or page.
        /// </param>
        /// <param name="permissionName">
        /// The permission name to validate, such as View, Add, Edit, or Delete.
        /// </param>
        /// <returns>
        /// An API response indicating whether the permission is granted.
        /// </returns>
        public async Task<ApiResponse<bool>> Has(
            string menuUrlPrefix,
            string permissionName)
        {
            return await GetAsync<bool>(
                $"api/Permission/has-permission?menuUrlPrefix={Uri.EscapeDataString(menuUrlPrefix)}&permissionName={Uri.EscapeDataString(permissionName)}");
        }
    }
}
