using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public interface ILibraryCategorySubjectClientService
    {
        Task<ApiResponse<SpResult>> UpsertLibraryCategorySubjectAsync(LibraryCategorySubjectRequest request);
        Task<ApiResponse<LibraryCategorySubjectModel>> GetByIDAsync(int id);
        Task<ApiResponse<PagedResult<LibraryCategorySubjectModel>>> GetAllLibraryCategorySubjectWithPageAsync(LibraryCategorySubjectSearchRequest request);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request);
        Task<ApiResponse<List<LibraryCategoryModel>>> GetAllLibraryCategoryAsync(int? companyId = null);
        Task<ApiResponse<List<LibrarySubjectModel>>> GetAllLibrarySubjectAsync(int? companyId = null);
    }
}
