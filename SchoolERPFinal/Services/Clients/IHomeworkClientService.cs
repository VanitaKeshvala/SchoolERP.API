using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface IHomeworkClientService
    {
        Task<ApiResponse<List<HomeworkViewModel>>> GetAllAsync(bool includeDeleted = false);
        Task<ApiResponse<HomeworkViewModel>> GetByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertAsync(HomeworkUpsertRequest request);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request);
        Task<ApiResponse<PagedResult<HomeworkViewModel>>> GetAllHomeWorkWithPageAsync(SearchRequest request);
    }
}
