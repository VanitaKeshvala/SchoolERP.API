using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface ISectionClientService
    {
        Task<ApiResponse<List<MstSectionViewModel>>> GetAllAsync(bool includeDeleted = false);
        Task<ApiResponse<List<MstSectionViewModel>>> GetByClassAsync(int classId);
        Task<ApiResponse<MstSectionViewModel>> GetByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertAsync(MstSectionUpsertRequest request);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(int id, bool isActive);
    }
}
