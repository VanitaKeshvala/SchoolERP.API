using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public interface ILessonPlanClientService
    {
        Task<ApiResponse<List<LessonPlanModel>>> GetAllAsync(int companyId, int sessionId);
        Task<ApiResponse<LessonPlanModel>> GetByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertAsync(LessonPlanModelRequest request);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request);
        Task<ApiResponse<PagedResult<LessonPlanModel>>> GetAllLessonPlanWithPageAsync(LessonPlanSearchRequest request);
        Task<ApiResponse<List<LessonPlanMap>>> GetAllMapLessonAsync(int companyId, int sessionId, int lessonId);
        Task<ApiResponse<List<LessonDropDwonResponse>>> BindLessonDropDwonListAsync(LessonDropDwonReq req);
    }
}
