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
    public class ClassApiController : ControllerBase
    {
        private readonly IClassService _classService;
        private readonly ICompanyService _companyService;
        private readonly ISessionService _sessionService;
        private readonly IUserMenuPermissionService _menuPerm;

        private const string MenuPath = "/Academics/Class";

        public ClassApiController(IClassService classService, ICompanyService companyService, ISessionService sessionService, IUserMenuPermissionService menuPerm)
        {
            _classService = classService;
            _companyService = companyService;
            _sessionService = sessionService;
            _menuPerm = menuPerm;
        }

        [HttpGet("GetAll")]
        public IActionResult GetAll(bool includeDeleted = false)
        {
            int userId = GetCurrentUserId();
            int companyId = _companyService.GetUserCurrentCompany(userId) ?? 0;
            int sessionId = _sessionService.GetUserCurrentSession(userId) ?? 0;

            if (companyId == 0 || sessionId == 0)
                return Ok(ApiResponse<List<MstClassViewModel>>.SuccessResponse(new List<MstClassViewModel>()));

            var data = _classService.GetAllClasses(companyId, sessionId, includeDeleted);
            return Ok(ApiResponse<List<MstClassViewModel>>.SuccessResponse(data));
        }

        [HttpGet("GetByID/{id}")]
        public IActionResult GetByID(int id)
        {
            var data = _classService.GetClassByID(id);
            if (data == null) return NotFound(ApiResponse<MstClassViewModel>.ErrorResponse("Class not found"));
            return Ok(ApiResponse<MstClassViewModel>.SuccessResponse(data));
        }

        [HttpPost("Upsert")]
        public async Task<IActionResult> Upsert([FromBody] MstClassUpsertRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            int userId = GetCurrentUserId();

            var isCreate = request.ClassID <= 0;
            if (isCreate && !await _menuPerm.Has(User, MenuPath, "Add"))
                return Ok(new { success = false, message = "You do not have permission to add classes." });
            if (!isCreate && !await _menuPerm.Has(User, MenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to edit classes." });

            int companyId = _companyService.GetUserCurrentCompany(userId) ?? 0;
            int sessionId = _sessionService.GetUserCurrentSession(userId) ?? 0;

            if (companyId == 0 || sessionId == 0)
                return BadRequest(ApiResponse<dynamic>.ErrorResponse("Current company or session not set."));

            var (success, message) = _classService.UpsertClass(request, companyId, sessionId, userId);
            return Ok(new { success, message });
        }

        [HttpPost("Delete")]
        public async Task<IActionResult> Delete(List<int> ids)
        {
            if (!await _menuPerm.Has(User, MenuPath, "Delete"))
                return Ok(new { success = false, message = "You do not have permission to delete classes." });

            int userId = GetCurrentUserId();
            var (success, message) = _classService.DeleteClass(ids, userId);
            return Ok(new { success, message });
        }

        [HttpPost("ToggleStatus")]
        public async Task<IActionResult> ToggleStatus(int id, bool isActive)
        {
            if (!await _menuPerm.Has(User, MenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to change class status." });

            int userId = GetCurrentUserId();
            var (success, message) = _classService.ToggleClassStatus(id, isActive, userId);
            return Ok(new { success, message });
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 1;
        }
    }
}
