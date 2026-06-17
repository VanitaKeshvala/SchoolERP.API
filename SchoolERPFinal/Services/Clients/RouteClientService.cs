using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class RouteClientService : BaseApiClient, IRouteClientService
    {
        public RouteClientService(HttpClient httpClient) : base(httpClient) { }

        public Task<ApiResponse<List<RouteViewModel>>> GetAllRoutesAsync()
            => GetAsync<List<RouteViewModel>>("api/RoutesApi/GetAllRoutes");

        public Task<ApiResponse<RouteViewModel>> GetRouteByIDAsync(int id)
            => GetAsync<RouteViewModel>($"api/RoutesApi/GetRouteByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertRouteAsync(RouteUpsertRequest req)
            => PostAsync<dynamic>("api/RoutesApi/UpsertRoute", req);

        public Task<ApiResponse<dynamic>> DeleteRouteAsync(List<int> id)
            => PostAsync<dynamic>($"api/RoutesApi/DeleteRoute", id);

        public Task<ApiResponse<dynamic>> ToggleRouteStatusAsync(int id, bool isActive)
            => PostAsync<dynamic>($"api/RoutesApi/ToggleRouteStatus?id={id}&isActive={isActive}", null!);
    }
}
