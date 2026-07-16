using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public class LibraryDocumentStatusClientService : BaseApiClient, ILibraryDocumentStatusClientService
    {
        public LibraryDocumentStatusClientService(HttpClient client) : base(client) { }

        public async Task<ApiResponse<SpResult>> UpsertLibraryDocumentStatusAsync(LibraryDocumentStatusRequest request)
        {
            return await PostAsync<SpResult>("api/LibraryDocumentStatusAPI/Save", request);
        }
        public async Task<ApiResponse<LibraryDocumentStatusModel>> GetByIDAsync(int id)
        {
            return await GetAsync<LibraryDocumentStatusModel>($"api/LibraryDocumentStatusAPI/GetById/{id}");
        }
        public async Task<ApiResponse<PagedResult<LibraryDocumentStatusModel>>> GetAllDocumentStatusWithPageAsync(SearchRequest request)
        {
            return await PostAsync<PagedResult<LibraryDocumentStatusModel>>("api/LibraryDocumentStatusAPI/GetAllDocumentStatusWithPage", request);
        }
        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids)
        {
            return await PostAsync<dynamic>($"api/LibraryDocumentStatusAPI/Delete", ids);
        }

        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request)
        {
            return await PostAsync<dynamic>($"api/LibraryDocumentStatusAPI/ToggleStatus", request);
        }
    }
}
