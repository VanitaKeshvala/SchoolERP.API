using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.API.Interfaces
{
    public interface ILibraryDocumentStatusService
    {
        Task<ApiResponse> UpsertDocumentStatusAsync(LibraryDocumentStatusRequest model);
        Task<LibraryDocumentStatusModel?> GetDocumentStatusByIdAsync(int documentStatusId);
        Task<PagedResult<LibraryDocumentStatusModel>> GetAllDocumentStatusWithPage(SearchRequest req);
        (bool success, string message) DeleteDocumentStatus(List<int> ids, int userId);
        (bool success, string message) ToggleDocumentStatus(StatusUpdateRequest request);
    }
}
