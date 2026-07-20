using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public class TopicClientService : BaseApiClient, ITopicClientService
    {
        public TopicClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<ApiResponse<List<TopicViewModel>>> GetAllAsync(int companyId, int sessionId)
        {
            return await GetAsync<List<TopicViewModel>>($"api/TopicAPI/GetAll?companyId={companyId}&sessionId={sessionId}");
        }

        public async Task<ApiResponse<TopicViewModel>> GetByIDAsync(int id)
        {
            return await GetAsync<TopicViewModel>($"api/TopicAPI/GetByID?id={id}");
        }

        public async Task<ApiResponse<dynamic>> UpsertAsync(TopicModelRequest request)
        {
            return await PostAsync<dynamic>("api/TopicAPI/Upsert", request);
        }

        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids)
        {
            return await PostAsync<dynamic>($"api/TopicAPI/Delete", ids);
        }

        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request)
        {
            return await PostAsync<dynamic>($"api/TopicAPI/ToggleStatus", request);
        }

        public async Task<ApiResponse<PagedResult<TopicViewModel>>> GetAllTopicWithPageAsync(TopicSearchRequest request)
        {
            return await PostAsync<PagedResult<TopicViewModel>>("api/TopicAPI/GetAllTopicWithPage", request);
        }

        public async Task<ApiResponse<List<TopicMap>>> GetAllMapTopicAsync(int companyId, int sessionId, int topicId)
        {
            return await GetAsync<List<TopicMap>>($"api/TopicAPI/GetAllMapTopic?companyId={companyId}&sessionId={sessionId}&topicId={topicId}");
        }

        public async Task<ApiResponse<PagedResult<LessonSyllabusStatusResponse>>> GetAllTopicSyllaBussStatusWithPageAsync(TopicSearchRequest request)
        {
            return await PostAsync<PagedResult<LessonSyllabusStatusResponse>>("api/TopicAPI/GetAllTopicSyllaBussStatusWithPage", request);
        }

        public async Task<ApiResponse<dynamic>> ToggleTopicCompleteStatusAsync(StatusUpdateRequest request)
        {
            return await PostAsync<dynamic>($"api/TopicAPI/ToggleTopicCompleteStatus", request);
        }

        public async Task<ApiResponse<List<TopicDropDwonResponse>>> BindTopicDropDwonListAsync(int lessonMapId)
        {
            return await GetAsync<List<TopicDropDwonResponse>>($"api/TopicAPI/BindTopicDropDwonList?lessonMapId={lessonMapId}");
        }
    }
}
