using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public interface IHostelTypeClientService
    {
        Task<ApiResponse<dynamic>> UpsertHostelTypeAsync(HostelTypeUpsertRequest req);
        Task<ApiResponse<HostelTypeModel>> GetByIDAsync(int id);
        Task<ApiResponse<List<HostelTypeModel>>> GetAllAsync(int? companyId = null, int? sessionId = null);
        Task<ApiResponse<PagedResult<HostelTypeModel>>> GetAllHostelTypeWithPageAsync(HostelTypeSearchRequest request);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request);
    }
}
