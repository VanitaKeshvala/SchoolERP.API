using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This service handles the actual work of sending requests to the main system to manage organization details in the database via an API.
    /// </summary>
    public class OrganisationClientService : BaseApiClient, IOrganisationClientService
    {
        public OrganisationClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        /// <summary>
        /// Sends a request to the server to get a list of all organizations.
        /// </summary>
        public async Task<ApiResponse<List<OrganisationViewModel>>> GetAllOrganisationsAsync(bool includeDeleted = false)
        {
            return await GetAsync<List<OrganisationViewModel>>($"api/OrganisationApi/GetAll?includeDeleted={includeDeleted}");
        }

        /// <summary>
        /// Sends a request to the server to look up details for a specific organization by its ID.
        /// </summary>
        public async Task<ApiResponse<OrganisationViewModel>> GetOrganisationByIDAsync(int id)
        {
            return await GetAsync<OrganisationViewModel>($"api/OrganisationApi/GetByID/{id}");
        }

        /// <summary>
        /// Sends organization details to the server to be saved or updated.
        /// </summary>
        public async Task<ApiResponse<dynamic>> UpsertOrganisationAsync(OrganisationUpsertRequest request)
        {
            return await PostAsync<dynamic>("api/OrganisationApi/Upsert", request);
        }

        /// <summary>
        /// Sends a request to the server to delete an organization record.
        /// </summary>
        public async Task<ApiResponse<dynamic>> DeleteOrganisationAsync(int id)
        {
            return await PostAsync<dynamic>($"api/OrganisationApi/Delete/{id}", null);
        }

        /// <summary>
        /// Sends a request to the server to update whether an organization is currently active.
        /// </summary>
        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(int id, bool isActive)
        {
            return await PostAsync<dynamic>($"api/OrganisationApi/ToggleStatus?id={id}&isActive={isActive}", null);
        }
    }
}
