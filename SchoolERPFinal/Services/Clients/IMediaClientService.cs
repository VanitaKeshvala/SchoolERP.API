using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public interface IMediaClientService
    {
        Task<ApiResponse<dynamic>> UpsertAsync(MediaRequest request);
        Task<ApiResponse<PagedResult<MediaViewModel>>> GetAllMediaWithPageAsync(SearchRequest request);
    }
}
