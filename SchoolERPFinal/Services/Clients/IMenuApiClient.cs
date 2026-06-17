using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface IMenuApiClient
    {
        /// <summary>
        /// Retrieves the complete list of menus available for the current user.
        /// </summary>
        /// <returns>
        /// Returns a collection of menu items wrapped in an API response.
        /// </returns>
        Task<ApiResponse<List<MenuViewModel>>> GetMenusAsync();

        /// <summary>
        /// Loads user permissions based on the specified user ID.
        /// Optionally filters permissions by menu URL prefix and permission name.
        /// </summary>
        /// <param name="userId">The ID of the user whose permissions are being retrieved.</param>
        /// <param name="menuUrlPrefix">Optional menu URL prefix used to filter permissions.</param>
        /// <param name="permissionName">Optional permission name used to filter specific permissions.</param>
        /// <returns>
        /// Returns a collection of user permissions wrapped in an API response.
        /// </returns>
        Task<ApiResponse<List<UserPermissionViewModel>>> LoadPermissions(int userId, string? menuUrlPrefix = null, string? permissionName = null);
    }
}
