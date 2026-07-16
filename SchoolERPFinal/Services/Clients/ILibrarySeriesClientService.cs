using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public interface ILibrarySeriesClientService
    {
        Task<ApiResponse<SpResult>> UpsertLibrarySeriesAsync(LibrarySeriesRequest request);
        Task<ApiResponse<LibrarySeriesModel>> GetByIDAsync(int id);
        Task<ApiResponse<PagedResult<LibrarySeriesModel>>> GetAllLibrarySeriesWithPageAsync(SearchRequest request);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request);
    }
}
