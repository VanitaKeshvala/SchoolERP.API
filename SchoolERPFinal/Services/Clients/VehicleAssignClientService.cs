using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class VehicleAssignClientService : BaseApiClient, IVehicleAssignClientService
    {
        public VehicleAssignClientService(HttpClient httpClient) : base(httpClient) { }

        public Task<ApiResponse<List<VehicleAssignViewModel>>> GetAllAssignmentsAsync()
            => GetAsync<List<VehicleAssignViewModel>>("api/VehicleAssignApi/GetAllAssignments");

        public Task<ApiResponse<VehicleAssignViewModel>> GetAssignmentByIDAsync(int id)
            => GetAsync<VehicleAssignViewModel>($"api/VehicleAssignApi/GetAssignmentByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertAssignmentsAsync(VehicleAssignUpsertRequest req)
            => PostAsync<dynamic>("api/VehicleAssignApi/UpsertAssignments", req);

        public Task<ApiResponse<dynamic>> DeleteAssignmentAsync(List<int> id)
            => PostAsync<dynamic>($"api/VehicleAssignApi/DeleteAssignment", id);

        public Task<ApiResponse<dynamic>> ToggleAssignmentStatusAsync(int id, bool isActive)
            => PostAsync<dynamic>($"api/VehicleAssignApi/ToggleAssignmentStatus?id={id}&isActive={isActive}", null!);
    }
}
