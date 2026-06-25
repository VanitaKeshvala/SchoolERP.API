using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This service handles the actual work of sending requests to the main system to manage school company details in the database via an API.
    /// </summary>
    public class CompanyClientService : BaseApiClient, ICompanyClientService
    {
        public CompanyClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        /// <summary>
        /// Sends a request to the server to get a list of all school companies.
        /// </summary>
        public async Task<ApiResponse<List<MstCompanyViewModel>>> GetAllAsync(bool includeDeleted = false)
        {
            return await GetAsync<List<MstCompanyViewModel>>($"api/CompanyApi/GetAll?includeDeleted={includeDeleted}");
        }

        /// <summary>
        /// Sends a request to the server to get a list of school companies assigned to the current user.
        /// </summary>
        public async Task<ApiResponse<List<MstCompanyViewModel>>> GetAssignedCompaniesAsync()
        {
            return await GetAsync<List<MstCompanyViewModel>>("api/CompanyApi/GetAssignedCompanies");
        }

        /// <summary>
        /// Sends a request to the server to look up details for a specific company by its ID.
        /// </summary>
        public async Task<ApiResponse<MstCompanyViewModel>> GetByIDAsync(int id)
        {
            return await GetAsync<MstCompanyViewModel>($"api/CompanyApi/GetByID/{id}");
        }

        /// <summary>
        /// Sends company details to the server to be saved or updated.
        /// </summary>
        public async Task<ApiResponse<dynamic>> UpsertAsync(MstCompanyUpsertRequest request)
        {
            return await PostAsync<dynamic>("api/CompanyApi/Upsert", request);
        }

        /// <summary>
        /// Sends a request to the server to delete a company record.
        /// </summary>
        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> id)
        {
            return await PostAsync<dynamic>($"api/CompanyApi/Delete", id);
        }

        /// <summary>
        /// Sends a request to the server to update whether a company is currently active.
        /// </summary>
        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request)
        {
            return await PostAsync<dynamic>($"api/CompanyApi/ToggleStatus", request);
        }

        /// <summary>
        /// Sends a request to the server to save the user's preferred active school company.
        /// </summary>
        public async Task<ApiResponse<dynamic>> SetCurrentCompanyAsync(SetCurrentCompanyRequest request)
        {
            return await PostAsync<dynamic>("api/CompanyApi/SetCurrent", request);
        }

        /// <summary>
        /// Sends a request to the server to find out which school company the user is currently working with.
        /// </summary>
        public async Task<ApiResponse<int?>> GetUserCurrentCompanyAsync()
        {
            return await GetAsync<int?>("api/CompanyApi/GetUserCurrentCompany");
        }
    }
}
