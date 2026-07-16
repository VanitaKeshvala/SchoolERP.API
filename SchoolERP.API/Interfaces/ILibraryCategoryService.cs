using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.API.Interfaces
{
    public interface ILibraryCategoryService
    {
        Task<ApiResponse> UpsertLibraryCategoryAsync(LibraryCategoryRequest model);
        Task<LibraryCategoryModel?> GetLibraryCategoryByIdAsync(int categoryId);
        Task<PagedResult<LibraryCategoryModel>> GetAllLibraryCategoryWithPage(SearchRequest req);
        (bool success, string message) DeleteLibraryCategory(List<int> ids, int userId);
        (bool success, string message) ToggleLibraryCategoryStatus(StatusUpdateRequest request);
    }
}
