using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public class StateClientService: BaseApiClient, IStateClientService
    {
        public StateClientService(HttpClient client) : base(client) { }
        public async Task<ApiResponse<dynamic>> UpsertStateAsync(StateRequestModel request)
        {
            return await PostAsync<dynamic>("api/StateAPI/Save", request);
        }
        public async Task<ApiResponse<StateModel>> GetByIDAsync(int id)
        {
            return await GetAsync<StateModel>($"api/StateAPI/GetById/{id}");
        }

        public async Task<ApiResponse<List<StateModel>>> GetAllAsync(int? companyId = null, int? sessionId = null)
        {
            return await GetAsync<List<StateModel>>($"api/StateAPI/GetAll?companyId={companyId}&sessionId={sessionId}");
        }

        public async Task<ApiResponse<PagedResult<StateModel>>> GetAllStateWithPageAsync(HostelTypeSearchRequest request)
        {
            return await PostAsync<PagedResult<StateModel>>("api/StateAPI/GetAllStateWithPage", request);
        }

        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids)
        {
            return await PostAsync<dynamic>($"api/StateAPI/Delete", ids);
        }

        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request)
        {
            return await PostAsync<dynamic>($"api/StateAPI/ToggleStatus", request);
        }
        public async Task<ApiResponse<List<StateModel>>> GetAllStateByCountyAsync(int? companyId = null, int? sessionId = null, int? countryId=null)
        {
            return await GetAsync<List<StateModel>>($"api/StateAPI/GetAllStateByCounty?companyId={companyId}&sessionId={sessionId}&countryId={countryId}");
        }
    }
}
