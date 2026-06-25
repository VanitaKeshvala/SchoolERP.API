using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.API.Interfaces
{
    public interface IDashboardApiService
    {
        Task<List<DashboardModel>> GetAllAsync();
        Task<DashboardModel?> GetByIdAsync(int dashboardId);
        Task<(bool Success, string Message, int DashboardID)> InsertUpdateAsync(DashboardRequestModel request, int userId, string ipAddress);
        Task<(bool Success, string Message)> ToggleStatusAsync(StatusUpdateRequest request);
        Task<(bool Success, string Message)> DeleteMultipleAsync(List<int> dashboardIds, int userId, string ipAddress);
        Task<DashboardModel?> GetByRoleIdAsync(int roleId);
    }
}
