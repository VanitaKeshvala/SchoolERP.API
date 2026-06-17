using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Interfaces;
using SchoolERP.API.Models;
using SchoolERP.API.Models.Common;
using System.Security.Claims;

namespace SchoolERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    /// <summary>
    /// This controller provides the technical endpoints for managing academic sessions (school years) through the API.
    /// </summary>
    public class SessionApiController : ControllerBase
    {
        private readonly ISessionService _sessionService;
        private readonly IUserMenuPermissionService _menuPerm;
        private readonly ICompanyService _companySvc;

        private const string MenuPath = "/Settings";

        public SessionApiController(ISessionService sessionService, IUserMenuPermissionService menuPerm, ICompanyService companySvc)
        {
            _sessionService = sessionService;
            _companySvc = companySvc;
            _menuPerm = menuPerm;
        }
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "1");
        private int CompanyId => _companySvc.GetUserCurrentCompany(UserId) ?? 0;
        /// <summary>
        /// Gets the full list of all academic sessions defined in the system.
        /// </summary>
        [HttpGet("GetAll")]
        public IActionResult GetAll(bool includeDeleted = false)
        {
            var data = _sessionService.GetAllSessions(includeDeleted);
            return Ok(ApiResponse<List<MstSessionViewModel>>.SuccessResponse(data));
        }

        /// <summary>
        /// Gets the details of one specific academic session using its unique ID number.
        /// </summary>
        [HttpGet("GetByID/{id}")]
        public IActionResult GetByID(int id)
        {
            var data = _sessionService.GetSessionByID(id);
            if (data == null) return NotFound(ApiResponse<MstSessionViewModel>.ErrorResponse("Time phase interval not found"));
            return Ok(ApiResponse<MstSessionViewModel>.SuccessResponse(data));
        }

        /// <summary>
        /// Saves a new academic session or updates an existing one with the dates and name you provided.
        /// </summary>
        [HttpPost("Upsert")]
        public async Task<IActionResult> Upsert([FromBody] MstSessionUpsertRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            int userId = GetCurrentUserId();
            var isCreate = request.SessionId <= 0;
            if (isCreate && !await _menuPerm.Has(User, MenuPath, "Add"))
                return Ok(new { success = false, message = "You do not have permission to add sessions." });
            if (!isCreate && !await _menuPerm.Has(User, MenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to edit sessions." });

            var (success, message) = _sessionService.UpsertSession(request, userId, CompanyId);
            return Ok(new { success, message });
        }

        /// <summary>
        /// Permanently removes an academic session from the system's records.
        /// </summary>
        [HttpPost("Delete")]
        public async Task<IActionResult> Delete(List<int> id)
        {
            if (!await _menuPerm.Has(User, MenuPath, "Delete"))
                return Ok(new { success = false, message = "You do not have permission to delete sessions." });

            int userId = GetCurrentUserId();
            var (success, message) = _sessionService.DeleteSession(id, userId);
            return Ok(new { success, message });
        }

        /// <summary>
        /// Turns an academic session's active status on or off.
        /// </summary>
        [HttpPost("ToggleStatus")]
        public async Task<IActionResult> ToggleStatus(int id, bool isActive)
        {
            if (!await _menuPerm.Has(User, MenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to change status." });

            int userId = GetCurrentUserId();
            var (success, message) = _sessionService.ToggleSessionStatus(id, isActive, userId);
            return Ok(new { success, message });
        }

        /// <summary>
        /// Sets the chosen academic session as the currently 'active' one for the user in the database.
        /// </summary>
        [HttpPost("SetCurrent")]
        public IActionResult SetCurrent([FromBody] SetCurrentSessionRequest request)
        {
            int userId = GetCurrentUserId();
            var (success, message) = _sessionService.UpdateUserCurrentSession(userId, request.SessionId);
            if (!success) return BadRequest(ApiResponse<dynamic>.ErrorResponse(message));
            return Ok(ApiResponse<dynamic>.SuccessResponse(null, message));
        }

        /// <summary>
        /// Gets the ID of the academic session that the user is currently working in.
        /// </summary>
        [HttpGet("GetUserCurrentSession")]
        public IActionResult GetUserCurrentSession()
        {
            int userId = GetCurrentUserId();
            var sessionId = _sessionService.GetUserCurrentSession(userId);
            return Ok(ApiResponse<int?>.SuccessResponse(sessionId));
        }

        /// <summary>
        /// Resolves cryptographically bound credentials parsing the requesting context.
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 1;
        }
    }
}
