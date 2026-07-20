using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public interface IDailyAssignmentClientService
    {
        Task<ApiResponse<dynamic>> UpsertAsync(AssignmentUpsertRequest request);
        Task<ApiResponse<dynamic>> UpsertAttachmentAsync(AssignmentAttachmentUpsertRequest request);
        Task<ApiResponse<PagedResult<DailyAssignmentModel>>> GetAllDailyAssignmentWithPageAsync(DailyAssignmentSearchRequest request);
        Task<ApiResponse<DailyAssignmentModel>> GetByIDAsync(int assignmentID);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
    }
}
