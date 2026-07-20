using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public class LessonPlanClientService:BaseApiClient, ILessonPlanClientService
    {
        public LessonPlanClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<ApiResponse<List<LessonPlanModel>>> GetAllAsync(int companyId, int sessionId)
        {
            return await GetAsync<List<LessonPlanModel>>($"api/LessonPlanAPI/GetAll?companyId={companyId}&sessionId={sessionId}");
        }

        public async Task<ApiResponse<LessonPlanModel>> GetByIDAsync(int id)
        {
            return await GetAsync<LessonPlanModel>($"api/LessonPlanAPI/GetByID?id={id}");
        }

        public async Task<ApiResponse<dynamic>> UpsertAsync(LessonPlanModelRequest request)
        {
            return await PostAsync<dynamic>("api/LessonPlanAPI/Upsert", request);
        }

        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids)
        {
            return await PostAsync<dynamic>($"api/LessonPlanAPI/Delete", ids);
        }

        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request)
        {
            return await PostAsync<dynamic>($"api/LessonPlanAPI/ToggleStatus", request);
        }

        public async Task<ApiResponse<PagedResult<LessonPlanModel>>> GetAllLessonPlanWithPageAsync(LessonPlanSearchRequest request)
        {
            return await PostAsync<PagedResult<LessonPlanModel>>("api/LessonPlanAPI/GetAllLessonPlanWithPage", request);
        }

        public async Task<ApiResponse<List<LessonPlanMap>>> GetAllMapLessonAsync(int companyId, int sessionId, int lessonId)
        {
            return await GetAsync<List<LessonPlanMap>>($"api/LessonPlanAPI/GetAllMapLesson?companyId={companyId}&sessionId={sessionId}&lessonId={lessonId}");
        }

        public async Task<ApiResponse<List<LessonDropDwonResponse>>> BindLessonDropDwonListAsync(LessonDropDwonReq req)
        {
            return await PostAsync<List<LessonDropDwonResponse>>("api/LessonPlanAPI/BindLessonDropDwonList", req);
        }
    }
}
