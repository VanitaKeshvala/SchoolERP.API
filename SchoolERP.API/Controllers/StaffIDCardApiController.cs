using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using SchoolERP.API.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace SchoolERP.API.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StaffIDCardApiController : ControllerBase
    {
        private readonly IStaffIDCardService _idCardService;
        private readonly ICompanyService _companyService;
        private readonly ISessionService _sessionService;

        public StaffIDCardApiController(IStaffIDCardService idCardService, ICompanyService companyService, ISessionService sessionService)
        {
            _idCardService = idCardService;
            _companyService = companyService;
            _sessionService = sessionService;
        }

        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            try
            {
                int userId = GetCurrentUserId();
                int companyId = _companyService.GetUserCurrentCompany(userId) ?? 0;
                int sessionId = _sessionService.GetUserCurrentSession(userId) ?? 0;

                var data = _idCardService.GetAll(companyId, sessionId);
                return Ok(ApiResponse<List<StaffIDCardViewModel>>.SuccessResponse(data));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<List<StaffIDCardViewModel>>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("GetByID/{id}")]
        public IActionResult GetByID(int id)
        {
            try
            {
                var data = _idCardService.GetByID(id);
                return Ok(ApiResponse<StaffIDCardViewModel>.SuccessResponse(data));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<StaffIDCardViewModel>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("Upsert")]
        public IActionResult Upsert([FromBody] StaffIDCardUpsertRequest request)
        {
            try
            {
                int userId = GetCurrentUserId();
                int companyId = _companyService.GetUserCurrentCompany(userId) ?? 0;
                int sessionId = _sessionService.GetUserCurrentSession(userId) ?? 0;

                var (result, message) = _idCardService.Upsert(request, userId, companyId, sessionId);
                return Ok(ApiResponse<int>.SuccessResponse(result, message));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<int>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("Delete")]
        public IActionResult Delete(List<int> id)
        {
            try
            {
                int userId = GetCurrentUserId();
                var (result, message) = _idCardService.Delete(id, userId);
                return Ok(ApiResponse<int>.SuccessResponse(result, message));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<int>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("ToggleStatus")]
        public IActionResult ToggleStatus(int id, bool isActive)
        {
            try
            {
                int userId = GetCurrentUserId();
                var (result, message) = _idCardService.ToggleStatus(id, isActive, userId);
                return Ok(ApiResponse<int>.SuccessResponse(result, message));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<int>.ErrorResponse(ex.Message));
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 1;
        }
    }
}
