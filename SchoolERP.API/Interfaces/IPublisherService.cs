using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.API.Interfaces
{
    public interface IPublisherService
    {
        Task<ApiResponse> UpsertPublisherAsync(PublisherRequest model);
        Task<PublisherModel?> GetPublisherByIdAsync(int publisherId);
        Task<PagedResult<PublisherModel>> GetAllPublishertWithPage(SearchRequest req);
        (bool success, string message) DeletePublisher(List<int> ids, int userId);
        (bool success, string message) TogglePublisherStatus(StatusUpdateRequest request);
    }
}
