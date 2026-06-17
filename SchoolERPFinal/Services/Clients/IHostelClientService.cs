using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface IHostelClientService
    {
        Task<ApiResponse<List<RoomTypeViewModel>>> GetAllRoomTypesAsync(bool includeDeleted = false);
        Task<ApiResponse<RoomTypeViewModel>> GetRoomTypeByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertRoomTypeAsync(RoomTypeUpsertRequest req);
        Task<ApiResponse<dynamic>> DeleteRoomTypeAsync(List<int> id);
        Task<ApiResponse<dynamic>> ToggleRoomTypeStatusAsync(int id, bool isActive);

        // Hostel
        Task<ApiResponse<List<HostelViewModel>>> GetAllHostelsAsync(bool includeDeleted = false);
        Task<ApiResponse<HostelViewModel>> GetHostelByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertHostelAsync(HostelUpsertRequest req);
        Task<ApiResponse<dynamic>> DeleteHostelAsync(List<int> id);
        Task<ApiResponse<dynamic>> ToggleHostelStatusAsync(int id, bool isActive);

        // Hostel Room
        Task<ApiResponse<List<HostelRoomViewModel>>> GetAllHostelRoomsAsync(bool includeDeleted = false);
        Task<ApiResponse<HostelRoomViewModel>> GetHostelRoomByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertHostelRoomAsync(HostelRoomUpsertRequest req);
        Task<ApiResponse<dynamic>> DeleteHostelRoomAsync(List<int> id);
        Task<ApiResponse<dynamic>> ToggleHostelRoomStatusAsync(int id, bool isActive);
    }
}
