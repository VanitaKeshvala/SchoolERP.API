using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public class WardensClientService: BaseApiClient, IWardensClientService
    {
        public WardensClientService(HttpClient client) : base(client) { }
        public async Task<ApiResponse<WardenSaveResponse>> UpsertWardensAsync(WardensRequestModel request)
        {
            return await PostAsync<WardenSaveResponse>("api/WardensAPI/Save", request);
        }
        public async Task<ApiResponse<WardensModel>> GetByIDAsync(int id)
        {
            return await GetAsync<WardensModel>($"api/WardensAPI/GetById/{id}");
        }

        public async Task<ApiResponse<List<WardensModel>>> GetAllAsync(int? companyId = null, int? sessionId = null)
        {
            return await GetAsync<List<WardensModel>>($"api/WardensAPI/GetAll?companyId={companyId}&sessionId={sessionId}");
        }

        public async Task<ApiResponse<PagedResult<WardensModel>>> GetAllWardensWithPageAsync(WardensSearchRequest request)
        {
            return await PostAsync<PagedResult<WardensModel>>("api/WardensAPI/GetAllWardensWithPage", request);
        }

        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids)
        {
            return await PostAsync<dynamic>($"api/WardensAPI/Delete", ids);
        }

        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(WardensRequestModel request)
        {
            return await PostAsync<dynamic>($"api/WardensAPI/ToggleStatus", request);
        }

        public async Task<ApiResponse<PagedResult<WardensModel>>> UpdateWardenProfileAsync(WardenProfileRequest request)
        {
            return await PostAsync<PagedResult<WardensModel>>("api/WardensAPI/UpdateWardenProfile", request);
        }
    }
}
