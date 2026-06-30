using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface IClassClientService
    {
        Task<ApiResponse<List<MstClassViewModel>>> GetAllAsync(bool includeDeleted = false, int? sessionId = null);
        Task<ApiResponse<MstClassViewModel>> GetByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertAsync(MstClassUpsertRequest request);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request);
        Task<ApiResponse<PagedResult<MstClassViewModel>>> GetAllClassWithPageAsync(ClassSearchRequest request);
    }
}
