using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public class CityClientService:BaseApiClient, ICityClientService
    {
        public CityClientService(HttpClient client) : base(client) { }
        
        public async Task<ApiResponse<CityApiResponse>> UpsertCityAsync(CityUpsertRequest request)
        {
            return await PostAsync<CityApiResponse>("api/CityAPI/Save", request);
        }
        public async Task<ApiResponse<CityModel>> GetByIDAsync(int id)
        {
            return await GetAsync<CityModel>($"api/CityAPI/GetById/{id}");
        }

        public async Task<ApiResponse<CityListResponse>> GetAllAsync(int? companyId = null, int? sessionId = null)
        {
            return await GetAsync<CityListResponse>($"api/CityAPI/GetAll?companyId={companyId}&sessionId={sessionId}");
        }

        public async Task<ApiResponse<PagedResult<CityModel>>> GetAllCityWithPageAsync(CitySearchRequest request)
        {
            return await PostAsync<PagedResult<CityModel>>("api/CityAPI/GetAllCityWithPage", request);
        }

        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids)
        {
            return await PostAsync<dynamic>($"api/CityAPI/Delete", ids);
        }

        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request)
        {
            return await PostAsync<dynamic>($"api/CityAPI/ToggleStatus", request);
        }
        public async Task<ApiResponse<List<CityModel>>> GetAllCityByStateIdWisesync(int id)
        {
            return await GetAsync<List<CityModel>>($"api/CityAPI/GetAllCityByStateIdWise/{id}");
        }
    }
}
