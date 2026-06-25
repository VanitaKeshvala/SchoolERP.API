using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This service handles the actual work of sending requests to the main system to manage academic session details in the database via an API.
    /// </summary>
    public class SessionClientService : BaseApiClient, ISessionClientService
    {
        public SessionClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        /// <summary>
        /// Sends a request to the server to get a list of all academic sessions.
        /// </summary>
        public async Task<ApiResponse<List<MstSessionViewModel>>> GetAllAsync(bool includeDeleted = false)
        {
            return await GetAsync<List<MstSessionViewModel>>($"api/SessionApi/GetAll?includeDeleted={includeDeleted}");
        }

        /// <summary>
        /// Sends a request to the server to look up details for a specific session by its ID.
        /// </summary>
        public async Task<ApiResponse<MstSessionViewModel>> GetByIDAsync(int id)
        {
            return await GetAsync<MstSessionViewModel>($"api/SessionApi/GetByID/{id}");
        }

        /// <summary>
        /// Sends session details to the server to be saved or updated.
        /// </summary>
        public async Task<ApiResponse<dynamic>> UpsertAsync(MstSessionUpsertRequest request)
        {
            return await PostAsync<dynamic>("api/SessionApi/Upsert", request);
        }

        /// <summary>
        /// Sends a request to the server to delete a session record.
        /// </summary>
        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> id)
        {
            return await PostAsync<dynamic>($"api/SessionApi/Delete/", id);
        }

        /// <summary>
        /// Sends a request to the server to update whether a session is currently active.
        /// </summary>
        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(int id, bool isActive)
        {
            return await PostAsync<dynamic>($"api/SessionApi/ToggleStatus?id={id}&isActive={isActive}", null);
        }

        /// <summary>
        /// Sends a request to the server to save the user's preferred active session.
        /// </summary>
        public async Task<ApiResponse<dynamic>> SetCurrentSessionAsync(SetCurrentSessionRequest request)
        {
            return await PostAsync<dynamic>("api/SessionApi/SetCurrent", request);
        }

        /// <summary>
        /// Sends a request to the server to find out which session the user is currently working with.
        /// </summary>
        public async Task<ApiResponse<int?>> GetUserCurrentSessionAsync()
        {
            return await GetAsync<int?>("api/SessionApi/GetUserCurrentSession");
        }
    }
}
