using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class HostelClientService : BaseApiClient, IHostelClientService
    {
        public HostelClientService(HttpClient httpClient) : base(httpClient) { }

        public Task<ApiResponse<List<RoomTypeViewModel>>> GetAllRoomTypesAsync(bool includeDeleted = false, int? sessionId = null, int? companyId = null)
            => GetAsync<List<RoomTypeViewModel>>($"api/HostelApi/GetAllRoomTypes?includeDeleted={includeDeleted}&sessionId={sessionId}&companyId={companyId}");

        public Task<ApiResponse<RoomTypeViewModel>> GetRoomTypeByIDAsync(int id)
            => GetAsync<RoomTypeViewModel>($"api/HostelApi/GetRoomTypeByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertRoomTypeAsync(RoomTypeUpsertRequest req)
            => PostAsync<dynamic>("api/HostelApi/UpsertRoomType", req);

        public Task<ApiResponse<dynamic>> DeleteRoomTypeAsync(List<int> id)
            => PostAsync<dynamic>($"api/HostelApi/DeleteRoomType",id);

        public Task<ApiResponse<dynamic>> ToggleRoomTypeStatusAsync(StatusUpdateRequest request)
            => PostAsync<dynamic>($"api/HostelApi/ToggleRoomTypeStatus", request!);

        // Hostel
        public Task<ApiResponse<List<HostelViewModel>>> GetAllHostelsAsync(bool includeDeleted = false, int? sessionId = null,int? companyId=null)
            => GetAsync<List<HostelViewModel>>($"api/HostelApi/GetAllHostels?includeDeleted={includeDeleted}&sessionId={sessionId}&companyId={companyId}");

        public Task<ApiResponse<HostelViewModel>> GetHostelByIDAsync(int id)
            => GetAsync<HostelViewModel>($"api/HostelApi/GetHostelByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertHostelAsync(HostelUpsertRequest req)
            => PostAsync<dynamic>("api/HostelApi/UpsertHostel", req);

        public Task<ApiResponse<dynamic>> DeleteHostelAsync(List<int> id)
            => PostAsync<dynamic>($"api/HostelApi/DeleteHostel", id!);

        public Task<ApiResponse<dynamic>> ToggleHostelStatusAsync(StatusUpdateRequest request)
            => PostAsync<dynamic>($"api/HostelApi/ToggleHostelStatus", request!);

        // Hostel Room
        public Task<ApiResponse<List<HostelRoomViewModel>>> GetAllHostelRoomsAsync(bool includeDeleted = false,int? sessionId=null)
            => GetAsync<List<HostelRoomViewModel>>($"api/HostelApi/GetAllHostelRooms?includeDeleted={includeDeleted}&sessionId={sessionId}");

        public Task<ApiResponse<HostelRoomViewModel>> GetHostelRoomByIDAsync(int id)
            => GetAsync<HostelRoomViewModel>($"api/HostelApi/GetHostelRoomByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertHostelRoomAsync(HostelRoomUpsertRequest req)
            => PostAsync<dynamic>("api/HostelApi/UpsertHostelRoom", req);

        public Task<ApiResponse<dynamic>> DeleteHostelRoomAsync(List<int> id)
            => PostAsync<dynamic>($"api/HostelApi/DeleteHostelRoom", id);

        public Task<ApiResponse<dynamic>> ToggleHostelRoomStatusAsync(StatusUpdateRequest request)
            => PostAsync<dynamic>($"api/HostelApi/ToggleHostelRoomStatus", request);

        public async Task<ApiResponse<PagedResult<RoomTypeViewModel>>> GetAllRoomTypeWithPageAsync(ClassSearchRequest request)
        {
            return await PostAsync<PagedResult<RoomTypeViewModel>>("api/HostelApi/GetAllRoomTypeWithPage", request);
        }

        public async Task<ApiResponse<PagedResult<HostelViewModel>>> GetAllHotelWithPageAsync(HotelSearchRequest request)
        {
            return await PostAsync<PagedResult<HostelViewModel>>("api/HostelApi/GetAllHotelWithPage", request);
        }

        public async Task<ApiResponse<PagedResult<HostelRoomViewModel>>> GetAllHostelRoomWithPageAsync(HotelSearchRequest request)
        {
            return await PostAsync<PagedResult<HostelRoomViewModel>>("api/HostelApi/GetAllHostelRoomlWithPage", request);
        }

        public Task<ApiResponse<List<HostelRoomRateViewModel>>> GetHostelRoomRateByIDAsync(int id)
           => GetAsync<List<HostelRoomRateViewModel>>($"api/HostelApi/GetHostelRoomRateByID/{id}");

        public Task<ApiResponse<List<RoomTypeViewModel>>> GetAllRoomOccupancyByRoomTypesWiseAsync(int roomTypeId)
            => GetAsync<List<RoomTypeViewModel>>($"api/HostelApi/GetAllRoomOccupancyByRoomTypesWise?roomTypeId={roomTypeId}");

        public Task<ApiResponse<List<HostelSummary>>> GetHostelSummary(int hostelId)
            => GetAsync<List<HostelSummary>>($"api/HostelApi/GetHostelSummary?hostelId={hostelId}");

        public async Task<ApiResponse<List<HostelReportResponse>>> GetAllHostelReportWithPageAsync(HotelReportSearchRequest request)
        {
            return await PostAsync<List<HostelReportResponse>>("api/HostelApi/GetAllHostelReportWithPage", request);
        }

    }
}
