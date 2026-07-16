using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.API.Interfaces
{
    public interface ILibraryDocumentTypeService
    {
        Task<ApiResponse> UpsertLibraryDocumentTypeAsync(LibraryDocumentTypeRequest model);
        Task<LibraryDocumentTypeModel?> GetLibraryDocumentTypeByIdAsync(int documentId);
        Task<PagedResult<LibraryDocumentTypeModel>> GetAllLibraryDocumentTypeWithPage(SearchRequest req);
        (bool success, string message) DeleteLibraryDocumentType(List<int> ids, int userId);
        (bool success, string message) ToggleLibraryDocumentTypeStatus(StatusUpdateRequest request);
    }
}
