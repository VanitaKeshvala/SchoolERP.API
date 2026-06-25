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
    public class StudentCertificateApiController : ControllerBase
    {
        private readonly IStudentCertificateService _certificateService;
        private readonly ICompanyService _companyService;
        private readonly ISessionService _sessionService;

        public StudentCertificateApiController(IStudentCertificateService certificateService, ICompanyService companyService, ISessionService sessionService)
        {
            _certificateService = certificateService;
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

                var data = _certificateService.GetAll(companyId, sessionId);
                return Ok(ApiResponse<List<StudentCertificateViewModel>>.SuccessResponse(data));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<List<StudentCertificateViewModel>>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("GetByID/{id}")]
        public IActionResult GetByID(int id)
        {
            try
            {
                var data = _certificateService.GetByID(id);
                return Ok(ApiResponse<StudentCertificateViewModel>.SuccessResponse(data));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<StudentCertificateViewModel>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("Upsert")]
        public IActionResult Upsert([FromBody] StudentCertificateUpsertRequest request)
        {
            try
            {
                int userId = GetCurrentUserId();
                int companyId = _companyService.GetUserCurrentCompany(userId) ?? 0;
                int sessionId = _sessionService.GetUserCurrentSession(userId) ?? 0;

                var (result, message) = _certificateService.Upsert(request, userId, companyId, sessionId);
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
                var (result, message) = _certificateService.Delete(id, userId);
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
                var (result, message) = _certificateService.ToggleStatus(id, isActive, userId);
                return Ok(ApiResponse<int>.SuccessResponse(result, message));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<int>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("Generate")]
        public IActionResult Generate(int studentId, int certificateId)
        {
            try
            {
                int userId = GetCurrentUserId();
                int companyId = _companyService.GetUserCurrentCompany(userId) ?? 0;
                int sessionId = _sessionService.GetUserCurrentSession(userId) ?? 0;

                var data = _certificateService.GenerateCertificate(studentId, certificateId, companyId, sessionId);
                return Ok(ApiResponse<string>.SuccessResponse(data));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 1;
        }
    }
}
