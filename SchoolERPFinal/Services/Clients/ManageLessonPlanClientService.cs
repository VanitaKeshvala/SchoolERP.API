using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public class ManageLessonPlanClientService : BaseApiClient, IManageLessonPlanClientService
    {
        public ManageLessonPlanClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<ApiResponse<ManageLessonPlanViewModel>> GetByIDAsync(int id)
        {
            return await GetAsync<ManageLessonPlanViewModel>($"api/ManageLessonPlanAPI/GetByID?id={id}");
        }
        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids)
        {
            return await PostAsync<dynamic>($"api/ManageLessonPlanAPI/Delete", ids);
        }
        public async Task<ApiResponse<dynamic>> UpsertAsync(ManageLessonPlanRequest request)
        {
            return await PostAsync<dynamic>("api/ManageLessonPlanAPI/Upsert", request);
        }

        public async Task<ApiResponse<PagedResult<ManageLessonPlanViewModel>>> GetAllLessonPlanWithPageAsync(ManageLessonPlanSearchRequest request)
        {
            return await PostAsync<PagedResult<ManageLessonPlanViewModel>>("api/ManageLessonPlanAPI/GetAllLessonPlanWithPage", request);
        }
        public async Task<ApiResponse<dynamic>> UpsertAttachmentAsync(ManageLessonPlanAttachmentUpsertRequest request)
        {
            return await PostAsync<dynamic>("api/ManageLessonPlanAPI/UpsertAttachment", request);
        }
        public async Task<ApiResponse<LessonPlanViewModel>> GetLessonPlanDetailByIdAsync(int lessonPlanId)
        {
            return await GetAsync<LessonPlanViewModel>($"api/ManageLessonPlanAPI/GetLessonPlanDetailById?lessonPlanId={lessonPlanId}");
        }

        public async Task<ApiResponse<dynamic>> UpsertLessonPlanCommitAsync(LessonPlanCommentRequest request)
        {
            return await PostAsync<dynamic>("api/ManageLessonPlanAPI/UpsertLessonPlanCommit", request);
        }

        public async Task<ApiResponse<List<LessonPlanCommentResponse>>> GetAllCommentListAsync(int companyId, int sessionId, int lessonPlanId)
        {
            return await GetAsync<List<LessonPlanCommentResponse>>($"api/ManageLessonPlanAPI/GetAllCommentList?companyId={companyId}&sessionId={sessionId}&lessonPlanId={lessonPlanId}");
        }
    }
}
