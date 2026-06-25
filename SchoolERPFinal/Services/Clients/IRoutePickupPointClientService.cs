using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface IRoutePickupPointClientService
    {
        Task<ApiResponse<List<RoutePickupPointViewModel>>> GetAllRoutePickupPointsAsync();
        Task<ApiResponse<RoutePickupPointViewModel>> GetRoutePickupPointByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertRoutePickupPointAsync(RoutePickupPointUpsertRequest req);
        Task<ApiResponse<dynamic>> DeleteRoutePickupPointAsync(List<int> id);
        Task<ApiResponse<dynamic>> ToggleRoutePickupPointStatusAsync(int id, bool isActive);
    }
}
