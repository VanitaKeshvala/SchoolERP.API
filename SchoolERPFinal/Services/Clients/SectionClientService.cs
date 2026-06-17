using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class SectionClientService : BaseApiClient, ISectionClientService
    {
        public SectionClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<ApiResponse<List<MstSectionViewModel>>> GetAllAsync(bool includeDeleted = false)
        {
            return await GetAsync<List<MstSectionViewModel>>($"api/SectionApi/GetAll?includeDeleted={includeDeleted}");
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

        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(int id, bool isActive)
        {
            return await PostAsync<dynamic>($"api/SectionApi/ToggleStatus?id={id}&isActive={isActive}", null!);
        }
    }
}
