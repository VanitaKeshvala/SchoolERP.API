using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public interface IHolidayClientService
    {
        Task<ApiResponse<dynamic>> UpsertHolidayAsync(HolidayRequestModel request);
        Task<ApiResponse<HolidayModel>> GetByIDAsync(int id);
        Task<ApiResponse<List<HolidayModel>>> GetAllAsync(int? companyId = null, int? sessionId = null);
        Task<ApiResponse<PagedResult<HolidayModel>>> GetAllHostelWithPageAsync(HostelTypeSearchRequest request);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request);
    }
}
