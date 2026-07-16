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
    public class LibrarySubjectAPIController : ControllerBase
    {
        private readonly ILibrarySubjectService _subjectService;
        private readonly IUserMenuPermissionService _menuPerm;
        private const string MenuPath = "/Publisher/Index";

        public LibrarySubjectAPIController(ILibrarySubjectService subjectService, IUserMenuPermissionService menuPerm)
        {
            _subjectService = subjectService;
            _menuPerm = menuPerm;
        }
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 1;
        }

        [HttpPost("Save")]
        public async Task<IActionResult> Save([FromBody] LibrarySubjectRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                model.UserID = GetCurrentUserId();
                // Capture client IP if not provided
                if (string.IsNullOrWhiteSpace(model.IPAddress))
                    model.IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                var response = await _subjectService.UpsertLibrarySubjectAsync(model);
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

                var response = await _subjectService.GetLibrarySubjectByIdAsync(id);
                return Ok(response);
            }
            catch (Exception)
            {
                throw;
            }

        }

        [HttpPost("GetAllLibrarySubjectWithPage")]
        public async Task<IActionResult> GetAllLibrarySubjectWithPage([FromBody] SearchRequest request)
        {
            try
            {
                if (request.CompanyID == 0)
                    return Ok(ApiResponse<List<LibrarySubjectModel>>.SuccessResponse(new List<LibrarySubjectModel>()));
                var data = await _subjectService.GetAllLibrarySubjectWithPage(request);
                return Ok(ApiResponse<PagedResult<LibrarySubjectModel>>.SuccessResponse(data));
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
                var (success, message) = _subjectService.DeleteLibrarySubject(ids, userId);
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
                var (success, message) = _subjectService.ToggleLibrarySubjectStatus(request);
                return Ok(new { success, message });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }

        }
    }
}
