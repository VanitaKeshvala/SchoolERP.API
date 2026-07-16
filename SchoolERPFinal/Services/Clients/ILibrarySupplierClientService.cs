using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public interface ILibrarySupplierClientService
    {
        Task<ApiResponse<SpResult>> UpsertLibrarySupplierAsync(LibrarySupplierRequest request);
        Task<ApiResponse<LibrarySupplierModel>> GetByIDAsync(int id);
        Task<ApiResponse<PagedResult<LibrarySupplierModel>>> GetAllLibrarySupplierWithPageAsync(SearchRequest request);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request);
    }
}
