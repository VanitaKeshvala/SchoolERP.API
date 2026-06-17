using SchoolERP.API.Models;

namespace SchoolERP.API.Interfaces
{
    public interface IStudentLeaveService
    {
        List<StudentLeaveViewModel> GetLeaveApplications(int? classId, int? sectionId, int? status, int companyId);
        (bool Success, string Message) UpdateLeaveStatus(int leaveAppId, int status, int companyId, int userId);
        (bool Success, string Message) UpsertLeaveApplication(StudentLeaveUpsertRequest req, int companyId, int userId);
        (byte[]? Bytes, string? FileName, string? ContentType) GetLeaveAttachment(int leaveAppId, int companyId);
    }
}
