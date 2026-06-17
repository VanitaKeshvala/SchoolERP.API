using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This service handles the actual work of sending requests to the main system to manage user category details in the database via an API.
    /// </summary>
    public class UserTypeClientService : BaseApiClient, IUserTypeClientService
    {
        public UserTypeClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        /// <summary>
        /// Sends a request to the server to get a list of all user types.
        /// </summary>
        public async Task<ApiResponse<List<MstUserTypeViewModel>>> GetAllAsync()
        {
            return await GetAsync<List<MstUserTypeViewModel>>("api/UserTypeApi");
        }

        /// <summary>
        /// Sends a request to the server to look up details for a specific user type by its ID.
        /// </summary>
        public async Task<ApiResponse<MstUserTypeViewModel>> GetByIdAsync(int id)
        {
            return await GetAsync<MstUserTypeViewModel>($"api/UserTypeApi/{id}");
        }

        /// <summary>
        /// Sends user type details to the server to be saved or updated.
        /// </summary>
        public async Task<ApiResponse<bool>> SaveAsync(MstUserTypeUpsertRequest request)
        {
            return await PostAsync<bool>("api/UserTypeApi/save", request);
        }

        /// <summary>
        /// Sends a request to the server to update whether a user type is active.
        /// </summary>
        public async Task<ApiResponse<bool>> ToggleStatusAsync(int typeId, bool isActive)
        {
            return await PostAsync<bool>($"api/UserTypeApi/toggle-status?typeId={typeId}&isActive={isActive}", null);
        }
    }
}
