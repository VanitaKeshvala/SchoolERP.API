using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface IStudentIDCardClientService
    {
        Task<ApiResponse<List<StudentIDCardViewModel>>> GetAll();
        Task<ApiResponse<StudentIDCardViewModel>> GetByID(int id);
        Task<ApiResponse<int>> Upsert(StudentIDCardUpsertRequest request);
        Task<ApiResponse<int>> Delete(List<int> id);
        Task<ApiResponse<int>> ToggleStatus(int id, bool isActive);
    }
}
