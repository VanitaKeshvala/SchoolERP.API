using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Interfaces;
using SchoolERP.API.Models;
using SchoolERP.API.Models.Common;
using SchoolERP.API.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace SchoolERP.API.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StudentIDCardApiController : ControllerBase
    {
        private readonly IStudentIDCardService _idCardService;
        private readonly ICompanyService _companyService;
        private readonly ISessionService _sessionService;

        public StudentIDCardApiController(IStudentIDCardService idCardService, ICompanyService companyService, ISessionService sessionService)
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
                return Ok(ApiResponse<List<StudentIDCardViewModel>>.SuccessResponse(data));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<List<StudentIDCardViewModel>>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("GetByID/{id}")]
        public IActionResult GetByID(int id)
        {
            try
            {
                var data = _idCardService.GetByID(id);
                return Ok(ApiResponse<StudentIDCardViewModel>.SuccessResponse(data));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<StudentIDCardViewModel>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("Upsert")]
        public IActionResult Upsert([FromBody] StudentIDCardUpsertRequest request)
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
