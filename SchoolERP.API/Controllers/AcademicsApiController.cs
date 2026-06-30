using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using SchoolERP.API.Services;

namespace SchoolERP.Net.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AcademicsApiController : ControllerBase
    {
        private readonly IAcademicsService _svc;
        private readonly ICompanyService _companySvc;
        private readonly ISessionService _sessionSvc;

        public AcademicsApiController(IAcademicsService svc, ICompanyService companySvc, ISessionService sessionSvc) 
        {
            _svc = svc;
            _companySvc = companySvc;
            _sessionSvc = sessionSvc;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "1");
        private int CompanyId => _companySvc.GetUserCurrentCompany(UserId) ?? 0;
        private int SessionId => _sessionSvc.GetUserCurrentSession(UserId) ?? 0;

        [HttpGet("GetTimeTableByClass")]
        public IActionResult GetTimeTableByClass(int classId, int sectionId)
        {
            try
            {
                var list = _svc.GetTimeTableByClass(CompanyId, SessionId, classId, sectionId);
                return Ok(ApiResponse<List<TimeTableViewModel>>.SuccessResponse(list));
            }
            catch (Exception ex) { return Ok(ApiResponse<List<TimeTableViewModel>>.ErrorResponse(ex.Message)); }
        }

        [HttpGet("GetTimeTableByStaff/{staffId}")]
        public IActionResult GetTimeTableByStaff(int staffId)
        {
            try
            {
                var list = _svc.GetTimeTableByStaff(CompanyId, SessionId, staffId);
                return Ok(ApiResponse<List<TimeTableViewModel>>.SuccessResponse(list));
            }
            catch (Exception ex) { return Ok(ApiResponse<List<TimeTableViewModel>>.ErrorResponse(ex.Message)); }
        }

        [HttpPost("UpsertTimeTable")]
        public IActionResult UpsertTimeTable([FromBody] TimeTableUpsertRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (success, message) = _svc.UpsertTimeTable(req, CompanyId, SessionId, UserId);
            return Ok(new { success, message });
        }

        [HttpPost("DeleteTimeTableSlot/{id}")]
        public IActionResult DeleteTimeTableSlot(int id)
        {
            var (success, message) = _svc.DeleteTimeTableSlot(id, UserId);
            return Ok(new { success, message });
        }

        [HttpGet("GetAllClassTeachers")]
        public IActionResult GetAllClassTeachers([FromQuery] List<int> classId, [FromQuery] List<int> sectionId, [FromQuery] int sessionID)
        {
            try
            {
                var list = _svc.GetAllClassTeachers(CompanyId, sessionID, classId,sectionId);
                return Ok(ApiResponse<List<ClassTeacherViewModel>>.SuccessResponse(list));
            }
            catch (Exception ex) { return Ok(ApiResponse<List<ClassTeacherViewModel>>.ErrorResponse(ex.Message)); }
        }

        [HttpPost("UpsertClassTeacher")]
        public IActionResult UpsertClassTeacher([FromBody] ClassTeacherUpsertRequest req)
        {
            try
            {
                if(req.CompanyID==null && req.CompanyID == 0) 
                {
                    req.CompanyID = CompanyId;
                }
                if (req.SessionID == null && req.SessionID == 0)
                {
                    req.SessionID = SessionId;
                }
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var (success, message) = _svc.UpsertClassTeacher(req, req.CompanyID, req.SessionID, UserId);
                return Ok(new { success, message });
            }
            catch (Exception ex)
            {
                return Ok(new { success=false, message=ex.Message });
            }            
        }

        [HttpPost("DeleteClassTeacher")]
        public IActionResult DeleteClassTeacher(List<int> ids)
        {
            var res = _svc.DeleteClassTeacher(ids, UserId);
            return Ok(new { res.success, res.message });
        }

        [HttpGet("GetStudentsForPromotion")]
        public IActionResult GetStudentsForPromotion(int classId, int sectionId)
        {
            try
            {
                var list = _svc.GetStudentsForPromotion(CompanyId, SessionId, classId, sectionId);
                return Ok(ApiResponse<List<StudentPromotionViewModel>>.SuccessResponse(list));
            }
            catch (Exception ex) { return Ok(ApiResponse<List<StudentPromotionViewModel>>.ErrorResponse(ex.Message)); }
        }

        [HttpPost("PromoteStudents")]
        public IActionResult PromoteStudents([FromBody] PromotionRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (success, message) = _svc.PromoteStudents(req, CompanyId, UserId);
            return Ok(new { success, message });
        }

        [HttpPost("GetAllClassTeachersWithPage")]
        public async Task<IActionResult> GetAllClassTeachersWithPage([FromBody] AcademicsSearchRequest request)
        {
            try
            {
                int userId = UserId;

                if (request.CompanyID == null)
                {
                    request.CompanyID = CompanyId;
                }
                if (request.SessionID == null)
                {
                    request.SessionID = SessionId;
                }
                if (request.CompanyID == 0 || request.SessionID == 0)
                    return Ok(ApiResponse<List<ClassTeacherViewModel>>.SuccessResponse(new List<ClassTeacherViewModel>()));

                var data = await _svc.GetAllClassTeachersWithPage(request);
                return Ok(ApiResponse<PagedResult<ClassTeacherViewModel>>.SuccessResponse(data));

            }
            catch (Exception ex)
            {
                throw;
            }

        }
    }
}
