using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface IHostelClientService
    {
        Task<ApiResponse<List<RoomTypeViewModel>>> GetAllRoomTypesAsync(bool includeDeleted = false, int? sessionId = null, int? companyId = null);
        Task<ApiResponse<RoomTypeViewModel>> GetRoomTypeByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertRoomTypeAsync(RoomTypeUpsertRequest req);
        Task<ApiResponse<dynamic>> DeleteRoomTypeAsync(List<int> id);
        Task<ApiResponse<dynamic>> ToggleRoomTypeStatusAsync(StatusUpdateRequest request);

        // Hostel
        Task<ApiResponse<List<HostelViewModel>>> GetAllHostelsAsync(bool includeDeleted = false, int? sessionId = null,int ? companyId = null);
        Task<ApiResponse<HostelViewModel>> GetHostelByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertHostelAsync(HostelUpsertRequest req);
        Task<ApiResponse<dynamic>> DeleteHostelAsync(List<int> id);
        Task<ApiResponse<dynamic>> ToggleHostelStatusAsync(StatusUpdateRequest request);

        // Hostel Room
        Task<ApiResponse<List<HostelRoomViewModel>>> GetAllHostelRoomsAsync(bool includeDeleted = false, int? sessionId = null);
        Task<ApiResponse<HostelRoomViewModel>> GetHostelRoomByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertHostelRoomAsync(HostelRoomUpsertRequest req);
        Task<ApiResponse<dynamic>> DeleteHostelRoomAsync(List<int> id);
        Task<ApiResponse<dynamic>> ToggleHostelRoomStatusAsync(StatusUpdateRequest request);
        Task<ApiResponse<PagedResult<RoomTypeViewModel>>> GetAllRoomTypeWithPageAsync(ClassSearchRequest request);
        Task<ApiResponse<PagedResult<HostelViewModel>>> GetAllHotelWithPageAsync(HotelSearchRequest request);

        Task<ApiResponse<PagedResult<HostelRoomViewModel>>> GetAllHostelRoomWithPageAsync(HotelSearchRequest request);
    }
}
