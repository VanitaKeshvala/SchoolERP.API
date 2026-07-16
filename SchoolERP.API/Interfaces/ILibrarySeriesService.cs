using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.API.Interfaces
{
    public interface ILibrarySeriesService
    {
        Task<ApiResponse> UpsertLibrarySeriesAsync(LibrarySeriesRequest model);
        Task<LibrarySeriesModel?> GetLibrarySeriesByIdAsync(int seriesId);
        Task<PagedResult<LibrarySeriesModel>> GetAllLibrarySeriesWithPage(SearchRequest req);
        (bool success, string message) DeleteLibrarySeries(List<int> ids, int userId);
        (bool success, string message) ToggleLibrarySeriesStatus(StatusUpdateRequest request);
    }
}
