using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public class LibraryLanguageClientService : BaseApiClient, ILibraryLanguageClientService
    {
        public LibraryLanguageClientService(HttpClient client) : base(client) { }

        public async Task<ApiResponse<dynamic>> UpsertPublisherAsync(LibraryLanguageRequest request)
        {
            return await PostAsync<dynamic>("api/LibraryLanguageAPI/Save", request);
        }
        public async Task<ApiResponse<LibraryLanguageModel>> GetByIDAsync(int id)
        {
            return await GetAsync<LibraryLanguageModel>($"api/LibraryLanguageAPI/GetById/{id}");
        }
        public async Task<ApiResponse<PagedResult<LibraryLanguageModel>>> GetAllLibraryLanguageWithPageAsync(SearchRequest request)
        {
            return await PostAsync<PagedResult<LibraryLanguageModel>>("api/LibraryLanguageAPI/GetAllLibraryLanguageWithPage", request);
        }
        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids)
        {
            return await PostAsync<dynamic>($"api/LibraryLanguageAPI/Delete", ids);
        }

        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request)
        {
            return await PostAsync<dynamic>($"api/LibraryLanguageAPI/ToggleStatus", request);
        }
    }
}
