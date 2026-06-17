using SchoolERP.API.Models;

namespace SchoolERP.API.Interfaces
{
    public interface IHomeworkService
    {
        List<HomeworkViewModel> GetAll(int companyId, int sessionId, bool includeDeleted = false);
        HomeworkViewModel? GetByID(int id);
        (bool success, string message) Upsert(HomeworkUpsertRequest request, int companyId, int sessionId, int userId);
        (bool success, string message) Delete(int id, int userId);
        (bool success, string message) ToggleStatus(int id, bool isActive, int userId);
    }
}
