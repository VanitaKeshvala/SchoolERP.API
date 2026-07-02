using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public class CountryClientService: BaseApiClient, ICountryClientService
    {
        public CountryClientService(HttpClient client) : base(client) { }
        public async Task<ApiResponse<dynamic>> UpsertCountryAsync(CountryRequestModel request)
        {
            return await PostAsync<dynamic>("api/CountryAPI/Save", request);
        }
        public async Task<ApiResponse<CountryModel>> GetByIDAsync(int id)
        {
            return await GetAsync<CountryModel>($"api/CountryAPI/GetById/{id}");
        }

        public async Task<ApiResponse<List<CountryModel>>> GetAllAsync(int? companyId = null, int? sessionId = null)
        {
            return await GetAsync<List<CountryModel>>($"api/CountryAPI/GetAll?companyId={companyId}&sessionId={sessionId}");
        }

        public async Task<ApiResponse<PagedResult<CountryModel>>> GetAllCountryWithPageAsync(HostelTypeSearchRequest request)
        {
            return await PostAsync<PagedResult<CountryModel>>("api/CountryAPI/GetAllCountryWithPage", request);
        }

        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids)
        {
            return await PostAsync<dynamic>($"api/CountryAPI/Delete", ids);
        }

        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request)
        {
            return await PostAsync<dynamic>($"api/CountryAPI/ToggleStatus", request);
        }


    }
}
