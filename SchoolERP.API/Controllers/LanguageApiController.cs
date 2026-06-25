using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using SchoolERP.API.Services;
using System.Collections.Generic;
using System.Security.Claims;

namespace SchoolERP.API.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    /// <summary>
    /// This controller provides the technical endpoints for managing languages through the API.
    /// </summary>
    public class LanguageApiController : ControllerBase
    {
        private readonly ILanguageService _languageService;

        public LanguageApiController(ILanguageService languageService)
        {
            _languageService = languageService;
        }

        /// <summary>
        /// Gets the full list of all languages supported by the system.
        /// </summary>
        [HttpGet("GetAll")]
        public IActionResult GetAll(bool includeDeleted = false)
        {
            var data = _languageService.GetAllLanguages(includeDeleted);
            return Ok(ApiResponse<List<MstLanguageViewModel>>.SuccessResponse(data));
        }

        /// <summary>
        /// Gets the details of one specific language using its unique ID number.
        /// </summary>
        [HttpGet("GetByID/{id}")]
        public IActionResult GetByID(int id)
        {
            var data = _languageService.GetLanguageByID(id);
            if (data == null) return NotFound(ApiResponse<MstLanguageViewModel>.ErrorResponse("Language identifier nullified"));
            return Ok(ApiResponse<MstLanguageViewModel>.SuccessResponse(data));
        }

        /// <summary>
        /// Saves a new language or updates an existing one with the details provided.
        /// </summary>
        [HttpPost("Upsert")]
        public IActionResult Upsert([FromBody] MstLanguageUpsertRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            int userId = GetCurrentUserId();
            var (success, message) = _languageService.UpsertLanguage(request, userId);
            if (success) return Ok(ApiResponse<dynamic>.SuccessResponse(new { success, message }));
            return BadRequest(ApiResponse<dynamic>.ErrorResponse(message));
        }

        /// <summary>
        /// Permanently removes a language record from the system.
        /// </summary>
        [HttpPost("Delete")]
        public IActionResult Delete(List<int> id)
        {
            int userId = GetCurrentUserId();
            var (success, message) = _languageService.DeleteLanguage(id, userId);
            if (success) return Ok(ApiResponse<dynamic>.SuccessResponse(new { success, message }));
            return BadRequest(ApiResponse<dynamic>.ErrorResponse(message));
        }

        /// <summary>
        /// Turns a language's active status on or off.
        /// </summary>
        [HttpPost("ToggleStatus")]
        public IActionResult ToggleStatus(int id, bool isActive)
        {
            int userId = GetCurrentUserId();
            var (success, message) = _languageService.ToggleLanguageStatus(id, isActive, userId);
            if (success) return Ok(ApiResponse<dynamic>.SuccessResponse(new { success, message }));
            return BadRequest(ApiResponse<dynamic>.ErrorResponse(message));
        }

        /// <summary>
        /// Extracts user identifier securely bypassing payload vulnerabilities.
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 1; 
        }
    }
}
