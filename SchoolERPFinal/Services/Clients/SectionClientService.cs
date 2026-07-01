using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class SectionClientService : BaseApiClient, ISectionClientService
    {
        public SectionClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<ApiResponse<List<MstSectionViewModel>>> GetAllAsync(bool includeDeleted = false, int sessionId=0)
        {
            return await GetAsync<List<MstSectionViewModel>>($"api/SectionApi/GetAll?includeDeleted={includeDeleted}&sessionId={sessionId}");
        }

        public async Task<ApiResponse<List<MstSectionViewModel>>> GetByClassAsync(int classId)
        {
            return await GetAsync<List<MstSectionViewModel>>($"api/SectionApi/GetByClass/{classId}");
        }

        public async Task<ApiResponse<MstSectionViewModel>> GetByIDAsync(int id)
        {
            return await GetAsync<MstSectionViewModel>($"api/SectionApi/GetByID/{id}");
        }

        public async Task<ApiResponse<dynamic>> UpsertAsync(MstSectionUpsertRequest request)
        {
            return await PostAsync<dynamic>("api/SectionApi/Upsert", request);
        }

        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids)
        {
            return await PostAsync<dynamic>($"api/SectionApi/Delete", ids!);
        }

        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request)
        {
            return await PostAsync<dynamic>($"api/SectionApi/ToggleStatus", request);
        }

        public async Task<ApiResponse<dynamic>> CopyToSessionAsync(SectionCopyRequest request)
        {
            return await PostAsync<dynamic>($"api/SectionApi/CopyToSession", request);
        }

        public async Task<ApiResponse<PagedResult<MstSectionViewModel>>> GetAllSectionWithPagePageAsync(HostelTypeSearchRequest request)
        {
            return await PostAsync<PagedResult<MstSectionViewModel>>("api/SectionApi/GetAllSectionWithPage", request);
        }
    }
}
