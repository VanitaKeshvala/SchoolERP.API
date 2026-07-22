using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class RoomCoolingTypeClientService:BaseApiClient, IRoomCoolingTypeClientService
    {
        public RoomCoolingTypeClientService(HttpClient client) : base(client) { }

        public async Task<ApiResponse<dynamic>> UpsertRoomCoolingTypeAsync(RoomCoolingTypeListDto request)
        {
            return await PostAsync<dynamic>("api/RoomCoolingTypeAPI/Save", request);
        }
        public async Task<ApiResponse<RoomCoolingType>> GetByIDAsync(int id)
        {
            return await GetAsync<RoomCoolingType>($"api/RoomCoolingTypeAPI/GetById/{id}");
        }

        public async Task<ApiResponse<List<RoomCoolingType>>> GetAllAsync(int? companyId = null, int? sessionId = null)
        {
            return await GetAsync<List<RoomCoolingType>>($"api/RoomCoolingTypeAPI/GetAll?companyId={companyId}&sessionId={sessionId}");
        }

        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids)
        {
            return await PostAsync<dynamic>($"api/RoomCoolingTypeAPI/Delete", ids);
        }

        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request)
        {
            return await PostAsync<dynamic>($"api/RoomCoolingTypeAPI/ToggleStatus", request);
        }

        public async Task<ApiResponse<PagedResult<RoomCoolingType>>> GetAllRoomCoolingTypeWithPageAsync(ClassSearchRequest request)
        {
            return await PostAsync<PagedResult<RoomCoolingType>>("api/RoomCoolingTypeAPI/GetAllRoomCoolingTypeWithPage", request);
        }
    }
}


