using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface IHomeworkClientService
    {
        Task<ApiResponse<List<HomeworkViewModel>>> GetAllAsync(bool includeDeleted = false);
        Task<ApiResponse<HomeworkViewModel>> GetByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertAsync(HomeworkUpsertRequest request);
        Task<ApiResponse<dynamic>> DeleteAsync(int id);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(int id, bool isActive);
    }
}
