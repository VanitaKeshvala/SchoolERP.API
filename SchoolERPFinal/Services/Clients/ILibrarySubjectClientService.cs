using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public interface ILibrarySubjectClientService
    {
        Task<ApiResponse<SpResult>> UpsertLibrarySubjectAsync(LibrarySubjectRequest request);
        Task<ApiResponse<LibrarySubjectModel>> GetByIDAsync(int id);
        Task<ApiResponse<PagedResult<LibrarySubjectModel>>> GetAllLibrarySubjectWithPageAsync(SearchRequest request);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request);
    }
}
