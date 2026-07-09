using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public class WeeklyHolidaysSettingClientService:BaseApiClient, IWeeklyHolidaysSettingClientService
    {
        public WeeklyHolidaysSettingClientService(HttpClient client) : base(client) { }

        public async Task<ApiResponse<dynamic>> UpsertWeeklyHolidaysSettingAsync(WeeklyHolidayBatchRequest request)
        {
            return await PostAsync<dynamic>("api/WeeklyHolidaysSettingAPI/Save", request);
        }
        public async Task<ApiResponse<WeeklyHolidaysSettingModel>> GetByIDAsync(int id)
        {
            return await GetAsync<WeeklyHolidaysSettingModel>($"api/WeeklyHolidaysSettingAPI/GetById/{id}");
        }

        public async Task<ApiResponse<List<WeeklyHolidaysSettingModel>>> GetAllAsync(int? companyId)
        {
            return await GetAsync<List<WeeklyHolidaysSettingModel>>($"api/WeeklyHolidaysSettingAPI/GetAll?companyId={companyId}");
        }

        public async Task<ApiResponse<PagedResult<WeeklyHolidaysSettingModel>>> GetAllWeeklyHolidaysSettingWithPageAsync(WeeklyHolidaysSettingSearchRequest request)
        {
            return await PostAsync<PagedResult<WeeklyHolidaysSettingModel>>("api/WeeklyHolidaysSettingAPI/GetAllWeeklyHolidaysSettingWithPage", request);
        }
        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids)
        {
            return await PostAsync<dynamic>($"api/WeeklyHolidaysSettingAPI/Delete", ids);
        }

        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request)
        {
            return await PostAsync<dynamic>($"api/WeeklyHolidaysSettingAPI/ToggleStatus", request);
        }
    }
}
