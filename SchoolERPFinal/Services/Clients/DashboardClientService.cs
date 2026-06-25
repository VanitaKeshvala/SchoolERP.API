using static System.Net.WebRequestMethods;
using System.Text;
using SchoolERP.Shared.Models.Common;
using Common = SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;
using System.ComponentModel.Design;

namespace SchoolERP.Net.Services.Clients
{ 
    public class DashboardClientService: BaseApiClient, IDashboardClientService
    {
        public DashboardClientService(HttpClient httpClient) : base(httpClient)
        {
        }
        // ── GET ALL ───────────────────────────────────────────────
        public async Task<Common.ApiResponse<List<DashboardModel>>> GetAllAsync()
        {
            return await GetAsync<List<DashboardModel>>("api/DashboardApi/");
        }

        // ── GET BY ID ─────────────────────────────────────────────
        public async Task<Common.ApiResponse<DashboardModel?>> GetByIdAsync(int dashboardId)
        {
            return await GetAsync<DashboardModel>($"api/DashboardApi/{dashboardId}");
        }

        // ── SAVE (INSERT / UPDATE) ────────────────────────────────
        public async Task<Common.ApiResponse<object>> SaveAsync(DashboardRequestModel request)
        {
            return await PostAsync<dynamic>($"api/DashboardApi/Save", request);
        }

        // ── TOGGLE STATUS ─────────────────────────────────────────
        public async Task<Common.ApiResponse<object>> ToggleStatusAsync(StatusUpdateRequest request)
        {
            return await PostAsync<dynamic>("api/DashboardApi/ToggleStatus", request);
        }

        // ── DELETE MULTIPLE ───────────────────────────────────────
        public async Task<Common.ApiResponse<object>> DeleteMultipleAsync(List<int> id)
        {
            return await PostAsync<dynamic>("api/DashboardApi/DeleteMultiple", id);
        }
        // ── GET BY ID ─────────────────────────────────────────────
        public async Task<Common.ApiResponse<DashboardModel?>> GetByRoleIdAsync(int roleId)
        {
            return await GetAsync<DashboardModel>($"api/DashboardApi/GetByRoleId/{roleId}");
        }
    }
}
