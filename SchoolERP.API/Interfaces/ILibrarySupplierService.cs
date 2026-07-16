using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.API.Interfaces
{
    public interface ILibrarySupplierService
    {
        Task<ApiResponse> UpsertLibrarySupplierAsync(LibrarySupplierRequest model);
        Task<LibrarySupplierModel?> GetLibrarySupplierByIdAsync(int supplierId);
        Task<PagedResult<LibrarySupplierModel>> GetAllLibrarySupplierWithPage(SearchRequest req);
        (bool success, string message) DeleteLibrarySupplier(List<int> ids, int userId);
        (bool success, string message) ToggleLibrarySupplierStatus(StatusUpdateRequest request);
    }
}
