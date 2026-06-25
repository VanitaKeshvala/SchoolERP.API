using SchoolERP.Shared.Models;
using Common = SchoolERP.Shared.Models.Common;
namespace SchoolERP.API.Interfaces
{
    public interface IAuthServices
    {
        Task<Common.ApiResponse<UserSessionModel?>> LoginAsync(string username, string password);
        Task<DashboardModel?> GetDashboardByIdAsync(int dashboardId);
    }
}
