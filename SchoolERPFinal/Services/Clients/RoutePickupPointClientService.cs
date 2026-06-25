using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class RoutePickupPointClientService : BaseApiClient, IRoutePickupPointClientService
    {
        public RoutePickupPointClientService(HttpClient httpClient) : base(httpClient) { }

        public Task<ApiResponse<List<RoutePickupPointViewModel>>> GetAllRoutePickupPointsAsync()
            => GetAsync<List<RoutePickupPointViewModel>>("api/RoutePickupPointsApi/GetAllRoutePickupPoints");

        public Task<ApiResponse<RoutePickupPointViewModel>> GetRoutePickupPointByIDAsync(int id)
            => GetAsync<RoutePickupPointViewModel>($"api/RoutePickupPointsApi/GetRoutePickupPointByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertRoutePickupPointAsync(RoutePickupPointUpsertRequest req)
            => PostAsync<dynamic>("api/RoutePickupPointsApi/UpsertRoutePickupPoint", req);

        public Task<ApiResponse<dynamic>> DeleteRoutePickupPointAsync(List<int> id)
            => PostAsync<dynamic>($"api/RoutePickupPointsApi/DeleteRoutePickupPoint", id);

        public Task<ApiResponse<dynamic>> ToggleRoutePickupPointStatusAsync(int id, bool isActive)
            => PostAsync<dynamic>($"api/RoutePickupPointsApi/ToggleRoutePickupPointStatus?id={id}&isActive={isActive}", null!);
    }
}
