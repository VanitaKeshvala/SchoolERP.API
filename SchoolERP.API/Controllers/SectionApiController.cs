using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using System.Security.Claims;

namespace SchoolERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SectionApiController : ControllerBase
    {
        private readonly ISectionService _sectionService;
        private readonly ICompanyService _companyService;
        private readonly ISessionService _sessionService;
        private readonly IUserMenuPermissionService _menuPerm;

        private const string MenuPath = "/Academics/Section";

        public SectionApiController(ISectionService sectionService, ICompanyService companyService, ISessionService sessionService, IUserMenuPermissionService menuPerm)
        {
            _sectionService = sectionService;
            _companyService = companyService;
            _sessionService = sessionService;
            _menuPerm = menuPerm;
        }


        [HttpGet("GetAll")]
        public IActionResult GetAll(bool includeDeleted = false, int sessionId=0)
        {
            int userId = GetCurrentUserId();
            int companyId = _companyService.GetUserCurrentCompany(userId) ?? 0;
            //int sessionId = _sessionService.GetUserCurrentSession(userId) ?? 0;

            if (companyId == 0 || sessionId == 0)
                return Ok(ApiResponse<List<MstSectionViewModel>>.SuccessResponse(new List<MstSectionViewModel>()));

            var data = _sectionService.GetAllSections(companyId, sessionId, includeDeleted, userId);
            return Ok(ApiResponse<List<MstSectionViewModel>>.SuccessResponse(data));
        }

        [HttpGet("GetByClass/{classId}")]
        public IActionResult GetByClass(int classId)
        {
            var data = _sectionService.GetSectionsByClass(classId);
            return Ok(ApiResponse<List<MstSectionViewModel>>.SuccessResponse(data));
        }

        [HttpGet("GetByID/{id}")]
        public IActionResult GetByID(int id)
        {
            var data = _sectionService.GetSectionByID(id);
            if (data == null) return NotFound(ApiResponse<MstSectionViewModel>.ErrorResponse("Section not found"));
            return Ok(ApiResponse<MstSectionViewModel>.SuccessResponse(data));
        }

        [HttpPost("Upsert")]
        public async Task<IActionResult> Upsert([FromBody] MstSectionUpsertRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            int userId = GetCurrentUserId();

            var isCreate = request.SectionID <= 0;
            if (isCreate && !await _menuPerm.Has(User, MenuPath, "Add"))
                return Ok(new { success = false, message = "You do not have permission to add sections." });
            if (!isCreate && !await _menuPerm.Has(User, MenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to edit sections." });

            int companyId = _companyService.GetUserCurrentCompany(userId) ?? 0;
            int sessionId = _sessionService.GetUserCurrentSession(userId) ?? 0;

            if (companyId == 0 || sessionId == 0)
                return BadRequest(ApiResponse<dynamic>.ErrorResponse("Current company or session not set."));

            var (success, message) = _sectionService.UpsertSection(request, companyId, sessionId, userId);
            return Ok(new { success, message });
        }

        [HttpPost("Delete")]
        public async Task<IActionResult> Delete(List<int> ids)
        {
            if (!await _menuPerm.Has(User, MenuPath, "Delete"))
                return Ok(new { success = false, message = "You do not have permission to delete sections." });

            int userId = GetCurrentUserId();
            var (success, message) = _sectionService.DeleteSection(ids, userId);
            return Ok(new { success, message });
        }

        [HttpPost("ToggleStatus")]
        public async Task<IActionResult> ToggleStatus([FromBody] StatusUpdateRequest request)
        {
            try
            {
                if (!await _menuPerm.Has(User, MenuPath, "Edit"))
                    return Ok(new { success = false, message = "You do not have permission to change section status." });

                int userId = GetCurrentUserId();
                var (success, message) = _sectionService.ToggleSectionStatus(request);
                return Ok(new { success, message });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message =ex.Message });
            }            
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 1;
        }

        

        [HttpPost("CopyToSession")]
        public async Task<IActionResult> CopyToSession([FromBody] SectionCopyRequest request)
        {
            try
            {
                if (!await _menuPerm.Has(User, MenuPath, "Add"))
                    return Ok(new { success = false, message = "You do not have permission to copy sections." });

                if (string.IsNullOrWhiteSpace(request.SectionIds))
                    return Ok(new { success = false, message = "Please select at least one section to copy." });

                if (request.TargetSessionId <= 0)
                    return Ok(new { success = false, message = "Please select a target session." });

                int userId = GetCurrentUserId();
                int companyId = _companyService.GetUserCurrentCompany(userId) ?? 0;

                if (companyId == 0)
                    return Ok(new { success = false, message = "Company not found." });

                var (success, message, inserted, skipped) =
                    _sectionService.CopySectionsToSession(
                        request.SectionIds,
                        request.TargetSessionId,
                        companyId,
                        userId);

                return Ok(new { success, message, inserted, skipped });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }
    }
}
