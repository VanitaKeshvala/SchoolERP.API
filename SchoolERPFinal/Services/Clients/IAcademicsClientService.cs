using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface IAcademicsClientService
    {
        Task<ApiResponse<List<TimeTableViewModel>>> GetTimeTableByClassAsync(int classId, int sectionId);
        Task<ApiResponse<List<TimeTableViewModel>>> GetTimeTableByStaffAsync(int staffId);
        Task<ApiResponse<dynamic>> UpsertTimeTableAsync(TimeTableUpsertRequest req);
        Task<ApiResponse<dynamic>> DeleteTimeTableSlotAsync(int id);

        Task<ApiResponse<List<ClassTeacherViewModel>>> GetAllClassTeachersAsync(
            List<int>? classIds = null,
            List<int>? sectionIds = null);
        Task<ApiResponse<dynamic>> UpsertClassTeacherAsync(ClassTeacherUpsertRequest req);
        Task<ApiResponse<dynamic>> DeleteClassTeacherAsync(List<int> ids);

        Task<ApiResponse<List<StudentPromotionViewModel>>> GetStudentsForPromotionAsync(int classId, int sectionId);
        Task<ApiResponse<dynamic>> PromoteStudentsAsync(PromotionRequest req);
    }
}
