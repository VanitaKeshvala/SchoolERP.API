using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.API.Interfaces
{
    public interface ILibraryBudgetService
    {
        Task<ApiResponse> UpsertLibraryBudgetAsync(LibraryBudgetRequest model);
        Task<LibraryBudgetModel?> GetLibraryBudgetByIdAsync(int budgetId);
        Task<PagedResult<LibraryBudgetModel>> GetAllLibraryBudgetWithPage(SearchRequest req);
        (bool success, string message) DeleteLibraryBudgety(List<int> ids, int userId);
        (bool success, string message) ToggleLibraryBudgetyStatus(StatusUpdateRequest request);
    }
}
