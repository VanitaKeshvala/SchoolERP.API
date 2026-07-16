using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public interface ILibraryBudgetClientService
    {
        Task<ApiResponse<dynamic>> UpsertLibraryBudgetAsync(LibraryBudgetRequest request);
        Task<ApiResponse<LibraryBudgetModel>> GetByIDAsync(int id);
        Task<ApiResponse<PagedResult<LibraryBudgetModel>>> GetAllLibraryBudgetWithPageAsync(SearchRequest request);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request);
    }
}
