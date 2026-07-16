using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.API.Interfaces
{
    public interface IHomeworkService
    {
        List<HomeworkViewModel> GetAll(int companyId, int sessionId, bool includeDeleted = false);
        HomeworkViewModel? GetByID(int id);
        Task<ApiResponse> Upsert(
            HomeworkUpsertRequest request,
            int companyId,
            int sessionId,
            int userId);
        (bool success, string message) Delete(List<int> ids, int userId);
        (bool success, string message) ToggleStatus(StatusUpdateRequest request);
        Task<PagedResult<HomeworkViewModel>> GetAllHomeWorkWithPage(SearchRequest req);
    }
}
