using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public class LibraryBudgetClientService:BaseApiClient, ILibraryBudgetClientService
    {
        public LibraryBudgetClientService(HttpClient client) : base(client) { }

        public async Task<ApiResponse<dynamic>> UpsertLibraryBudgetAsync(LibraryBudgetRequest request)
        {
            return await PostAsync<dynamic>("api/LibraryBudgetAPI/Save", request);
        }
        public async Task<ApiResponse<LibraryBudgetModel>> GetByIDAsync(int id)
        {
            return await GetAsync<LibraryBudgetModel>($"api/LibraryBudgetAPI/GetById/{id}");
        }
        public async Task<ApiResponse<PagedResult<LibraryBudgetModel>>> GetAllLibraryBudgetWithPageAsync(SearchRequest request)
        {
            return await PostAsync<PagedResult<LibraryBudgetModel>>("api/LibraryBudgetAPI/GetAllLibraryBudgetWithPage", request);
        }
        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids)
        {
            return await PostAsync<dynamic>($"api/LibraryBudgetAPI/Delete", ids);
        }

        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request)
        {
            return await PostAsync<dynamic>($"api/LibraryBudgetAPI/ToggleStatus", request);
        }
    }
}
