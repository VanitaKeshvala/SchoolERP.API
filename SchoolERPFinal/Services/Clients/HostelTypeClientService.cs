using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public class HostelTypeClientService: BaseApiClient, IHostelTypeClientService
    {
        public HostelTypeClientService(HttpClient client) : base(client) { }
                
        public async Task<ApiResponse<dynamic>> UpsertHostelTypeAsync(HostelTypeUpsertRequest request)
        {
            return await PostAsync<dynamic>("api/HostelTypeAPI/Save", request);
        }
        public async Task<ApiResponse<HostelTypeModel>> GetByIDAsync(int id)
        {
            return await GetAsync<HostelTypeModel>($"api/HostelTypeAPI/GetById/{id}");
        }

        public async Task<ApiResponse<List<HostelTypeModel>>> GetAllAsync(int? companyId = null,int ? sessionId = null)
        {
            return await GetAsync<List<HostelTypeModel>>($"api/HostelTypeAPI/GetAll?companyId={companyId}&sessionId={sessionId}");
        }

        public async Task<ApiResponse<PagedResult<HostelTypeModel>>> GetAllHostelTypeWithPageAsync(HostelTypeSearchRequest request)
        {
            return await PostAsync<PagedResult<HostelTypeModel>>("api/HostelTypeAPI/GetAllHostelTypeWithPage", request);
        }

        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids)
        {
            return await PostAsync<dynamic>($"api/HostelTypeAPI/Delete", ids);
        }

        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request)
        {
            return await PostAsync<dynamic>($"api/HostelTypeAPI/ToggleStatus", request);
        }


    }
}
