using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using SchoolERP.API.Services;
using System.Security.Claims;

namespace SchoolERP.Net.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    /// <summary>
    /// This controller provides the technical endpoints for managing currency settings through the API.
    /// </summary>
    public class CurrencyApiController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;

        public CurrencyApiController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        /// <summary>
        /// Gets the full list of all available currencies from the system.
        /// </summary>
        [HttpGet("GetAll")]
        public IActionResult GetAll(bool includeDeleted = false)
        {
            var data = _currencyService.GetAllCurrencies(includeDeleted);
            return Ok(ApiResponse<List<MstCurrencyViewModel>>.SuccessResponse(data));
        }

        /// <summary>
        /// Gets the details of one specific currency using its unique ID number.
        /// </summary>
        [HttpGet("GetByID/{id}")]
        public IActionResult GetByID(int id)
        {
            var data = _currencyService.GetCurrencyByID(id);
            if (data == null) return NotFound(ApiResponse<MstCurrencyViewModel>.ErrorResponse("Denomination not found"));
            return Ok(ApiResponse<MstCurrencyViewModel>.SuccessResponse(data));
        }

        /// <summary>
        /// Saves a new currency or updates an existing one with the details provided.
        /// </summary>
        [HttpPost("Upsert")]
        public IActionResult Upsert([FromBody] MstCurrencyUpsertRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            int userId = GetCurrentUserId();
            var (success, message) = _currencyService.UpsertCurrency(request, userId);
            return Ok(new { success, message });
        }

        /// <summary>
        /// Permanently removes a currency's record from the system.
        /// </summary>
        [HttpPost("Delete")]
        public IActionResult Delete(List<int> id)
        {
            int userId = GetCurrentUserId();
            var (success, message) = _currencyService.DeleteCurrency(id, userId);
            return Ok(new { success, message });
        }

        /// <summary>
        /// Turns a currency's active status on or off.
        /// </summary>
        [HttpPost("ToggleStatus")]
        public IActionResult ToggleStatus(int id, bool isActive)
        {
            int userId = GetCurrentUserId();
            var (success, message) = _currencyService.ToggleCurrencyStatus(id, isActive, userId);
            return Ok(new { success, message });
        }

        /// <summary>
        /// Contextually parses incoming bearer identities.
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 1; 
        }
    }
}
