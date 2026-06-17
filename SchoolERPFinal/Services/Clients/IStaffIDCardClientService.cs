using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface IStaffIDCardClientService
    {
        Task<ApiResponse<List<StaffIDCardViewModel>>> GetAll();
        Task<ApiResponse<StaffIDCardViewModel>> GetByID(int id);
        Task<ApiResponse<int>> Upsert(StaffIDCardUpsertRequest request);
        Task<ApiResponse<int>> Delete(List<int> id);
        Task<ApiResponse<int>> ToggleStatus(int id, bool isActive);
    }
}
