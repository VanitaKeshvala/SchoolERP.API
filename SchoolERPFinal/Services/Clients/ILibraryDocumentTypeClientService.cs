using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public interface ILibraryDocumentTypeClientService
    {
        Task<ApiResponse<SpResult>> UpsertLibraryDocumentTypeAsync(LibraryDocumentTypeRequest request);
        Task<ApiResponse<LibraryDocumentTypeModel>> GetByIDAsync(int id);
        Task<ApiResponse<PagedResult<LibraryDocumentTypeModel>>> GetAllLibraryDocumentTypeWithPageAsync(SearchRequest request);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request);
    }
}
