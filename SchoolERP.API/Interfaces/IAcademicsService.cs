using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.API.Interfaces
{
    public interface IAcademicsService
    {
        List<TimeTableViewModel> GetTimeTableByClass(int companyId, int sessionId, int classId, int sectionId);
        List<TimeTableViewModel> GetTimeTableByStaff(TimeTableSearchRequest request);
        (bool success, string message) UpsertTimeTable(TimeTableUpsertRequest req, int companyId, int sessionId, int userId);
        (bool success, string message) DeleteTimeTableSlot(int id, int userId);

        List<ClassTeacherViewModel> GetAllClassTeachers(int companyId, int sessionId, List<int> classId=null, List<int> sectionId=null);
        (bool success, string message) UpsertClassTeacher(ClassTeacherUpsertRequest req, int companyId, int sessionId, int userId);
        (bool success, string message) DeleteClassTeacher(List<int> id, int userId);

        List<StudentPromotionViewModel> GetStudentsForPromotion(int companyId, int sessionId, int classId, int sectionId);
        (bool success, string message) PromoteStudents(PromotionRequest req, int companyId, int userId);

        Task<PagedResult<ClassTeacherViewModel>> GetAllClassTeachersWithPage(AcademicsSearchRequest req);
        Task<PagedResult<StudentPromotionViewModel>> GetForPromotionPageIndex(SearchPromotedStudent req);
        TimeTableViewModel? GetTimeTableById(int companyId, int sessionId, int timeTableID);
    }
}
