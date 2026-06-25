using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class PickupPointClientService : BaseApiClient, IPickupPointClientService
    {
        public PickupPointClientService(HttpClient httpClient) : base(httpClient) { }

        public Task<ApiResponse<List<PickupPointViewModel>>> GetAllPickupPointsAsync()
            => GetAsync<List<PickupPointViewModel>>("api/PickupPointApi/GetAllPickupPoints");

        public Task<ApiResponse<PickupPointViewModel>> GetPickupPointByIDAsync(int id)
            => GetAsync<PickupPointViewModel>($"api/PickupPointApi/GetPickupPointByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertPickupPointAsync(PickupPointUpsertRequest req)
            => PostAsync<dynamic>("api/PickupPointApi/UpsertPickupPoint", req);

        public Task<ApiResponse<dynamic>> DeletePickupPointAsync(List<int> id)
            => PostAsync<dynamic>($"api/PickupPointApi/DeletePickupPoint", id);

        public Task<ApiResponse<dynamic>> TogglePickupPointStatusAsync(int id, bool isActive)
            => PostAsync<dynamic>($"api/PickupPointApi/TogglePickupPointStatus?id={id}&isActive={isActive}", null!);
    }
}
