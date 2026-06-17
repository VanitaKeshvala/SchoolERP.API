using SchoolERP.API.Models;

namespace SchoolERP.API.Interfaces
{
    public interface IStudentInformationService
    {
        List<StudentDisableReasonViewModel> GetAllDisableReasons(int companyId, int sessionId);
        (bool Success, string Message) UpsertDisableReason(StudentDisableReasonUpsertRequest req, int companyId, int sessionId, int userId);
        (bool Success, string Message) DeleteDisableReason(List<int> ids, int userId);

        List<StudentHouseViewModel> GetAllStudentHouses(int companyId, int sessionId);
        (bool Success, string Message) UpsertStudentHouse(StudentHouseUpsertRequest req, int companyId, int sessionId, int userId);
        (bool Success, string Message) DeleteStudentHouse(List<int> ids, int userId);

        List<StudentCategoryViewModel> GetAllStudentCategories(int companyId, int sessionId);
        (bool Success, string Message) UpsertStudentCategory(StudentCategoryUpsertRequest req, int companyId, int sessionId, int userId);
        (bool Success, string Message) DeleteStudentCategory(List<int> ids, int userId);
        string GetNewStudentRollNo(int companyId, int sessionId, Dictionary<string, string> dynamicValues = null);
        string GetNextSimpleAdmissionNo(int companyId);
        (bool Success, string Message, int StudentID) UpsertStudentAdmission(StudentAdmissionUpsertRequest req, int companyId, int sessionId, int userId);
        StudentDetailsViewModel GetStudentDetails(int studentId, int companyId, int sessionId);
        List<StudentListViewModel> GetStudentList(int companyId, int sessionId, int? classId, int? sectionId, string? searchTerm);
        (bool Success, string Message) ToggleStudentStatus(StudentStatusToggleRequest req, int userId);
        List<MultiClassStudentCardViewModel> GetMultiClassStudents(int companyId, int sessionId, int? classId, int? sectionId, string? searchTerm);
        List<StudentListViewModel> GetDisabledStudentList(int companyId, int sessionId, int? classId, int? sectionId, string? searchTerm);
        (bool Success, string Message) UpsertStudentMultiClass(StudentMultiClassUpsertRequest req, int companyId, int sessionId, int userId);
        (bool Success, string Message) DeleteStudentMultiClass(int id, int userId);
        (bool Success, string Message) BulkDeleteStudents(List<int> studentIds, int userId);
        (bool Success, string Message) DeleteStudent(int id, int userId);

        // Timeline
        List<StudentTimelineViewModel> GetStudentTimeline(int studentId);
        (bool Success, string Message) UpsertStudentTimeline(StudentTimelineUpsertRequest req, int companyId, int sessionId, int userId);
        (bool Success, string Message) DeleteStudentTimeline(int id, int userId);
        (byte[] Bytes, string FileName, string ContentType) GetStudentTimelineDocument(int id);
    }
}
