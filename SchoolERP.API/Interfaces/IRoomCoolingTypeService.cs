using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.API.Interfaces
{
    public interface IRoomCoolingTypeService
    {
        Task<ApiResponse> UpsertRoomCoolingTypeAsync(RoomCoolingTypeListDto model);
        Task<List<RoomCoolingType>> GetAllRoomCoolingTypeAsync(int companyId, int sessionId, bool includeDeleted = false);
        Task<RoomCoolingType?> GetRoomCoolingTypeByIdAsync(int roomCoolingTypeId);
        (bool success, string message) DeleteRoomCoolingType(List<int> ids, int userId);
        (bool success, string message) ToggleRoomCoolingTypeStatus(StatusUpdateRequest request);
        Task<PagedResult<RoomCoolingType>> GetAllRoomCoolingTypeWithPage(ClassSearchRequest req);
    }
}
