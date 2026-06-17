using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class MenuApiClient : BaseApiClient, IMenuApiClient
    {
        public MenuApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        /// <summary>
        /// Retrieves all menu items from the Menu API.
        /// </summary>
        /// <returns>
        /// Returns a list of menu records wrapped in an API response.
        /// </returns>
        public async Task<ApiResponse<List<MenuViewModel>>> GetMenusAsync()
        {
            return await GetAsync<List<MenuViewModel>>(
                "api/MenuAPI/GetAllMenus");
        }

        /// <summary>
        /// Retrieves permission details for the specified user.
        /// Optionally filters permissions by menu URL prefix and permission name.
        /// </summary>
        /// <returns>
        /// Returns a list of user permissions wrapped in an API response.
        /// </returns>
        public async Task<ApiResponse<List<UserPermissionViewModel>>> LoadPermissions(int userId, string? menuUrlPrefix = null, string? permissionName = null)
        {
            return await PostAsync<List<UserPermissionViewModel>>(
                $"api/MenuAPI/GetUserPermissions?userId={userId}&menuUrlPrefix={menuUrlPrefix}&permissionName={permissionName}",null);
        }
    }
}
