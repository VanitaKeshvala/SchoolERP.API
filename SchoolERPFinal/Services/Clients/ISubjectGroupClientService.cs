using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface ISubjectGroupClientService
    {
        Task<ApiResponse<List<MstSubjectGroupViewModel>>> GetAllAsync(bool includeDeleted = false);
        Task<ApiResponse<MstSubjectGroupViewModel>> GetByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertAsync(MstSubjectGroupUpsertRequest request);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(int id, bool isActive);
    }
}
