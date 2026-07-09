using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public interface IWeeklyHolidaysSettingClientService
    {
        Task<ApiResponse<dynamic>> UpsertWeeklyHolidaysSettingAsync(WeeklyHolidayBatchRequest request);
        Task<ApiResponse<WeeklyHolidaysSettingModel>> GetByIDAsync(int id);
        Task<ApiResponse<List<WeeklyHolidaysSettingModel>>> GetAllAsync(int? companyId);
        Task<ApiResponse<PagedResult<WeeklyHolidaysSettingModel>>> GetAllWeeklyHolidaysSettingWithPageAsync(WeeklyHolidaysSettingSearchRequest request);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request);
    }
}
