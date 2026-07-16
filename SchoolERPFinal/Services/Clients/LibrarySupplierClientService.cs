using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public class LibrarySupplierClientService: BaseApiClient, ILibrarySupplierClientService
    {
        public LibrarySupplierClientService(HttpClient client) : base(client) { }

        public async Task<ApiResponse<SpResult>> UpsertLibrarySupplierAsync(LibrarySupplierRequest request)
        {
            return await PostAsync<SpResult>("api/LibrarySupplierAPI/Save", request);
        }
        public async Task<ApiResponse<LibrarySupplierModel>> GetByIDAsync(int id)
        {
            return await GetAsync<LibrarySupplierModel>($"api/LibrarySupplierAPI/GetById/{id}");
        }
        public async Task<ApiResponse<PagedResult<LibrarySupplierModel>>> GetAllLibrarySupplierWithPageAsync(SearchRequest request)
        {
            return await PostAsync<PagedResult<LibrarySupplierModel>>("api/LibrarySupplierAPI/GetAllLibrarySupplierWithPage", request);
        }
        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids)
        {
            return await PostAsync<dynamic>($"api/LibrarySupplierAPI/Delete", ids);
        }

        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request)
        {
            return await PostAsync<dynamic>($"api/LibrarySupplierAPI/ToggleStatus", request);
        }
    }
}
