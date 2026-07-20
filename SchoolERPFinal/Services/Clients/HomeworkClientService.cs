using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class HomeworkClientService : BaseApiClient, IHomeworkClientService
    {
        public HomeworkClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<ApiResponse<List<HomeworkViewModel>>> GetAllAsync(bool includeDeleted = false)
        {
            return await GetAsync<List<HomeworkViewModel>>($"api/HomeworkApi/GetAll?includeDeleted={includeDeleted}");
        }

        public async Task<ApiResponse<HomeworkViewModel>> GetByIDAsync(int id, int? studentId = null)
        {
            return await GetAsync<HomeworkViewModel>($"api/HomeworkApi/GetByID?id={id}&studentId={studentId}");
        }

        public async Task<ApiResponse<dynamic>> UpsertAsync(HomeworkUpsertRequest request)
        {
            return await PostAsync<dynamic>("api/HomeworkApi/Upsert", request);
        }

        public async Task<ApiResponse<dynamic>> UpsertAttachmentAsync(HomeworkAttachmentUpsertRequest request)
        {
            return await PostAsync<dynamic>("api/HomeworkApi/UpsertAttachment", request);
        }

        public async Task<ApiResponse<List<HomeworkAttachmentViewModel>>> GetAllHomeWorkAttechmentByIdAsync(int homeWorkId)
        {
            return await GetAsync<List<HomeworkAttachmentViewModel>>($"api/HomeworkApi/GetAllHomeWorkAttechmentById?homeWorkId={homeWorkId}");
        }

        //public async Task<ApiResponse<dynamic>> DeleteAsync(int id)
        //{
        //    return await PostAsync<dynamic>($"api/HomeworkApi/Delete/{id}", null!);
        //}


        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids)
        {
            return await PostAsync<dynamic>($"api/HomeworkApi/Delete", ids);
        }

        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request)
        {
            return await PostAsync<dynamic>($"api/HomeworkApi/ToggleStatus", request);
        }

        public async Task<ApiResponse<PagedResult<HomeworkViewModel>>> GetAllHomeWorkWithPageAsync(SearchRequest request)
        {
            return await PostAsync<PagedResult<HomeworkViewModel>>("api/HomeworkApi/GetAllHomeWorkWithPage", request);
        }

        public async Task<ApiResponse<dynamic>> UpsertSubmissionAsync(HomeworkSubmissionUpsertRequest request)
        {
            return await PostAsync<dynamic>("api/HomeworkApi/UpsertSubmission", request);
        }

        public async Task<ApiResponse<dynamic>> UpsertSubmissionAttachmentAsync(HomeworkSubmissionAttachmentUpsertRequest request)
        {
            return await PostAsync<dynamic>("api/HomeworkApi/UpsertSubmissionAttachment", request);
        }

        public async Task<ApiResponse<dynamic>> UpsertEvaluateHomeworkAsync(HomeworkSubmissionEvaluateUpsertRequest request)
        {
            return await PostAsync<dynamic>("api/HomeworkApi/UpsertEvaluateHomework", request);
        }

        public async Task<ApiResponse<PagedResult<HomeworkSubmissionListDto>>> GetAllHomeWorkSubmissionWithPageAsync(SearchRequest request)
        {
            return await PostAsync<PagedResult<HomeworkSubmissionListDto>>("api/HomeworkApi/GetAllHomeWorkSubmissionWithPage", request);
        }

        public async Task<ApiResponse<List<HomeworkAttachmentViewModel>>> GetAllHomeWorkSubmissionAttechmentByIdAsync(int? submissionID)
        {
            return await GetAsync<List<HomeworkAttachmentViewModel>>($"api/HomeworkApi/GetAllHomeWorkSubmissionAttechmentById?submissionID={submissionID}");
        }
    }
}
