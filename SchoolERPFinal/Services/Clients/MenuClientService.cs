using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This service handles the actual work of sending requests to the main system to manage navigation menus in the database via an API.
    /// </summary>
    public class MenuClientService : BaseApiClient, IMenuClientService
    {
        public MenuClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        /// <summary>
        /// Sends a request to the server to get a list of all menus.
        /// </summary>
        public async Task<ApiResponse<List<MenuViewModel>>> GetAllMenusAsync()
        {
            return await GetAsync<List<MenuViewModel>>("api/MasterMenuApi");
        }

        /// <summary>
        /// Sends a request to the server to look up details for a specific menu by its ID.
        /// </summary>
        public async Task<ApiResponse<MenuViewModel>> GetMenuByIdAsync(int id)
        {
            return await GetAsync<MenuViewModel>($"api/MasterMenuApi/{id}");
        }

        /// <summary>
        /// Sends menu details to the server to be saved or updated.
        /// </summary>
        public async Task<ApiResponse<bool>> SaveMenuAsync(MenuUpsertRequest request)
        {
            return await PostAsync<bool>("api/MasterMenuApi/save", request);
        }

        /// <summary>
        /// Sends a request to the server to update whether a menu item is currently visible.
        /// </summary>
        public async Task<ApiResponse<bool>> ToggleStatusAsync(int menuId, bool isActive)
        {
            return await PostAsync<bool>($"api/MasterMenuApi/toggle-status?menuId={menuId}&isActive={isActive}", null);
        }

        /// <summary>
        /// Sends a request to the server to update the sequence in which menus appear.
        /// </summary>
        public async Task<ApiResponse<bool>> UpdateMenuOrderAsync(List<MenuOrderRequest> orders)
        {
            return await PostAsync<bool>("api/MasterMenuApi/update-order", orders);
        }

        /// <summary>
        /// Sends a request to the server to delete a specific menu item.
        /// </summary>
        public async Task<ApiResponse<bool>> DeleteMenuAsync(List<int> id)
        {
            return await PostAsync<bool>($"api/MasterMenuApi/delete", id);
        }
    }
}
