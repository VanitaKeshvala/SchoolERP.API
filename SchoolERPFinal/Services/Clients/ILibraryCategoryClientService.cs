using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public interface ILibraryCategoryClientService
    {
        Task<ApiResponse<SpResult>> UpsertLibraryCategoryAsync(LibraryCategoryRequest request);
        Task<ApiResponse<LibraryCategoryModel>> GetByIDAsync(int id);
        Task<ApiResponse<PagedResult<LibraryCategoryModel>>> GetAllLibraryCategoryWithPagePageAsync(SearchRequest request);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request);
    }
}
