using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface IVehicleAssignClientService
    {
        Task<ApiResponse<List<VehicleAssignViewModel>>> GetAllAssignmentsAsync();
        Task<ApiResponse<VehicleAssignViewModel>> GetAssignmentByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertAssignmentsAsync(VehicleAssignUpsertRequest req);
        Task<ApiResponse<dynamic>> DeleteAssignmentAsync(List<int> id);
        Task<ApiResponse<dynamic>> ToggleAssignmentStatusAsync(int id, bool isActive);
    }
}
