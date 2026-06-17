using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class HostelClientService : BaseApiClient, IHostelClientService
    {
        public HostelClientService(HttpClient httpClient) : base(httpClient) { }

        public Task<ApiResponse<List<RoomTypeViewModel>>> GetAllRoomTypesAsync(bool includeDeleted = false)
            => GetAsync<List<RoomTypeViewModel>>($"api/HostelApi/GetAllRoomTypes?includeDeleted={includeDeleted}");

        public Task<ApiResponse<RoomTypeViewModel>> GetRoomTypeByIDAsync(int id)
            => GetAsync<RoomTypeViewModel>($"api/HostelApi/GetRoomTypeByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertRoomTypeAsync(RoomTypeUpsertRequest req)
            => PostAsync<dynamic>("api/HostelApi/UpsertRoomType", req);

        public Task<ApiResponse<dynamic>> DeleteRoomTypeAsync(List<int> id)
            => PostAsync<dynamic>($"api/HostelApi/DeleteRoomType",id);

        public Task<ApiResponse<dynamic>> ToggleRoomTypeStatusAsync(int id, bool isActive)
            => PostAsync<dynamic>($"api/HostelApi/ToggleRoomTypeStatus?id={id}&isActive={isActive}", null!);

        // Hostel
        public Task<ApiResponse<List<HostelViewModel>>> GetAllHostelsAsync(bool includeDeleted = false)
            => GetAsync<List<HostelViewModel>>($"api/HostelApi/GetAllHostels?includeDeleted={includeDeleted}");

        public Task<ApiResponse<HostelViewModel>> GetHostelByIDAsync(int id)
            => GetAsync<HostelViewModel>($"api/HostelApi/GetHostelByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertHostelAsync(HostelUpsertRequest req)
            => PostAsync<dynamic>("api/HostelApi/UpsertHostel", req);

        public Task<ApiResponse<dynamic>> DeleteHostelAsync(List<int> id)
            => PostAsync<dynamic>($"api/HostelApi/DeleteHostel", id!);

        public Task<ApiResponse<dynamic>> ToggleHostelStatusAsync(int id, bool isActive)
            => PostAsync<dynamic>($"api/HostelApi/ToggleHostelStatus?id={id}&isActive={isActive}", null!);

        // Hostel Room
        public Task<ApiResponse<List<HostelRoomViewModel>>> GetAllHostelRoomsAsync(bool includeDeleted = false)
            => GetAsync<List<HostelRoomViewModel>>($"api/HostelApi/GetAllHostelRooms?includeDeleted={includeDeleted}");

        public Task<ApiResponse<HostelRoomViewModel>> GetHostelRoomByIDAsync(int id)
            => GetAsync<HostelRoomViewModel>($"api/HostelApi/GetHostelRoomByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertHostelRoomAsync(HostelRoomUpsertRequest req)
            => PostAsync<dynamic>("api/HostelApi/UpsertHostelRoom", req);

        public Task<ApiResponse<dynamic>> DeleteHostelRoomAsync(List<int> id)
            => PostAsync<dynamic>($"api/HostelApi/DeleteHostelRoom", id);

        public Task<ApiResponse<dynamic>> ToggleHostelRoomStatusAsync(int id, bool isActive)
            => PostAsync<dynamic>($"api/HostelApi/ToggleHostelRoomStatus?id={id}&isActive={isActive}", null!);
    }
}
