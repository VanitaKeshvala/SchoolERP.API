using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using System.Security.Claims;

namespace SchoolERP.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleAssignApiController : ControllerBase
    {
        private readonly IVehicleAssignService _vehicleAssignService;
        private readonly ICompanyService _companySvc;
        private readonly ISessionService _sessionSvc;
        private readonly IUserMenuPermissionService _menuPerm;

        private const string MenuPath = "/Transport/VehicleAssign";

        public VehicleAssignApiController(IVehicleAssignService vehicleAssignService, ICompanyService companySvc, ISessionService sessionSvc, IUserMenuPermissionService menuPerm)
        {
            _vehicleAssignService = vehicleAssignService;
            _companySvc = companySvc;
            _sessionSvc = sessionSvc;
            _menuPerm = menuPerm;
        }

        private int GetUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("UserId"), out var id) ? id : 0;
        private int GetCompanyId() => _companySvc.GetUserCurrentCompany(GetUserId()) ?? 0;
        private int GetSessionId() => _sessionSvc.GetUserCurrentSession(GetUserId()) ?? 0;

        [HttpGet("GetAllAssignments")]
        public IActionResult GetAllAssignments()
        {
            var data = _vehicleAssignService.GetAllAssignments(GetCompanyId(), GetSessionId());
            return Ok(new { success = true, data });
        }

        [HttpGet("GetAssignmentByID/{id}")]
        public IActionResult GetAssignmentByID(int id)
        {
            var data = _vehicleAssignService.GetAssignmentByID(id);
            if (data == null) return Ok(new { success = false, message = "Record not found" });
            return Ok(new { success = true, data });
        }

        [HttpPost("UpsertAssignments")]
        public async Task<IActionResult> UpsertAssignments([FromBody] VehicleAssignUpsertRequest req)
        {
            // This action always creates new assignments for the selected vehicles on a route.
            if (!await _menuPerm.Has(User, MenuPath, "Add"))
                return Ok(new { success = false, message = "You do not have permission to add assignments." });

            var res = _vehicleAssignService.UpsertAssignments(req, GetCompanyId(), GetSessionId(), GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        [HttpPost("DeleteAssignment")]
        public async Task<IActionResult> DeleteAssignment(List<int> id)
        {
            if (!await _menuPerm.Has(User, MenuPath, "Delete"))
                return Ok(new { success = false, message = "You do not have permission to delete assignments." });

            var res = _vehicleAssignService.DeleteAssignment(id, GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        [HttpPost("ToggleAssignmentStatus")]
        public async Task<IActionResult> ToggleAssignmentStatus(int id, bool isActive)
        {
            if (!await _menuPerm.Has(User, MenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to change status." });

            var res = _vehicleAssignService.ToggleAssignmentStatus(id, isActive, GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }
    }
}
