using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using SchoolERP.API.Services;
using System;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SchoolERP.API.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// This is a technical 'API' controller that provides data about staff and HR activities.
    /// It handles background requests for managing job titles, departments, staff attendance, 
    /// leave requests, and payroll processing.
    /// </summary>
    public class HumanResourceApiController : ControllerBase
    {
        private readonly IHumanResourceService _hrService;
        private readonly ICompanyService _companySvc;
        private readonly ISessionService _sessionSvc;
        private readonly IUserMenuPermissionService _menuPerm;
        private readonly IUserService _userService;

        private const string DesignationMenuPath = "/HumanResource/Designation";
        private const string DepartmentMenuPath = "/HumanResource/Department";
        private const string LeaveTypeMenuPath = "/HumanResource/LeaveType";
        private const string StaffMenuPath = "/HumanResource/Staffs";

        public HumanResourceApiController(IHumanResourceService hrService, ICompanyService companySvc, ISessionService sessionSvc, IUserMenuPermissionService menuPerm, IUserService userService)
        {
            _hrService = hrService;
            _companySvc = companySvc;
            _sessionSvc = sessionSvc;
            _menuPerm = menuPerm;
            _userService = userService;
        }

        private int GetUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("UserId"), out var id) ? id : 0;
        private int GetCompanyId() => _companySvc.GetUserCurrentCompany(GetUserId()) ?? 0;
        private int GetSessionId() => _sessionSvc.GetUserCurrentSession(GetUserId()) ?? 0;

        [HttpGet("GetAllDesignations")]
        /// <summary>
        /// Provides a list of all job designations (like Teacher, Clerk).
        /// </summary>
        public IActionResult GetAllDesignations()
        {
            var data = _hrService.GetAllDesignations(GetCompanyId(), GetSessionId());
            return Ok(new { success = true, data });
        }

        [HttpGet("GetDesignationByID/{id}")]
        public IActionResult GetDesignationByID(int id)
        {
            var data = _hrService.GetDesignationByID(id);
            if (data == null) return Ok(new { success = false, message = "Record not found" });
            return Ok(new { success = true, data });
        }

        [HttpPost("UpsertDesignation")]
        public async Task<IActionResult> UpsertDesignation([FromBody] HRDesignationUpsertRequest req)
        {
            var isCreate = req.HRDesignationID <= 0;
            if (isCreate && !await _menuPerm.Has(User, DesignationMenuPath, "Add"))
                return Ok(new { success = false, message = "You do not have permission to add designations." });
            if (!isCreate && !await _menuPerm.Has(User, DesignationMenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to edit designations." });

            var res = _hrService.UpsertDesignation(req, GetCompanyId(), GetSessionId(), GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        [HttpPost("DeleteDesignation")]
        public async Task<IActionResult> DeleteDesignation(List<int> id)
        {
            if (!await _menuPerm.Has(User, DesignationMenuPath, "Delete"))
                return Ok(new { success = false, message = "You do not have permission to delete designations." });

            var res = _hrService.DeleteDesignation(id, GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        [HttpPost("ToggleDesignationStatus")]
        public async Task<IActionResult> ToggleDesignationStatus(int id, bool isActive)
        {
            if (!await _menuPerm.Has(User, DesignationMenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to change designation status." });

            var res = _hrService.ToggleDesignationStatus(id, isActive, GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        [HttpGet("GetAllDepartments")]
        /// <summary>
        /// Provides a list of all school departments.
        /// </summary>
        public IActionResult GetAllDepartments()
        {
            var data = _hrService.GetAllDepartments(GetCompanyId(), GetSessionId());
            if (data.Data == null) return Ok(new { success = false, message = data.Message });
            return Ok(new { success = true, data });
        }

        [HttpGet("GetDepartmentByID/{id}")]
        public IActionResult GetDepartmentByID(int id)
        {
            var data = _hrService.GetDepartmentByID(id);
            if (data == null) return Ok(new { success = false, message = "Record not found" });
            return Ok(new { success = true, data });
        }

        [HttpPost("UpsertDepartment")]
        public async Task<IActionResult> UpsertDepartment([FromBody] HRDepartmentUpsertRequest req)
        {
            var isCreate = req.DepartmentID <= 0;
            if (isCreate && !await _menuPerm.Has(User, DepartmentMenuPath, "Add"))
                return Ok(new { success = false, message = "You do not have permission to add departments." });
            if (!isCreate && !await _menuPerm.Has(User, DepartmentMenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to edit departments." });

            var res = _hrService.UpsertDepartment(req, GetCompanyId(), GetSessionId(), GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        [HttpPost("DeleteDepartment")]
        public async Task<IActionResult> DeleteDepartment(List<int> id)
        {
            if (!await _menuPerm.Has(User, DepartmentMenuPath, "Delete"))
                return Ok(new { success = false, message = "You do not have permission to delete departments." });

            var res = _hrService.DeleteDepartment(id, GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        [HttpPost("ToggleDepartmentStatus")]
        public async Task<IActionResult> ToggleDepartmentStatus(int id, bool isActive)
        {
            if (!await _menuPerm.Has(User, DepartmentMenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to change department status." });

            var res = _hrService.ToggleDepartmentStatus(id, isActive, GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        [HttpGet("GetAllLeaveTypes")]
        /// <summary>
        /// Provides a list of all types of leave available to staff.
        /// </summary>
        public IActionResult GetAllLeaveTypes()
        {
            var data = _hrService.GetAllLeaveTypes(GetCompanyId(), GetSessionId());
            return Ok(new { success = true, data });
        }

        [HttpGet("GetLeaveTypeByID/{id}")]
        public IActionResult GetLeaveTypeByID(int id)
        {
            var data = _hrService.GetLeaveTypeByID(id);
            if (data == null) return Ok(new { success = false, message = "Record not found" });
            return Ok(new { success = true, data });
        }

        [HttpPost("UpsertLeaveType")]
        public async Task<IActionResult> UpsertLeaveType([FromBody] HRLeaveTypeUpsertRequest req)
        {
            var isCreate = req.LeaveTypeID <= 0;
            if (isCreate && !await _menuPerm.Has(User, LeaveTypeMenuPath, "Add"))
                return Ok(new { success = false, message = "You do not have permission to add leave types." });
            if (!isCreate && !await _menuPerm.Has(User, LeaveTypeMenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to edit leave types." });

            var res = _hrService.UpsertLeaveType(req, GetCompanyId(), GetSessionId(), GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        [HttpPost("DeleteLeaveType")]
        public async Task<IActionResult> DeleteLeaveType([FromBody] List<int> id)
        {
            if (!await _menuPerm.Has(User, LeaveTypeMenuPath, "Delete"))
                return Ok(new { success = false, message = "You do not have permission to delete leave types." });

            var res = _hrService.DeleteLeaveType(id, GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        [HttpPost("ToggleLeaveTypeStatus")]
        public async Task<IActionResult> ToggleLeaveTypeStatus(int id, bool isActive)
        {
            if (!await _menuPerm.Has(User, LeaveTypeMenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to change leave type status." });

            var res = _hrService.ToggleLeaveTypeStatus(id, isActive, GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        [HttpGet("GetAllStaff")]
        /// <summary>
        /// Provides a list of all staff members. 
        /// It can be filtered to show only active or only inactive staff.
        /// </summary>
        public IActionResult GetAllStaff(bool? isActive = null,int? sessionID=null)
        {
            try
            {
                if(sessionID == null) 
                {
                    sessionID = GetSessionId();
                }
                var data = _hrService.GetAllStaff(GetCompanyId(), sessionID.Value);
                if (isActive.HasValue)
                {
                    data = data.Where(s => s.IsActive == isActive.Value).ToList();
                }
                else
                {
                    data = data.ToList();
                }
                return Ok(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, ex.Message });
            }
            
        }

        [HttpGet("GetStaffByID/{id}")]
        public IActionResult GetStaffByID(int id)
        {
            var data = _hrService.GetStaffByID(id);
            if (data == null) return Ok(new { success = false, message = "Record not found" });
            return Ok(new { success = true, data });
        }

        [HttpPost("UpsertStaff")]
        public async Task<IActionResult> UpsertStaff([FromBody] HRStaffUpsertRequest req)
        {
            try
            {
                var isCreate = req.StaffID <= 0;
                if (isCreate && !await _menuPerm.Has(User, StaffMenuPath, "Add"))
                    return Ok(new { success = false, message = "You do not have permission to add staff." });
                if (!isCreate && !await _menuPerm.Has(User, StaffMenuPath, "Edit"))
                    return Ok(new { success = false, message = "You do not have permission to edit staff." });

                var res = _hrService.UpsertStaff(req, GetCompanyId(), GetSessionId(), GetUserId());
                return Ok(new { success = res.Result, message = res.Message,Data=res.StaffID });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
            
        }

        [HttpPost("DeleteStaff")]
        public async Task<IActionResult> DeleteStaff(List<int> id)
        {
            if (!await _menuPerm.Has(User, StaffMenuPath, "Delete"))
                return Ok(new { success = false, message = "You do not have permission to delete staff." });

            var res = _hrService.DeleteStaff(id, GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        [HttpGet("GetNewStaffCode")]
        public IActionResult GetNewStaffCode()
        {
            var data = _hrService.GetNewStaffCode(GetCompanyId(), GetSessionId());
            return Ok(new { success = true, data });
        }
        
        [HttpGet("GetStaffAttendance")]
        /// <summary>
        /// Provides the attendance records for staff members on a specific date.
        /// </summary>
        public IActionResult GetStaffAttendance(DateTime date, int? roleId)
        {
            var data = _hrService.GetStaffAttendance(GetCompanyId(), GetSessionId(), date, roleId);
            return Ok(new { success = true, data });
        }

        [HttpPost("SaveStaffAttendance")]
        public async Task<IActionResult> SaveStaffAttendance([FromBody] List<HRStaffAttendanceUpsertRequest> reqs)
        {
            if (!await _menuPerm.Has(User, StaffMenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to mark attendance." });

            var successCount = 0;
            var companyId = GetCompanyId();
            var sessionId = GetSessionId();
            var userId = GetUserId();

            foreach (var req in reqs)
            {
                var res = _hrService.SaveStaffAttendance(req, companyId, sessionId, userId);
                if (res.Success) successCount++;
            }

            return Ok(new { success = true, message = $"Successfully saved attendance for {successCount} staff members." });
        }

        // --- Apply Leave ---

        [HttpGet("GetAllApplyLeave")]
        /// <summary>
        /// Provides a list of all leave applications submitted by staff.
        /// </summary>
        public IActionResult GetAllApplyLeave()
        {
            var data = _hrService.GetAllApplyLeave(GetCompanyId(), GetSessionId());
            return Ok(new { success = true, data });
        }

        [HttpGet("GetApplyLeaveByID/{id}")]
        public IActionResult GetApplyLeaveByID(int id)
        {
            var data = _hrService.GetApplyLeaveByID(id);
            if (data == null) return Ok(new { success = false, message = "Record not found" });
            return Ok(new { success = true, data });
        }

        [HttpGet("GetLeaveBalance")]
        public IActionResult GetLeaveBalance(int staffId, int leaveTypeId)
        {
            var data = _hrService.GetLeaveBalance(staffId, leaveTypeId, GetCompanyId(), GetSessionId());
            return Ok(new { success = true, data });
        }

        [HttpGet("GetStaffAllLeaveBalances")]
        public IActionResult GetStaffAllLeaveBalances(int staffId)
        {
            var data = _hrService.GetStaffAllLeaveBalances(staffId, GetCompanyId(), GetSessionId());
            return Ok(new { success = true, data });
        }

        [HttpPost("UpsertApplyLeave")]
        public async Task<IActionResult> UpsertApplyLeave([FromBody] HRApplyLeaveUpsertRequest req)
        {
            // Allow management if user has permission for either ApplyLeave or ApproveLeave
            if (!await _menuPerm.Has(User, "/HumanResource/ApplyLeave", "Add") && 
                !await _menuPerm.Has(User, "/HumanResource/ApplyLeave", "Edit") &&
                !await _menuPerm.Has(User, "/HumanResource/ApproveLeave", "Edit"))
            {
                return Ok(new { success = false, message = "You do not have permission to manage leave applications." });
            }

            var res = _hrService.UpsertApplyLeave(req, GetCompanyId(), GetSessionId(), GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        [HttpPost("DeleteApplyLeave/{id}")]
        public async Task<IActionResult> DeleteApplyLeave(int id)
        {
            if (!await _menuPerm.Has(User, "/HumanResource/ApplyLeave", "Delete"))
                return Ok(new { success = false, message = "You do not have permission to delete leave applications." });

            var res = _hrService.DeleteApplyLeave(id, GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        [HttpPost("UpdateApplyLeaveStatus")]
        public async Task<IActionResult> UpdateApplyLeaveStatus([FromBody] HRApplyLeaveStatusUpdateRequest req)
        {
            if (!await _menuPerm.Has(User, "/HumanResource/ApproveLeave", "Edit"))
                return Ok(new { success = false, message = "You do not have permission to approve/disapprove leaves." });

            var res = _hrService.UpdateApplyLeaveStatus(req, GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        // --- Payroll ---

        [HttpGet("GetAllPayroll")]
        public async Task<IActionResult> GetAllPayroll(int month, int year, int? roleId)
        {
            if (!await _menuPerm.Has(User, "/HumanResource/Payroll", "View"))
                return Ok(new { success = false, message = "You do not have permission to view payroll." });

            int companyId = GetCompanyId();
            int sessionId = GetSessionId();
            var list = _hrService.GetAllPayroll(companyId, sessionId, month, year, roleId);
            return Ok(new { success = true, data = list, debug_companyId = companyId, debug_sessionId = sessionId });
        }

        [HttpPost("GeneratePayroll")]
        public async Task<IActionResult> GeneratePayroll([FromBody] HRPayrollGenerateRequest req)
        {
            if (!await _menuPerm.Has(User, "/HumanResource/Payroll", "Edit"))
                return Ok(new { success = false, message = "You do not have permission to generate payroll." });

            var res = _hrService.GeneratePayroll(req, GetCompanyId(), GetSessionId(), GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        [HttpPost("SaveDetailedPayroll")]
        public async Task<IActionResult> SaveDetailedPayroll([FromBody] HRPayrollSaveRequest req)
        {
            if (!await _menuPerm.Has(User, "/HumanResource/Payroll", "Edit"))
                return Ok(new { success = false, message = "You do not have permission to save payroll." });

            var res = _hrService.SaveDetailedPayroll(req, GetCompanyId(), GetSessionId(), GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        [HttpPost("PayPayroll")]
        public async Task<IActionResult> PayPayroll([FromBody] HRPayrollPaymentRequest req)
        {
            if (!await _menuPerm.Has(User, "/HumanResource/Payroll", "Edit"))
                return Ok(new { success = false, message = "You do not have permission to record payment." });

            var res = _hrService.MarkAsPaid(req, GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }
        
        [HttpGet("GetPayrollDetails/{id}")]
        public async Task<IActionResult> GetPayrollDetails(int id)
        {
            if (!await _menuPerm.Has(User, "/HumanResource/Payroll", "View"))
                return Ok(new { success = false, message = "You do not have permission to view payroll details." });

            var data = _hrService.GetPayrollDetails(id);
            if (data == null || data.Summary == null || data.Summary.PayrollID == 0)
            {
                string msg = "Payroll record not found.";
                if (data?.Summary?.Note != null && data.Summary.Note.StartsWith("Error"))
                    msg = data.Summary.Note;

                return Ok(new { success = false, message = msg });
            }

            return Ok(new { success = true, data });
        }

        [HttpGet("GetStaffPayroll/{staffId}")]
        public IActionResult GetStaffPayroll(int staffId)
        {
            try
            {
                var data = _hrService.GetStaffPayroll(staffId);
                return Ok(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Database Error: " + ex.Message });
            }
        }

        [HttpGet("GetStaffLeaves/{staffId}")]
        public IActionResult GetStaffLeaves(int staffId)
        {
            try
            {
                var data = _hrService.GetStaffLeaves(staffId);
                return Ok(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Database Error: " + ex.Message });
            }
        }

        [HttpGet("GetStaffAttendanceHistory/{staffId}/{year}")]
        public IActionResult GetStaffAttendanceHistory(int staffId, int year)
        {
            try
            {
                int companyId = GetCompanyId();
                var data = _hrService.GetStaffAttendanceHistory(staffId, year, companyId);
                return Ok(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Database Error: " + ex.Message });
            }
        }

        [HttpGet("GetStaffTimeline/{staffId}")]
        public IActionResult GetStaffTimeline(int staffId)
        {
            try
            {
                var data = _hrService.GetStaffTimeline(staffId);
                return Ok(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Database Error: " + ex.Message });
            }
        }

        [HttpPost("UpsertTimeline")]
        public IActionResult UpsertTimeline([FromBody] HRStaffTimelineUpsertRequest req)
        {
            try
            {
                var result = _hrService.UpsertTimeline(req, GetCompanyId(), GetSessionId(), GetUserId());
                return Ok(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Database Error: " + ex.Message });
            }
        }

        [HttpDelete("DeleteTimeline/{id}")]
        public IActionResult DeleteTimeline(int id)
        {
            try
            {
                var result = _hrService.DeleteTimeline(id, GetUserId());
                return Ok(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Database Error: " + ex.Message });
            }
        }

        [HttpGet("DownloadTimelineDoc/{id}")]
        public IActionResult DownloadTimelineDoc(int id)
        {
            var doc = _hrService.GetTimelineDocument(id);
            if (doc.Bytes == null) return NotFound();
            return File(doc.Bytes, doc.ContentType, doc.FileName);
        }

        [HttpPost("ToggleStaffStatus")]
        public IActionResult ToggleStaffStatus([FromBody] HRStaffStatusToggleRequest req)
        {
            try
            {
                var result = _hrService.ToggleStaffStatus(req.StaffId, req.IsActive, GetUserId(), req.StatusDate);
                return Ok(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Database Error: " + ex.Message });
            }
        }

        [HttpPost("ChangeStaffPassword")]
        public async Task<IActionResult> ChangeStaffPassword([FromBody] HRStaffChangePasswordRequest req)
        {
            try
            {
                if (!await _menuPerm.Has(User, StaffMenuPath, "Edit"))
                    return Ok(new { success = false, message = "You do not have permission to edit staff details." });

                if (req.StaffID <= 0 || string.IsNullOrEmpty(req.NewPassword))
                    return Ok(new { success = false, message = "Invalid request details." });

                var staff = _hrService.GetStaffByID(req.StaffID);
                if (staff == null)
                    return Ok(new { success = false, message = "Staff member not found." });

                if (!staff.UserID.HasValue || staff.UserID <= 0)
                    return Ok(new { success = false, message = "This staff member does not have a linked user login account." });

                var (result, message) = _userService.ChangePassword(staff.UserID.Value, req.NewPassword, GetUserId());
                return Ok(new { success = result == 1, message });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Database Error: " + ex.Message });
            }
        }


        [HttpPost("UpdateProfile")]
        public IActionResult UpdateProfile([FromBody] HRStaffProfileRequest req)
        {
            try
            {
                var result = _hrService.UpdateProfile(req.StaffId, req.PhotoDoc, GetUserId());
                return Ok(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Database Error: " + ex.Message });
            }
        }
    }
}
