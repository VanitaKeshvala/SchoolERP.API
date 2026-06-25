using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using SchoolERP.API.Services;

namespace SchoolERP.Net.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    /// <summary>
    /// This controller provides the technical endpoints for configuring the SMS gateway settings through the API.
    /// </summary>
    public class SmsConfigApiController : ControllerBase
    {
        private readonly ISmsConfigService _smsConfigService;
        private readonly ICompanyService _companySvc;

        public SmsConfigApiController(ISmsConfigService smsConfigService, ICompanyService companySvc)
        {
            _smsConfigService = smsConfigService;
            _companySvc = companySvc;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "1");
        private int CompanyId => _companySvc.GetUserCurrentCompany(UserId) ?? 0;

        /// <summary>
        /// Gets the current SMS gateway settings (like the API URL and keys) from the system.
        /// </summary>
        [HttpGet("Get")]
        public IActionResult GetConfig()
        {
            try
            {
                var config = _smsConfigService.GetSmsConfig(CompanyId);
                return Ok(new { success = true, data = config });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Saves or updates the SMS gateway settings with the information you provided.
        /// </summary>
        [HttpPost("Upsert")]
        public IActionResult UpsertConfig([FromBody] MstSmsConfigUpsertRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid data format or missing required payload parameters" });
                }

                // Identify the requester for audit traceability mappings
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int userId = string.IsNullOrEmpty(userIdStr) ? 0 : int.Parse(userIdStr);

                var (success, message) = _smsConfigService.UpsertSmsConfig(request, userId,CompanyId);
                return Ok(new { success, message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
