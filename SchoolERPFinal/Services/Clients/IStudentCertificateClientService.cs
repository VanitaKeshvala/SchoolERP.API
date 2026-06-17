using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchoolERP.Net.Services.Clients
{
    public interface IStudentCertificateClientService
    {
        Task<ApiResponse<List<StudentCertificateViewModel>>> GetAll();
        Task<ApiResponse<StudentCertificateViewModel>> GetByID(int id);
        Task<ApiResponse<int>> Upsert(StudentCertificateUpsertRequest request);
        Task<ApiResponse<int>> Delete(List<int> id);
        Task<ApiResponse<int>> ToggleStatus(int id, bool isActive);
        Task<ApiResponse<string>> Generate(int studentId, int certificateId);
    }
}
