using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This interface defines the rules for how the application communicates with the main system to manage the navigation menus (what you see in the sidebar).
    /// </summary>
    public interface IMenuClientService
    {
        /// <summary>
        /// Asks the main system for a list of all menus.
        /// </summary>
        Task<ApiResponse<List<MenuViewModel>>> GetAllMenusAsync();

        /// <summary>
        /// Asks the main system for details about a specific menu using its ID.
        /// </summary>
        Task<ApiResponse<MenuViewModel>> GetMenuByIdAsync(int id);

        /// <summary>
        /// Sends information to the main system to save or update a menu item.
        /// </summary>
        Task<ApiResponse<bool>> SaveMenuAsync(MenuUpsertRequest request);

        /// <summary>
        /// Tells the main system to turn a menu item on or off.
        /// </summary>
        Task<ApiResponse<bool>> ToggleStatusAsync(int menuId, bool isActive);

        /// <summary>
        /// Sends a list of menu items with their new display order to the main system.
        /// </summary>
        Task<ApiResponse<bool>> UpdateMenuOrderAsync(List<MenuOrderRequest> orders);

        /// <summary>
        /// Deletes a menu item by ID.
        /// </summary>
        Task<ApiResponse<bool>> DeleteMenuAsync(List<int> id);
    }
}
