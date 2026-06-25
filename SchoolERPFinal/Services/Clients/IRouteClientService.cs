using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface IRouteClientService
    {
        Task<ApiResponse<List<RouteViewModel>>> GetAllRoutesAsync();
        Task<ApiResponse<RouteViewModel>> GetRouteByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertRouteAsync(RouteUpsertRequest req);
        Task<ApiResponse<dynamic>> DeleteRouteAsync(List<int> id);
        Task<ApiResponse<dynamic>> ToggleRouteStatusAsync(int id, bool isActive);
    }
}
