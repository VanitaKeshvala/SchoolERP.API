using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface IHomeworkClientService
    {
        Task<ApiResponse<List<HomeworkViewModel>>> GetAllAsync(bool includeDeleted = false);
        Task<ApiResponse<HomeworkViewModel>> GetByIDAsync(int id, int? studentId = null);
        Task<ApiResponse<dynamic>> UpsertAsync(HomeworkUpsertRequest request);
        Task<ApiResponse<List<HomeworkAttachmentViewModel>>> GetAllHomeWorkAttechmentByIdAsync(int homeWorkId);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request);
        Task<ApiResponse<PagedResult<HomeworkViewModel>>> GetAllHomeWorkWithPageAsync(SearchRequest request);
        Task<ApiResponse<dynamic>> UpsertAttachmentAsync(HomeworkAttachmentUpsertRequest request);
        Task<ApiResponse<dynamic>> UpsertSubmissionAsync(HomeworkSubmissionUpsertRequest request);
        Task<ApiResponse<dynamic>> UpsertSubmissionAttachmentAsync(HomeworkSubmissionAttachmentUpsertRequest request);
        Task<ApiResponse<dynamic>> UpsertEvaluateHomeworkAsync(HomeworkSubmissionEvaluateUpsertRequest request);

        Task<ApiResponse<PagedResult<HomeworkSubmissionListDto>>> GetAllHomeWorkSubmissionWithPageAsync(SearchRequest request);
        Task<ApiResponse<List<HomeworkAttachmentViewModel>>> GetAllHomeWorkSubmissionAttechmentByIdAsync(int? submissionID);
    }
}
