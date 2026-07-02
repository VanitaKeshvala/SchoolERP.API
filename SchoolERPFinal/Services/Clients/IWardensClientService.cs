using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public interface IWardensClientService
    {
        Task<ApiResponse<WardenSaveResponse>> UpsertWardensAsync(WardensRequestModel request);
        Task<ApiResponse<WardensModel>> GetByIDAsync(int id);
        Task<ApiResponse<List<WardensModel>>> GetAllAsync(int? companyId = null, int? sessionId = null);
        Task<ApiResponse<PagedResult<WardensModel>>> GetAllWardensWithPageAsync(WardensSearchRequest request);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(WardensRequestModel request);
        Task<ApiResponse<PagedResult<WardensModel>>> UpdateWardenProfileAsync(WardenProfileRequest request);
    }
}
