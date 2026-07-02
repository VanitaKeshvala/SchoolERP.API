using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public class HolidayClientService:BaseApiClient, IHolidayClientService
    {
        public HolidayClientService(HttpClient client) : base(client) { }

        public async Task<ApiResponse<dynamic>> UpsertHolidayAsync(HolidayRequestModel request)
        {
            return await PostAsync<dynamic>("api/HolidayAPI/Save", request);
        }
        public async Task<ApiResponse<HolidayModel>> GetByIDAsync(int id)
        {
            return await GetAsync<HolidayModel>($"api/HolidayAPI/GetById/{id}");
        }

        public async Task<ApiResponse<List<HolidayModel>>> GetAllAsync(int? companyId = null, int? sessionId = null)
        {
            return await GetAsync<List<HolidayModel>>($"api/HolidayAPI/GetAll?companyId={companyId}&sessionId={sessionId}");
        }

        public async Task<ApiResponse<PagedResult<HolidayModel>>> GetAllHostelWithPageAsync(HostelTypeSearchRequest request)
        {
            return await PostAsync<PagedResult<HolidayModel>>("api/HolidayAPI/GetAllHolidayWithPage", request);
        }

        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids)
        {
            return await PostAsync<dynamic>($"api/HolidayAPI/Delete", ids);
        }

        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request)
        {
            return await PostAsync<dynamic>($"api/HolidayAPI/ToggleStatus", request);
        }
    }
}
