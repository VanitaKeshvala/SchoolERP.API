using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;

using Common = SchoolERP.Shared.Models.Common;
namespace SchoolERP.Net.Services.Clients
{
    public interface IDashboardClientService
    {
        Task<Common.ApiResponse<List<DashboardModel>>> GetAllAsync();
        Task<Common.ApiResponse<DashboardModel?>> GetByIdAsync(int dashboardId);
        Task<Common.ApiResponse<object>> SaveAsync(DashboardRequestModel request);
        Task<Common.ApiResponse<object>> ToggleStatusAsync(StatusUpdateRequest request);
        Task<Common.ApiResponse<object>> DeleteMultipleAsync(List<int> id);
        Task<Common.ApiResponse<DashboardModel?>> GetByRoleIdAsync(int roleId);
    }
}
