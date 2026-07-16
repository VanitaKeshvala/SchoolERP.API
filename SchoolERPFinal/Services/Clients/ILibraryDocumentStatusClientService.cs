using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public interface ILibraryDocumentStatusClientService
    {
        Task<ApiResponse<SpResult>> UpsertLibraryDocumentStatusAsync(LibraryDocumentStatusRequest request);
        Task<ApiResponse<LibraryDocumentStatusModel>> GetByIDAsync(int id);
        Task<ApiResponse<PagedResult<LibraryDocumentStatusModel>>> GetAllDocumentStatusWithPageAsync(SearchRequest request);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request);
    }
}
