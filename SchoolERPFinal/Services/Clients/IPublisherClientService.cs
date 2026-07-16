using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public interface IPublisherClientService
    {
        Task<ApiResponse<dynamic>> UpsertPublisherAsync(PublisherRequest request);
        Task<ApiResponse<PublisherModel>> GetByIDAsync(int id);
        Task<ApiResponse<PagedResult<PublisherModel>>> GetAllPublishertWithPageAsync(SearchRequest request);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request);
    }
}
