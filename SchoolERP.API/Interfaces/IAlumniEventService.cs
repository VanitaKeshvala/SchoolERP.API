using SchoolERP.Shared.Models;

namespace SchoolERP.API.Interfaces
{
    public interface IAlumniEventService
    {
        List<AlumniEventViewModel> GetEvents(string? searchText, int companyId);
        (bool Success, string Message) UpsertEvent(AlumniEventUpsertRequest req, int companyId, int userId);
        (bool Success, string Message) DeleteEvent(int eventId, int companyId, int userId);
        (bool Success, string Message) ToggleEventStatus(int eventId, bool isActive, int companyId, int userId);
        (byte[]? Bytes, string? FileName, string? ContentType) GetEventPhoto(int eventId, int companyId);
    }
}
