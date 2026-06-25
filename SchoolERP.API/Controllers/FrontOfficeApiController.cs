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
    public class FrontOfficeApiController : ControllerBase
    {
        private readonly IFrontOfficeService _svc;
        private readonly ICompanyService _companySvc;
        private readonly ISessionService _sessionSvc;
        private readonly IUserMenuPermissionService _menuPerm;

        private const string SetupMenuPath = "/FrontOffice/Setup";
        private const string ComplaintMenuPath = "/FrontOffice/Complaint";
        private const string PostalReceiveMenuPath = "/FrontOffice/PostalReceive";
        private const string PostalDispatchMenuPath = "/FrontOffice/PostalDispatch";
        private const string PhoneCallLogMenuPath = "/FrontOffice/PhoneCallLog";

        public FrontOfficeApiController(IFrontOfficeService svc, ICompanyService companySvc, ISessionService sessionSvc, IUserMenuPermissionService menuPerm)
        {
            _svc = svc;
            _companySvc = companySvc;
            _sessionSvc = sessionSvc;
            _menuPerm = menuPerm;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "1");
        private int CompanyId => _companySvc.GetUserCurrentCompany(UserId) ?? 0;
        private int SessionId => _sessionSvc.GetUserCurrentSession(UserId) ?? 0;

        // ─── PURPOSE ────────────────────────────────────────────
        [HttpGet("GetAllPurposes")]
        public IActionResult GetAllPurposes(bool includeDeleted = false)
        {
            var data = _svc.GetAllPurposes(CompanyId, SessionId, includeDeleted);
            return Ok(ApiResponse<List<MstFOPurposeViewModel>>.SuccessResponse(data));
        }

        [HttpGet("GetPurposeByID/{id}")]
        public IActionResult GetPurposeByID(int id)
        {
            var data = _svc.GetPurposeByID(id);
            if (data == null) return NotFound(ApiResponse<MstFOPurposeViewModel>.ErrorResponse("Not found"));
            return Ok(ApiResponse<MstFOPurposeViewModel>.SuccessResponse(data));
        }

        [HttpPost("UpsertPurpose")]
        public async Task<IActionResult> UpsertPurpose([FromBody] MstFOPurposeUpsertRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var isCreate = req.PurposeID <= 0;
            if (isCreate && !await _menuPerm.Has(User, SetupMenuPath, "Add"))
                return Ok(new { success = false, message = "You do not have permission to add purposes." });
            if (!isCreate && !await _menuPerm.Has(User, SetupMenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to edit purposes." });

            var (success, message) = _svc.UpsertPurpose(req, CompanyId, SessionId, UserId);
            return Ok(new { success, message });
        }

        [HttpPost("DeletePurpose")]
        public async Task<IActionResult> DeletePurpose(List<int> id)
        {
            if (!await _menuPerm.Has(User, SetupMenuPath, "Delete"))
                return Ok(new { success = false, message = "You do not have permission to delete purposes." });

            var (success, message) = _svc.DeletePurpose(id, UserId);
            return Ok(new { success, message });
        }

        [HttpPost("TogglePurposeStatus")]
        public async Task<IActionResult> TogglePurposeStatus(int id, bool isActive)
        {
            if (!await _menuPerm.Has(User, SetupMenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to change purpose status." });

            var (success, message) = _svc.TogglePurposeStatus(id, isActive, UserId);
            return Ok(new { success, message });
        }

        // ─── COMPLAINT TYPE ─────────────────────────────────────
        [HttpGet("GetAllComplaintTypes")]
        public IActionResult GetAllComplaintTypes(bool includeDeleted = false)
        {
            var data = _svc.GetAllComplaintTypes(CompanyId, SessionId, includeDeleted);
            return Ok(ApiResponse<List<MstFOComplaintTypeViewModel>>.SuccessResponse(data));
        }

        [HttpGet("GetComplaintTypeByID/{id}")]
        public IActionResult GetComplaintTypeByID(int id)
        {
            var data = _svc.GetComplaintTypeByID(id);
            if (data == null) return NotFound(ApiResponse<MstFOComplaintTypeViewModel>.ErrorResponse("Not found"));
            return Ok(ApiResponse<MstFOComplaintTypeViewModel>.SuccessResponse(data));
        }

        [HttpPost("UpsertComplaintType")]
        public async Task<IActionResult> UpsertComplaintType([FromBody] MstFOComplaintTypeUpsertRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var isCreate = req.ComplaintTypeID <= 0;
            if (isCreate && !await _menuPerm.Has(User, SetupMenuPath, "Add"))
                return Ok(new { success = false, message = "You do not have permission to add complaint types." });
            if (!isCreate && !await _menuPerm.Has(User, SetupMenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to edit complaint types." });

            var (success, message) = _svc.UpsertComplaintType(req, CompanyId, SessionId, UserId);
            return Ok(new { success, message });
        }

        [HttpPost("DeleteComplaintType")]
        public async Task<IActionResult> DeleteComplaintType(List<int> id)
        {
            if (!await _menuPerm.Has(User, SetupMenuPath, "Delete"))
                return Ok(new { success = false, message = "You do not have permission to delete complaint types." });

            var res = _svc.DeleteComplaintType(id, UserId);
            return Ok(new { res.success, res.message });
        }

        [HttpPost("ToggleComplaintTypeStatus")]
        public async Task<IActionResult> ToggleComplaintTypeStatus(int id, bool isActive)
        {
            if (!await _menuPerm.Has(User, SetupMenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to change complaint type status." });

            var (success, message) = _svc.ToggleComplaintTypeStatus(id, isActive, UserId);
            return Ok(new { success, message });
        }

        // ─── SOURCE ─────────────────────────────────────────────
        [HttpGet("GetAllSources")]
        public IActionResult GetAllSources(bool includeDeleted = false)
        {
            var data = _svc.GetAllSources(CompanyId, SessionId, includeDeleted);
            return Ok(ApiResponse<List<MstFOSourceViewModel>>.SuccessResponse(data));
        }

        [HttpGet("GetSourceByID/{id}")]
        public IActionResult GetSourceByID(int id)
        {
            var data = _svc.GetSourceByID(id);
            if (data == null) return NotFound(ApiResponse<MstFOSourceViewModel>.ErrorResponse("Not found"));
            return Ok(ApiResponse<MstFOSourceViewModel>.SuccessResponse(data));
        }

        [HttpPost("UpsertSource")]
        public async Task<IActionResult> UpsertSource([FromBody] MstFOSourceUpsertRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var isCreate = req.SourceID <= 0;
            if (isCreate && !await _menuPerm.Has(User, SetupMenuPath, "Add"))
                return Ok(new { success = false, message = "You do not have permission to add sources." });
            if (!isCreate && !await _menuPerm.Has(User, SetupMenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to edit sources." });

            var (success, message) = _svc.UpsertSource(req, CompanyId, SessionId, UserId);
            return Ok(new { success, message });
        }

        [HttpPost("DeleteSource")]
        public async Task<IActionResult> DeleteSource(List<int> id)
        {
            if (!await _menuPerm.Has(User, SetupMenuPath, "Delete"))
                return Ok(new { success = false, message = "You do not have permission to delete sources." });

            var resp = _svc.DeleteSource(id, UserId);
            return Ok(new { resp.success, resp.message });
        }

        [HttpPost("ToggleSourceStatus")]
        public async Task<IActionResult> ToggleSourceStatus(int id, bool isActive)
        {
            if (!await _menuPerm.Has(User, SetupMenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to change source status." });

            var (success, message) = _svc.ToggleSourceStatus(id, isActive, UserId);
            return Ok(new { success, message });
        }

        // ─── REFERENCE ──────────────────────────────────────────
        [HttpGet("GetAllReferences")]
        public IActionResult GetAllReferences(bool includeDeleted = false)
        {
            var data = _svc.GetAllReferences(CompanyId, SessionId, includeDeleted);
            return Ok(ApiResponse<List<MstFOReferenceViewModel>>.SuccessResponse(data));
        }

        [HttpGet("GetReferenceByID/{id}")]
        public IActionResult GetReferenceByID(int id)
        {
            var data = _svc.GetReferenceByID(id);
            if (data == null) return NotFound(ApiResponse<MstFOReferenceViewModel>.ErrorResponse("Not found"));
            return Ok(ApiResponse<MstFOReferenceViewModel>.SuccessResponse(data));
        }

        [HttpPost("UpsertReference")]
        public async Task<IActionResult> UpsertReference([FromBody] MstFOReferenceUpsertRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var isCreate = req.ReferenceID <= 0;
            if (isCreate && !await _menuPerm.Has(User, SetupMenuPath, "Add"))
                return Ok(new { success = false, message = "You do not have permission to add references." });
            if (!isCreate && !await _menuPerm.Has(User, SetupMenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to edit references." });

            var (success, message) = _svc.UpsertReference(req, CompanyId, SessionId, UserId);
            return Ok(new { success, message });
        }

        [HttpPost("DeleteReference")]
        public async  Task<IActionResult> DeleteReference(List<int> id)
        {
            if (!await _menuPerm.Has(User, SetupMenuPath, "Delete"))
                return Ok(new { success = false, message = "You do not have permission to delete references." });

            var (success, message) = _svc.DeleteReference(id, UserId);
            return Ok(new { success, message });
        }

        [HttpPost("ToggleReferenceStatus")]
        public async Task<IActionResult> ToggleReferenceStatus(int id, bool isActive)
        {
            if (!await _menuPerm.Has(User, SetupMenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to change reference status." });

            var (success, message) = _svc.ToggleReferenceStatus(id, isActive, UserId);
            return Ok(new { success, message });
        }
        
        // ─── COMPLAINT ──────────────────────────────────────────
        [HttpGet("GetAllComplaints")]
        public IActionResult GetAllComplaints(bool includeDeleted = false)
        {
            var data = _svc.GetAllComplaints(CompanyId, SessionId, includeDeleted);
            return Ok(ApiResponse<List<FOComplaintViewModel>>.SuccessResponse(data));
        }

        [HttpGet("GetComplaintByID/{id}")]
        public IActionResult GetComplaintByID(int id)
        {
            var data = _svc.GetComplaintByID(id);
            if (data == null) return NotFound(ApiResponse<FOComplaintViewModel>.ErrorResponse("Not found"));
            return Ok(ApiResponse<FOComplaintViewModel>.SuccessResponse(data));
        }

        [HttpPost("UpsertComplaint")]
        public async Task<IActionResult> UpsertComplaint([FromBody] FOComplaintUpsertRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var isCreate = req.ComplaintID <= 0;
            if (isCreate && !await _menuPerm.Has(User, ComplaintMenuPath, "Add"))
                return Ok(new { success = false, message = "You do not have permission to add complaints." });
            if (!isCreate && !await _menuPerm.Has(User, ComplaintMenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to edit complaints." });

            var (success, message) = _svc.UpsertComplaint(req, CompanyId, SessionId, UserId);
            return Ok(new { success, message });
        }

        [HttpPost("DeleteComplaint")]
        public async Task<IActionResult> DeleteComplaint(List<int> id)
        {
            if (!await _menuPerm.Has(User, ComplaintMenuPath, "Delete"))
                return Ok(new { success = false, message = "You do not have permission to delete complaints." });

            var (success, message) = _svc.DeleteComplaint(id, UserId);
            return Ok(new { success, message });
        }

        [HttpPost("ToggleComplaintStatus")]
        public async Task<IActionResult> ToggleComplaintStatus(int id, bool isActive)
        {
            if (!await _menuPerm.Has(User, ComplaintMenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to change complaint status." });

            var (success, message) = _svc.ToggleComplaintStatus(id, isActive, UserId);
            return Ok(new { success, message });
        }

        // ─── POSTAL RECEIVE ─────────────────────────────────────
        [HttpGet("GetAllPostalReceives")]
        public IActionResult GetAllPostalReceives(bool includeDeleted = false)
        {
            var data = _svc.GetAllPostalReceives(CompanyId, SessionId, includeDeleted);
            return Ok(ApiResponse<List<FOPostalReceiveViewModel>>.SuccessResponse(data));
        }

        [HttpGet("GetPostalReceiveByID/{id}")]
        public IActionResult GetPostalReceiveByID(int id)
        {
            var data = _svc.GetPostalReceiveByID(id);
            if (data == null) return NotFound(ApiResponse<FOPostalReceiveViewModel>.ErrorResponse("Not found"));
            return Ok(ApiResponse<FOPostalReceiveViewModel>.SuccessResponse(data));
        }

        [HttpPost("UpsertPostalReceive")]
        public async Task<IActionResult> UpsertPostalReceive([FromBody] FOPostalReceiveUpsertRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var isCreate = req.PostalReceiveID <= 0;
            if (isCreate && !await _menuPerm.Has(User, PostalReceiveMenuPath, "Add"))
                return Ok(new { success = false, message = "You do not have permission to add postal receives." });
            if (!isCreate && !await _menuPerm.Has(User, PostalReceiveMenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to edit postal receives." });

            var (success, message) = _svc.UpsertPostalReceive(req, CompanyId, SessionId, UserId);
            return Ok(new { success, message });
        }

        [HttpPost("DeletePostalReceive")]
        public async Task<IActionResult> DeletePostalReceive(List<int> id)
        {
            if (!await _menuPerm.Has(User, PostalReceiveMenuPath, "Delete"))
                return Ok(new { success = false, message = "You do not have permission to delete postal receives." });

            var (success, message) = _svc.DeletePostalReceive(id, UserId);
            return Ok(new { success, message });
        }

        [HttpPost("TogglePostalReceiveStatus")]
        public async Task<IActionResult> TogglePostalReceiveStatus(int id, bool isActive)
        {
            if (!await _menuPerm.Has(User, PostalReceiveMenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to change postal receive status." });

            var (success, message) = _svc.TogglePostalReceiveStatus(id, isActive, UserId);
            return Ok(new { success, message });
        }

        // ─── POSTAL DISPATCH ─────────────────────────────────────
        [HttpGet("GetAllPostalDispatches")]
        public IActionResult GetAllPostalDispatches(bool includeDeleted = false)
        {
            var data = _svc.GetAllPostalDispatches(CompanyId, SessionId, includeDeleted);
            return Ok(ApiResponse<List<FOPostalDispatchViewModel>>.SuccessResponse(data));
        }

        [HttpGet("GetPostalDispatchByID/{id}")]
        public IActionResult GetPostalDispatchByID(int id)
        {
            var data = _svc.GetPostalDispatchByID(id);
            if (data == null) return NotFound(ApiResponse<FOPostalDispatchViewModel>.ErrorResponse("Not found"));
            return Ok(ApiResponse<FOPostalDispatchViewModel>.SuccessResponse(data));
        }

        [HttpPost("UpsertPostalDispatch")]
        public async Task<IActionResult> UpsertPostalDispatch([FromBody] FOPostalDispatchUpsertRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var isCreate = req.PostalDispatchID <= 0;
            if (isCreate && !await _menuPerm.Has(User, PostalDispatchMenuPath, "Add"))
                return Ok(new { success = false, message = "You do not have permission to add postal dispatches." });
            if (!isCreate && !await _menuPerm.Has(User, PostalDispatchMenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to edit postal dispatches." });

            var (success, message) = _svc.UpsertPostalDispatch(req, CompanyId, SessionId, UserId);
            return Ok(new { success, message });
        }

        [HttpPost("DeletePostalDispatch")]
        public async Task<IActionResult> DeletePostalDispatch(List<int> id)
        {
            if (!await _menuPerm.Has(User, PostalDispatchMenuPath, "Delete"))
                return Ok(new { success = false, message = "You do not have permission to delete postal dispatches." });

            var (success, message) = _svc.DeletePostalDispatch(id, UserId);
            return Ok(new { success, message });
        }

        [HttpPost("TogglePostalDispatchStatus")]
        public async Task<IActionResult> TogglePostalDispatchStatus(int id, bool isActive)
        {
            if (!await _menuPerm.Has(User, PostalDispatchMenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to change postal dispatch status." });

            var (success, message) = _svc.TogglePostalDispatchStatus(id, isActive, UserId);
            return Ok(new { success, message });
        }

        // ─── PHONE CALL LOG ─────────────────────────────────────
        [HttpGet("GetAllPhoneCallLogs")]
        public IActionResult GetAllPhoneCallLogs(bool includeDeleted = false)
        {
            var data = _svc.GetAllPhoneCallLogs(CompanyId, SessionId, includeDeleted);
            return Ok(ApiResponse<List<FOPhoneCallLogViewModel>>.SuccessResponse(data));
        }

        [HttpGet("GetPhoneCallLogByID/{id}")]
        public IActionResult GetPhoneCallLogByID(int id)
        {
            var data = _svc.GetPhoneCallLogByID(id);
            if (data == null) return NotFound(ApiResponse<FOPhoneCallLogViewModel>.ErrorResponse("Not found"));
            return Ok(ApiResponse<FOPhoneCallLogViewModel>.SuccessResponse(data));
        }

        [HttpPost("UpsertPhoneCallLog")]
        public async Task<IActionResult> UpsertPhoneCallLog([FromBody] FOPhoneCallLogUpsertRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var isCreate = req.PhoneCallLogID <= 0;
            if (isCreate && !await _menuPerm.Has(User, PhoneCallLogMenuPath, "Add"))
                return Ok(new { success = false, message = "You do not have permission to add phone call logs." });
            if (!isCreate && !await _menuPerm.Has(User, PhoneCallLogMenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to edit phone call logs." });

            var (success, message) = _svc.UpsertPhoneCallLog(req, CompanyId, SessionId, UserId);
            return Ok(new { success, message });
        }

        [HttpPost("DeletePhoneCallLog")]
        public async Task<IActionResult> DeletePhoneCallLog(List<int> id)
        {
            if (!await _menuPerm.Has(User, PhoneCallLogMenuPath, "Delete"))
                return Ok(new { success = false, message = "You do not have permission to delete phone call logs." });

            var (success, message) = _svc.DeletePhoneCallLog(id, UserId);
            return Ok(new { success, message });
        }

        [HttpPost("TogglePhoneCallLogStatus")]
        public async Task<IActionResult> TogglePhoneCallLogStatus(int id, bool isActive)
        {
            if (!await _menuPerm.Has(User, PhoneCallLogMenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to change phone call log status." });

            var (success, message) = _svc.TogglePhoneCallLogStatus(id, isActive, UserId);
            return Ok(new { success, message });
        }

        // ─── VISITOR BOOK ─────────────────────────────────────
        [HttpGet("GetAllVisitors")]
        public IActionResult GetAllVisitors(bool includeDeleted = false)
        {
            var data = _svc.GetAllVisitors(CompanyId, SessionId, includeDeleted);
            return Ok(ApiResponse<List<FOVisitorBookViewModel>>.SuccessResponse(data));
        }

        [HttpGet("GetVisitorByID/{id}")]
        public IActionResult GetVisitorByID(int id)
        {
            var data = _svc.GetVisitorByID(id);
            if (data == null) return NotFound(ApiResponse<FOVisitorBookViewModel>.ErrorResponse("Not found"));
            return Ok(ApiResponse<FOVisitorBookViewModel>.SuccessResponse(data));
        }

        [HttpPost("UpsertVisitor")]
        public IActionResult UpsertVisitor([FromBody] FOVisitorBookUpsertRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (success, message) = _svc.UpsertVisitor(req, CompanyId, SessionId, UserId);
            return Ok(new { success, message });
        }

        [HttpPost("DeleteVisitor")]
        public IActionResult DeleteVisitor(List<int> id)
        {
            var (success, message) = _svc.DeleteVisitor(id, UserId);
            return Ok(new { success, message });
        }

        [HttpPost("ToggleVisitorStatus")]
        public IActionResult ToggleVisitorStatus(int id, bool isActive)
        {
            var (success, message) = _svc.ToggleVisitorStatus(id, isActive, UserId);
            return Ok(new { success, message });
        }

        // ─── ADMISSION INQUIRY ──────────────────────────────────────
        [HttpGet("GetAllAdmissionInquiries")]
        public IActionResult GetAllAdmissionInquiries(DateTime? fromDate = null, DateTime? toDate = null, int sourceId = 0, int classId = 0, string? status = null)
        {
            var data = _svc.GetAllAdmissionInquiries(CompanyId, SessionId, fromDate, toDate, sourceId, classId, status);
            return Ok(ApiResponse<List<FOAdmissionInquiryViewModel>>.SuccessResponse(data));
        }

        [HttpGet("GetAdmissionInquiryByID/{id}")]
        public IActionResult GetAdmissionInquiryByID(int id)
        {
            var data = _svc.GetAdmissionInquiryByID(id);
            if (data == null) return NotFound(ApiResponse<FOAdmissionInquiryViewModel>.ErrorResponse("Not found"));
            return Ok(ApiResponse<FOAdmissionInquiryViewModel>.SuccessResponse(data));
        }

        [HttpPost("UpsertAdmissionInquiry")]
        public IActionResult UpsertAdmissionInquiry([FromBody] FOAdmissionInquiryUpsertRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (success, message) = _svc.UpsertAdmissionInquiry(req, CompanyId, SessionId, UserId);
            return Ok(new { success, message });
        }

        [HttpPost("DeleteAdmissionInquiry")]
        public IActionResult DeleteAdmissionInquiry(List<int> id)
        {
            var (success, message) = _svc.DeleteAdmissionInquiry(id, UserId);
            return Ok(new { success, message });
        }

        [HttpPost("SaveInquiryFollowUp")]
        public IActionResult SaveInquiryFollowUp([FromBody] FOInquiryFollowUpSaveRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (success, message) = _svc.SaveInquiryFollowUp(req, UserId);
            return Ok(new { success, message });
        }
    }
}
