using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public interface IManageLessonPlanClientService
    {
        Task<ApiResponse<ManageLessonPlanViewModel>> GetByIDAsync(int id);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> UpsertAsync(ManageLessonPlanRequest request);
        Task<ApiResponse<PagedResult<ManageLessonPlanViewModel>>> GetAllLessonPlanWithPageAsync(ManageLessonPlanSearchRequest request);
        Task<ApiResponse<dynamic>> UpsertAttachmentAsync(ManageLessonPlanAttachmentUpsertRequest request);
        Task<ApiResponse<LessonPlanViewModel>> GetLessonPlanDetailByIdAsync(int lessonPlanId);
        Task<ApiResponse<dynamic>> UpsertLessonPlanCommitAsync(LessonPlanCommentRequest request);
        Task<ApiResponse<List<LessonPlanCommentResponse>>> GetAllCommentListAsync(int companyId, int sessionId, int lessonPlanId);
    }
}
