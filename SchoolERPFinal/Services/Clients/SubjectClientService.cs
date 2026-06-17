using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class SubjectClientService : BaseApiClient, ISubjectClientService
    {
        public SubjectClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<ApiResponse<List<MstSubjectViewModel>>> GetAllAsync(bool includeDeleted = false)
        {
            return await GetAsync<List<MstSubjectViewModel>>($"api/SubjectApi/GetAll?includeDeleted={includeDeleted}");
        }

        public async Task<ApiResponse<MstSubjectViewModel>> GetByIDAsync(int id)
        {
            return await GetAsync<MstSubjectViewModel>($"api/SubjectApi/GetByID/{id}");
        }

        public async Task<ApiResponse<dynamic>> UpsertAsync(MstSubjectUpsertRequest request)
        {
            return await PostAsync<dynamic>("api/SubjectApi/Upsert", request);
        }

        public async Task<ApiResponse<dynamic>> DeleteAsync([FromBody] List<int> ids)
        {
            return await PostAsync<dynamic>($"api/SubjectApi/Delete/", ids!);
        }

        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(int id, bool isActive)
        {
            return await PostAsync<dynamic>($"api/SubjectApi/ToggleStatus?id={id}&isActive={isActive}", null!);
        }
    }
}
