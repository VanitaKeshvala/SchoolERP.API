using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface IVehicleClientService
    {
        Task<ApiResponse<List<VehicleViewModel>>> GetAllVehiclesAsync();
        Task<ApiResponse<VehicleViewModel>> GetVehicleByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertVehicleAsync(VehicleFormModel form);
        Task<ApiResponse<dynamic>> DeleteVehicleAsync(List<int> id);
        Task<ApiResponse<dynamic>> ToggleVehicleStatusAsync(int id, bool isActive);
    }
}
