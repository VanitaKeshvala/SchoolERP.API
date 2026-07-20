using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class DailyAssignmentClientService:BaseApiClient,IDailyAssignmentClientService
    {
        public DailyAssignmentClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<ApiResponse<DailyAssignmentModel>> GetByIDAsync(int assignmentID)
        {
            return await GetAsync<DailyAssignmentModel>($"api/DailyAssignmentAPI/GetByID?assignmentID={assignmentID}");
        }

        public async Task<ApiResponse<dynamic>> UpsertAsync(AssignmentUpsertRequest request)
        {
            return await PostAsync<dynamic>("api/DailyAssignmentAPI/Upsert", request);
        }

        public async Task<ApiResponse<dynamic>> UpsertAttachmentAsync(AssignmentAttachmentUpsertRequest request)
        {
            return await PostAsync<dynamic>("api/DailyAssignmentAPI/UpsertAttachment", request);
        }

        public async Task<ApiResponse<PagedResult<DailyAssignmentModel>>> GetAllDailyAssignmentWithPageAsync(DailyAssignmentSearchRequest request)
        {
            return await PostAsync<PagedResult<DailyAssignmentModel>>("api/DailyAssignmentAPI/GetAllDailyAssignmentWithPage", request);
        }

        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids)
        {
            return await PostAsync<dynamic>($"api/DailyAssignmentAPI/Delete", ids);
        }
    }
}
