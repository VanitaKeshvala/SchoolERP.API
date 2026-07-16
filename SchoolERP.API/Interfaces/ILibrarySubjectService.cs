using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.API.Interfaces
{
    public interface ILibrarySubjectService
    {
        Task<ApiResponse> UpsertLibrarySubjectAsync(LibrarySubjectRequest model);
        Task<LibrarySubjectModel?> GetLibrarySubjectByIdAsync(int subjectId);
        Task<PagedResult<LibrarySubjectModel>> GetAllLibrarySubjectWithPage(SearchRequest req);
        (bool success, string message) DeleteLibrarySubject(List<int> ids, int userId);
        (bool success, string message) ToggleLibrarySubjectStatus(StatusUpdateRequest request);
    }
}
