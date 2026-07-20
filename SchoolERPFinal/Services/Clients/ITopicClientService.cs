using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public interface ITopicClientService
    {
        Task<ApiResponse<List<TopicViewModel>>> GetAllAsync(int companyId, int sessionId);
        Task<ApiResponse<TopicViewModel>> GetByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertAsync(TopicModelRequest request);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request);
        Task<ApiResponse<PagedResult<TopicViewModel>>> GetAllTopicWithPageAsync(TopicSearchRequest request);
        Task<ApiResponse<List<TopicMap>>> GetAllMapTopicAsync(int companyId, int sessionId, int topicId);
        Task<ApiResponse<PagedResult<LessonSyllabusStatusResponse>>> GetAllTopicSyllaBussStatusWithPageAsync(TopicSearchRequest request);
        Task<ApiResponse<dynamic>> ToggleTopicCompleteStatusAsync(StatusUpdateRequest request);
        Task<ApiResponse<List<TopicDropDwonResponse>>> BindTopicDropDwonListAsync(int lessonMapId);
    }
}
