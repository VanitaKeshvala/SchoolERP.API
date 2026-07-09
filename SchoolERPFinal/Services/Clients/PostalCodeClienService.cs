using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public class PostalCodeClienService:BaseApiClient, IPostalCodeClienService
    {
        public PostalCodeClienService(HttpClient client) : base(client) { }
        
        public async Task<ApiResponse<dynamic>> UpsertPostalCodAsync(PostalCodeEditModel request)
        {
            return await PostAsync<dynamic>("api/PostalCodeAPI/Save", request);
        }
        public async Task<ApiResponse<PostalCodeListViewModel>> GetByIDAsync(int id)
        {
            return await GetAsync<PostalCodeListViewModel>($"api/PostalCodeAPI/GetById/{id}");
        }

        public async Task<ApiResponse<List<PostalCodeListViewModel>>> GetAllAsync(int? companyId = null, int? sessionId = null)
        {
            return await GetAsync<List<PostalCodeListViewModel>>($"api/PostalCodeAPI/GetAll?companyId={companyId}&sessionId={sessionId}");
        }

        public async Task<ApiResponse<PagedResult<PostalCodeListViewModel>>> GetAllPostalCodeWithPageAsync(SerachPostalCode request)
        {
            return await PostAsync<PagedResult<PostalCodeListViewModel>>("api/PostalCodeAPI/GetAllPostalCodeWithPage", request);
        }

        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids)
        {
            return await PostAsync<dynamic>($"api/PostalCodeAPI/Delete", ids);
        }

        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request)
        {
            return await PostAsync<dynamic>($"api/PostalCodeAPI/ToggleStatus", request);
        }
       
        public async Task<ApiResponse<List<PostalCodeListViewModel>>> SearchPostalCodeAsync(string term)
        {
            return await GetAsync<List<PostalCodeListViewModel>>($"api/PostalCodeAPI/SearchPostalCode?term={term}");
        }
    }
}
