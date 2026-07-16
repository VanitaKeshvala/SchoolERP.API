using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.API.Interfaces
{
    public interface ILibraryLanguageService
    {
        Task<ApiResponse> UpsertLibraryLanguageAsync(LibraryLanguageRequest model);
        Task<LibraryLanguageModel?> GetLibraryLanguageByIdAsync(int languageId);
        Task<PagedResult<LibraryLanguageModel>> GetAllLibraryLanguageWithPage(SearchRequest req);
        (bool success, string message) DeleteLibraryLanguage(List<int> ids, int userId);
        (bool success, string message) ToggleLibraryLanguageStatus(StatusUpdateRequest request);
    }
}
