using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This service handles the actual work of sending requests to the main system to manage language settings in the database via an API.
    /// </summary>
    public class LanguageClientService : BaseApiClient, ILanguageClientService
    {
        public LanguageClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        /// <summary>
        /// Sends a request to the server to get a list of all available languages.
        /// </summary>
        public async Task<ApiResponse<List<MstLanguageViewModel>>> GetAllAsync(bool includeDeleted = false)
        {
            return await GetAsync<List<MstLanguageViewModel>>($"api/LanguageApi/GetAll?includeDeleted={includeDeleted}");
        }

        /// <summary>
        /// Sends a request to the server to look up details for a specific language by its ID.
        /// </summary>
        public async Task<ApiResponse<MstLanguageViewModel>> GetByIDAsync(int id)
        {
            return await GetAsync<MstLanguageViewModel>($"api/LanguageApi/GetByID/{id}");
        }

        /// <summary>
        /// Sends language details to the server to be saved or updated.
        /// </summary>
        public async Task<ApiResponse<dynamic>> UpsertAsync(MstLanguageUpsertRequest request)
        {
            return await PostAsync<dynamic>("api/LanguageApi/Upsert", request);
        }

        /// <summary>
        /// Sends a request to the server to delete a language record.
        /// </summary>
        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> id)
        {
            return await PostAsync<dynamic>($"api/LanguageApi/Delete/", id);
        }

        /// <summary>
        /// Sends a request to the server to update whether a language is currently active.
        /// </summary>
        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(int id, bool isActive)
        {
            return await PostAsync<dynamic>($"api/LanguageApi/ToggleStatus?id={id}&isActive={isActive}", null);
        }
    }
}
