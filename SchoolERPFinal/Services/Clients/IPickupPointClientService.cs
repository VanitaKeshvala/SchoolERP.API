using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface IPickupPointClientService
    {
        Task<ApiResponse<List<PickupPointViewModel>>> GetAllPickupPointsAsync();
        Task<ApiResponse<PickupPointViewModel>> GetPickupPointByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertPickupPointAsync(PickupPointUpsertRequest req);
        Task<ApiResponse<dynamic>> DeletePickupPointAsync(List<int> id);
        Task<ApiResponse<dynamic>> TogglePickupPointStatusAsync(int id, bool isActive);
    }
}
