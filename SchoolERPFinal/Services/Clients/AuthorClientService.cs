using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public class AuthorClientService: BaseApiClient,IAuthorClientService
    {
        public AuthorClientService(HttpClient client) : base(client) { }

        public async Task<ApiResponse<dynamic>> UpsertAuthorAsync(AuthorRequest request)
        {
            return await PostAsync<dynamic>("api/AuthorAPI/Save", request);
        }
        public async Task<ApiResponse<AuthorModel>> GetByIDAsync(int id)
        {
            return await GetAsync<AuthorModel>($"api/AuthorAPI/GetById/{id}");
        }
        public async Task<ApiResponse<PagedResult<AuthorModel>>> GetAllAuthorWithPageAsync(SearchRequest request)
        {
            return await PostAsync<PagedResult<AuthorModel>>("api/AuthorAPI/GetAllAuthorWithPage", request);
        }
        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids)
        {
            return await PostAsync<dynamic>($"api/AuthorAPI/Delete", ids);
        }

        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request)
        {
            return await PostAsync<dynamic>($"api/AuthorAPI/ToggleStatus", request);
        }
    }
}
