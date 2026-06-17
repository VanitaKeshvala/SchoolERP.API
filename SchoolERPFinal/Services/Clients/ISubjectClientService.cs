using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface ISubjectClientService
    {
        Task<ApiResponse<List<MstSubjectViewModel>>> GetAllAsync(bool includeDeleted = false);
        Task<ApiResponse<MstSubjectViewModel>> GetByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertAsync(MstSubjectUpsertRequest request);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(int id, bool isActive);
    }
}
