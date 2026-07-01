using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public class HolidayTypeClientService:BaseApiClient, IHolidayTypeClientService
    {
        public HolidayTypeClientService(HttpClient client) : base(client) { }

        public async Task<ApiResponse<dynamic>> UpsertHolidayTypeAsync(HolidayTypeRequest request)
        {
            return await PostAsync<dynamic>("api/HolidayType/Save", request);
        }
        public async Task<ApiResponse<HolidayType>> GetByIDAsync(int id)
        {
            return await GetAsync<HolidayType>($"api/HolidayType/GetById/{id}");
        }

        public async Task<ApiResponse<List<HolidayType>>> GetAllAsync(int? companyId = null, int? sessionId = null)
        {
            return await GetAsync<List<HolidayType>>($"api/HolidayType/GetAll?companyId={companyId}&sessionId={sessionId}");
        }

        public async Task<ApiResponse<PagedResult<HolidayType>>> GetAllHostelTypeWithPageAsync(HostelTypeSearchRequest request)
        {
            return await PostAsync<PagedResult<HolidayType>>("api/HolidayType/GetAllHolidayTypeWithPage", request);
        }

        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids)
        {
            return await PostAsync<dynamic>($"api/HolidayType/Delete", ids);
        }

        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request)
        {
            return await PostAsync<dynamic>($"api/HolidayType/ToggleStatus", request);
        }
    }
}
