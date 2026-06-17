using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class StaffIDCardClientService : BaseApiClient, IStaffIDCardClientService
    {
        public StaffIDCardClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<ApiResponse<List<StaffIDCardViewModel>>> GetAll()
        {
            return await GetAsync<List<StaffIDCardViewModel>>("api/StaffIDCardApi/GetAll");
        }

        public async Task<ApiResponse<StaffIDCardViewModel>> GetByID(int id)
        {
            return await GetAsync<StaffIDCardViewModel>($"api/StaffIDCardApi/GetByID/{id}");
        }

        public async Task<ApiResponse<int>> Upsert(StaffIDCardUpsertRequest request)
        {
            return await PostAsync<int>("api/StaffIDCardApi/Upsert", request);
        }

        public async Task<ApiResponse<int>> Delete(List<int> id)
        {
            return await PostAsync<int>($"api/StaffIDCardApi/Delete",id);
        }

        public async Task<ApiResponse<int>> ToggleStatus(int id, bool isActive)
        {
            return await PostAsync<int>($"api/StaffIDCardApi/ToggleStatus?id={id}&isActive={isActive}", null);
        }
    }
}
