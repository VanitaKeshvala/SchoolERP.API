using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class StudentIDCardClientService : BaseApiClient, IStudentIDCardClientService
    {
        public StudentIDCardClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<ApiResponse<List<StudentIDCardViewModel>>> GetAll()
        {
            return await GetAsync<List<StudentIDCardViewModel>>("api/StudentIDCardApi/GetAll");
        }

        public async Task<ApiResponse<StudentIDCardViewModel>> GetByID(int id)
        {
            return await GetAsync<StudentIDCardViewModel>($"api/StudentIDCardApi/GetByID/{id}");
        }

        public async Task<ApiResponse<int>> Upsert(StudentIDCardUpsertRequest request)
        {
            return await PostAsync<int>("api/StudentIDCardApi/Upsert", request);
        }

        public async Task<ApiResponse<int>> Delete(List<int> id)
        {
            return await PostAsync<int>($"api/StudentIDCardApi/Delete", id);
        }

        public async Task<ApiResponse<int>> ToggleStatus(int id, bool isActive)
        {
            return await PostAsync<int>($"api/StudentIDCardApi/ToggleStatus?id={id}&isActive={isActive}", null);
        }
    }
}
