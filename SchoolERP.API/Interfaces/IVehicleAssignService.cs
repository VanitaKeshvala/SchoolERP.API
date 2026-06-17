using SchoolERP.API.Models;

namespace SchoolERP.API.Interfaces
{
    public interface IVehicleAssignService
    {
        List<VehicleAssignViewModel> GetAllAssignments(int companyId, int sessionId);
        VehicleAssignViewModel? GetAssignmentByID(int id);
        (bool Success, string Message) UpsertAssignments(VehicleAssignUpsertRequest req, int companyId, int sessionId, int userId);
        (bool Success, string Message) DeleteAssignment(List<int> id, int userId);
        (bool Success, string Message) ToggleAssignmentStatus(int id, bool isActive, int userId);
    }
}
