using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class ClassClientService : BaseApiClient, IClassClientService
    {
        public ClassClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<ApiResponse<List<MstClassViewModel>>> GetAllAsync(bool includeDeleted = false)
        {
            return await GetAsync<List<MstClassViewModel>>($"api/ClassApi/GetAll?includeDeleted={includeDeleted}");
        }

        public async Task<ApiResponse<MstClassViewModel>> GetByIDAsync(int id)
        {
            return await GetAsync<MstClassViewModel>($"api/ClassApi/GetByID/{id}");
        }

        public async Task<ApiResponse<dynamic>> UpsertAsync(MstClassUpsertRequest request)
        {
            return await PostAsync<dynamic>("api/ClassApi/Upsert", request);
        }

        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids)
        {
            return await PostAsync<dynamic>($"api/ClassApi/Delete", ids);
        }

        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(int id, bool isActive)
        {
            return await PostAsync<dynamic>($"api/ClassApi/ToggleStatus?id={id}&isActive={isActive}", null!);
        }
    }
}
