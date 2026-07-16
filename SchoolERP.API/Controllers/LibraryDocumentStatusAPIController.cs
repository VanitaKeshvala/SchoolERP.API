using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;
using System.Security.Claims;

namespace SchoolERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LibraryDocumentStatusAPIController : ControllerBase
    {
        private readonly ILibraryDocumentStatusService _documentStatusService;
        private readonly IUserMenuPermissionService _menuPerm;
        private const string MenuPath = "/LibraryCategory/Index";

        public LibraryDocumentStatusAPIController(ILibraryDocumentStatusService documentStatusService, IUserMenuPermissionService menuPerm)
        {
            _documentStatusService = documentStatusService;
            _menuPerm = menuPerm;
        }
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 1;
        }

        [HttpPost("Save")]
        public async Task<IActionResult> Save([FromBody] LibraryDocumentStatusRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                model.UserID = GetCurrentUserId();
                // Capture client IP if not provided
                if (string.IsNullOrWhiteSpace(model.IPAddress))
                    model.IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                var response = await _documentStatusService.UpsertDocumentStatusAsync(model);
                return Ok(response);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse { Result = 0, Message = "Invalid ID." });

                var response = await _documentStatusService.GetDocumentStatusByIdAsync(id);
                return Ok(response);
            }
            catch (Exception)
            {
                throw;
            }

        }

        [HttpPost("GetAllDocumentStatusWithPage")]
        public async Task<IActionResult> GetAllDocumentStatusWithPage([FromBody] SearchRequest request)
        {
            try
            {
                if (request.CompanyID == 0)
                    return Ok(ApiResponse<List<LibraryDocumentStatusModel>>.SuccessResponse(new List<LibraryDocumentStatusModel>()));
                var data = await _documentStatusService.GetAllDocumentStatusWithPage(request);
                return Ok(ApiResponse<PagedResult<LibraryDocumentStatusModel>>.SuccessResponse(data));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost("Delete")]
        public async Task<IActionResult> Delete(List<int> ids)
        {
            try
            {
                if (!await _menuPerm.Has(User, MenuPath, "Delete"))
                    return Ok(new { success = false, message = "You do not have permission to delete classes." });

                int userId = GetCurrentUserId();
                var (success, message) = _documentStatusService.DeleteDocumentStatus(ids, userId);
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
                    return Ok(new { success = false, message = "You do not have permission to change class status." });

                int userId = GetCurrentUserId();
                var (success, message) = _documentStatusService.ToggleDocumentStatus(request);
                return Ok(new { success, message });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }

        }
    }
}
