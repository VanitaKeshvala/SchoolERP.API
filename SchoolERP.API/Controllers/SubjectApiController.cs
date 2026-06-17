using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Interfaces;
using SchoolERP.API.Models;
using SchoolERP.API.Models.Common;
using SchoolERP.API.Services;
using System.Collections.Generic;
using System.Security.Claims;

namespace SchoolERP.API.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SubjectApiController : ControllerBase
    {
        private readonly ISubjectService _subjectService;
        private readonly ICompanyService _companyService;
        private readonly ISessionService _sessionService;
        private readonly IUserMenuPermissionService _menuPerm;

        private const string MenuPath = "/Academics/Subject";

        public SubjectApiController(ISubjectService subjectService, ICompanyService companyService, ISessionService sessionService, IUserMenuPermissionService menuPerm)
        {
            _subjectService = subjectService;
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
                return Ok(ApiResponse<List<MstSubjectViewModel>>.SuccessResponse(new List<MstSubjectViewModel>()));

            var data = _subjectService.GetAllSubjects(companyId, sessionId, includeDeleted);
            return Ok(ApiResponse<List<MstSubjectViewModel>>.SuccessResponse(data));
        }

        [HttpGet("GetByID/{id}")]
        public IActionResult GetByID(int id)
        {
            var data = _subjectService.GetSubjectByID(id);
            if (data == null) return NotFound(ApiResponse<MstSubjectViewModel>.ErrorResponse("Subject not found"));
            return Ok(ApiResponse<MstSubjectViewModel>.SuccessResponse(data));
        }

        [HttpPost("Upsert")]
        public async Task<IActionResult> Upsert([FromBody] MstSubjectUpsertRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            int userId = GetCurrentUserId();

            var isCreate = request.SubjectID <= 0;
            if (isCreate && !await _menuPerm.Has(User, MenuPath, "Add"))
                return Ok(new { success = false, message = "You do not have permission to add subjects." });
            if (!isCreate && !await _menuPerm.Has(User, MenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to edit subjects." });

            int companyId = _companyService.GetUserCurrentCompany(userId) ?? 0;
            int sessionId = _sessionService.GetUserCurrentSession(userId) ?? 0;

            if (companyId == 0 || sessionId == 0)
                return BadRequest(ApiResponse<dynamic>.ErrorResponse("Current company or session not set."));

            var (success, message) = _subjectService.UpsertSubject(request, companyId, sessionId, userId);
            return Ok(new { success, message });
        }

        [HttpPost("Delete")]
        public async Task<IActionResult> Delete(List<int> ids)
        {
            if (!await _menuPerm.Has(User, MenuPath, "Delete"))
                return Ok(new { success = false, message = "You do not have permission to delete subjects." });

            int userId = GetCurrentUserId();
            var data = _subjectService.DeleteSubject(ids, userId);
            return Ok(new { data.success, data.message });
        }

        [HttpPost("ToggleStatus")]
        public async Task<IActionResult> ToggleStatus(int id, bool isActive)
        {
            if (!await _menuPerm.Has(User, MenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to change subject status." });

            int userId = GetCurrentUserId();
            var (success, message) = _subjectService.ToggleSubjectStatus(id, isActive, userId);
            return Ok(new { success, message });
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 1;
        }
    }
}
