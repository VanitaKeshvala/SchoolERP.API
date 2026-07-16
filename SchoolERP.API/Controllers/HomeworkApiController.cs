using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using SchoolERP.API.Services;
using System.Collections.Generic;
using System.Security.Claims;

namespace SchoolERP.Net.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HomeworkApiController : ControllerBase
    {
        private readonly IHomeworkService _service;
        private readonly ICompanyService _companyService;
        private readonly ISessionService _sessionService;
        private readonly IUserMenuPermissionService _menuPerm;

        private const string MenuPath = "/Homework/Add";

        public HomeworkApiController(IHomeworkService service, ICompanyService companyService, ISessionService sessionService, IUserMenuPermissionService menuPerm)
        {
            _service = service;
            _companyService = companyService;
            _sessionService = sessionService;
            _menuPerm = menuPerm;
        }

        [HttpGet("GetAll")]
        public IActionResult GetAll(bool includeDeleted = false)
        {
            try
            {
                int userId = GetCurrentUserId();
                int companyId = _companyService.GetUserCurrentCompany(userId) ?? 0;
                int sessionId = _sessionService.GetUserCurrentSession(userId) ?? 0;

                if (companyId == 0 || sessionId == 0)
                    return Ok(ApiResponse<List<HomeworkViewModel>>.SuccessResponse(new List<HomeworkViewModel>()));

                var data = _service.GetAll(companyId, sessionId, includeDeleted);
                return Ok(ApiResponse<List<HomeworkViewModel>>.SuccessResponse(data));
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
            
        }

        [HttpGet("GetByID/{id}")]
        public IActionResult GetByID(int id)
        {
            try
            {
                var data = _service.GetByID(id);
                if (data == null) return NotFound(ApiResponse<HomeworkViewModel>.ErrorResponse("Homework not found"));
                return Ok(ApiResponse<HomeworkViewModel>.SuccessResponse(data));
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
            
        }

        [HttpPost("Upsert")]
        public async Task<IActionResult> Upsert([FromBody] HomeworkUpsertRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                int userId = GetCurrentUserId();

                var isCreate = request.HomeworkID <= 0;
                if (isCreate && !await _menuPerm.Has(User, MenuPath, "Add"))
                    return Ok(new { success = false, message = "You do not have permission to add homework." });
                if (!isCreate && !await _menuPerm.Has(User, MenuPath, "Edit"))
                    return Ok(new { success = false, message = "You do not have permission to edit homework." });

                int companyId = _companyService.GetUserCurrentCompany(userId) ?? 0;
                int sessionId = _sessionService.GetUserCurrentSession(userId) ?? 0;

                if (companyId == 0 || sessionId == 0)
                    return BadRequest(ApiResponse<dynamic>.ErrorResponse("Current company or session not set."));

                var response = _service.Upsert(request, companyId, sessionId, userId);
                return Ok(ApiResponse<dynamic>.SuccessResponse(response));
                //return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
            
        }

        [HttpPost("Delete")]
        public async Task<IActionResult> Delete(List<int> ids)
        {
            try
            {
                if (!await _menuPerm.Has(User, MenuPath, "Delete"))
                    return Ok(new { success = false, message = "You do not have permission to delete homework." });

                int userId = GetCurrentUserId();
                var (success, message) = _service.Delete(ids, userId);
                return Ok(new { success, message });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
            
        }

        [HttpPost("ToggleStatus")]
        public async Task<IActionResult> ToggleStatus([FromBody] StatusUpdateRequest request)
        {
            try
            {
                if (!await _menuPerm.Has(User, MenuPath, "Edit"))
                    return Ok(new { success = false, message = "You do not have permission to change status." });

                int userId = GetCurrentUserId();
                var (success, message) = _service.ToggleStatus(request);
                return Ok(new { success, message });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
           
        }

        private int GetCurrentUserId()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                return userIdClaim != null ? int.Parse(userIdClaim.Value) : 1;
            }
            catch (Exception ex)
            {
                return 0;
            }
            
        }


        [HttpPost("GetAllHomeWorkWithPage")]
        public async Task<IActionResult> GetAllHomeWorkWithPage([FromBody] SearchRequest request)
        {
            try
            {
                if (request.CompanyID == 0 || request.SessionID == 0)
                    return Ok(ApiResponse<List<HomeworkViewModel>>.SuccessResponse(new List<HomeworkViewModel>()));
                var data = await _service.GetAllHomeWorkWithPage(request);
                return Ok(ApiResponse<PagedResult<HomeworkViewModel>>.SuccessResponse(data));
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message});
            }
        }

    }
}
