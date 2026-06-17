using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class SubjectGroupClientService : BaseApiClient, ISubjectGroupClientService
    {
        public SubjectGroupClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<ApiResponse<List<MstSubjectGroupViewModel>>> GetAllAsync(bool includeDeleted = false)
        {
            return await GetAsync<List<MstSubjectGroupViewModel>>($"api/SubjectGroupApi/GetAll?includeDeleted={includeDeleted}");
        }

        public async Task<ApiResponse<MstSubjectGroupViewModel>> GetByIDAsync(int id)
        {
            return await GetAsync<MstSubjectGroupViewModel>($"api/SubjectGroupApi/GetByID/{id}");
        }

        public async Task<ApiResponse<dynamic>> UpsertAsync(MstSubjectGroupUpsertRequest request)
        {
            return await PostAsync<dynamic>("api/SubjectGroupApi/Upsert", request);
        }

        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids)
        {
            return await PostAsync<dynamic>($"api/SubjectGroupApi/Delete", ids!);
        }

        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(int id, bool isActive)
        {
            return await PostAsync<dynamic>($"api/SubjectGroupApi/ToggleStatus?id={id}&isActive={isActive}", null!);
        }
    }
}
