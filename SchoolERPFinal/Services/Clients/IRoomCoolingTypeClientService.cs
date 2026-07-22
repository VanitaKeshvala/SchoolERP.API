using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public interface IRoomCoolingTypeClientService
    {
        Task<ApiResponse<dynamic>> UpsertRoomCoolingTypeAsync(RoomCoolingTypeListDto request);
        Task<ApiResponse<RoomCoolingType>> GetByIDAsync(int id);
        Task<ApiResponse<List<RoomCoolingType>>> GetAllAsync(int? companyId = null, int? sessionId = null);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request);
        Task<ApiResponse<PagedResult<RoomCoolingType>>> GetAllRoomCoolingTypeWithPageAsync(ClassSearchRequest request);
    }
}
