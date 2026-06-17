using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class HomeworkClientService : BaseApiClient, IHomeworkClientService
    {
        public HomeworkClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<ApiResponse<List<HomeworkViewModel>>> GetAllAsync(bool includeDeleted = false)
        {
            return await GetAsync<List<HomeworkViewModel>>($"api/HomeworkApi/GetAll?includeDeleted={includeDeleted}");
        }

        public async Task<ApiResponse<HomeworkViewModel>> GetByIDAsync(int id)
        {
            return await GetAsync<HomeworkViewModel>($"api/HomeworkApi/GetByID/{id}");
        }

        public async Task<ApiResponse<dynamic>> UpsertAsync(HomeworkUpsertRequest request)
        {
            return await PostAsync<dynamic>("api/HomeworkApi/Upsert", request);
        }

        public async Task<ApiResponse<dynamic>> DeleteAsync(int id)
        {
            return await PostAsync<dynamic>($"api/HomeworkApi/Delete/{id}", null!);
        }

        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(int id, bool isActive)
        {
            return await PostAsync<dynamic>($"api/HomeworkApi/ToggleStatus?id={id}&isActive={isActive}", null!);
        }
    }
}
