using System;
using System.Collections.Generic;

namespace SchoolERP.API.Models
{
    public class HRDesignationViewModel
    {
        public int HRDesignationID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public string DesignationName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
    }

    public class HRDesignationUpsertRequest
    {
        public int HRDesignationID { get; set; }
        public string DesignationName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class HRDesignationPageViewModel
    {
        public List<HRDesignationViewModel> Items { get; set; } = new();
    }

    public class HRDepartmentViewModel
    {
        public int DepartmentID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        //this two key added bcz dapper direct map sp so need two check sp run proper or exiqute message show
        public bool RESULT { get; set; }
        public string Message { get; set; }
    }

    public class HRDepartmentUpsertRequest
    {
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class HRDepartmentPageViewModel
    {
        public List<HRDepartmentViewModel> Items { get; set; } = new();
    }

    public class HRLeaveTypeViewModel
    {
        public int LeaveTypeID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
    }

    public class HRLeaveTypeUpsertRequest
    {
        public int LeaveTypeID { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class HRLeaveTypePageViewModel
    {
        public List<HRLeaveTypeViewModel> Items { get; set; } = new();
    }
    
    public class HRStaffViewModel
    {
        public int StaffID { get; set; }
        public int? UserID { get; set; }
        public string StaffCode { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string MotherName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
        public string EmergencyMobileNo { get; set; } = string.Empty;
        public DateTime? DOB { get; set; }
        public DateTime? DOJ { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string MaritalStatus { get; set; } = string.Empty;
        
        // Photo
        public byte[]? PhotoDoc { get; set; }
        public string? PhotoDocType { get; set; }
        public string? PhotoDocName { get; set; }

        public string CurrentAddress { get; set; } = string.Empty;
        public string PermanentAddress { get; set; } = string.Empty;
        public int? DesignationID { get; set; }
        public string DesignationName { get; set; } = string.Empty;
        public int? DepartmentID { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;
        public string WorkExperience { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;

        // Payroll
        public string EPFNo { get; set; } = string.Empty;
        public decimal BasicSalary { get; set; }
        public string ContractType { get; set; } = string.Empty;
        public string WorkShift { get; set; } = string.Empty;
        public string WorkLocation { get; set; } = string.Empty;

        // Leaves
        public int CasualLeave { get; set; }
        public int SickLeave { get; set; }
        public int ImpWorkLeave { get; set; }

        // Bank
        public string AccountTitle { get; set; } = string.Empty;
        public string BankAccountNo { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string IFSCCode { get; set; } = string.Empty;
        public string BankBranchName { get; set; } = string.Empty;

        // Social
        public string FacebookURL { get; set; } = string.Empty;
        public string TwitterURL { get; set; } = string.Empty;
        public string LinkedinURL { get; set; } = string.Empty;
        public string InstagramURL { get; set; } = string.Empty;

        // Documents (Metadata only for VM)
        public string? ResumeDocName { get; set; }
        public string? JoiningLetterDocName { get; set; }
        public string? ResignationLetterDocName { get; set; }
        public string? OtherDocName { get; set; }

        public bool IsActive { get; set; }
        public string Username { get; set; } = string.Empty;
        public int UserTypeID { get; set; }
        public List<int> RoleIDs { get; set; } = new();
        public List<int> CompanyIDs { get; set; } = new();
        public List<string> DisplayRoles { get; set; } = new();
        public string RoleName { get; set; } = string.Empty;
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public bool IsDelete { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public List<HRStaffLeaveQuotaViewModel> LeaveQuotas { get; set; } = new();
        public List<FieldModel> SystemFields { get; set; } = new(); // NEW: For dynamic visibility
        public List<FieldModel> CustomFields { get; set; } = new();
        public List<StudentCustomFieldValueViewModel> CustomFieldValues { get; set; } = new();
    }

    public class HRStaffLeaveQuotaRequest
    {
        public int LeaveTypeID { get; set; }
        public decimal MaxDays { get; set; }
    }

    public class HRStaffLeaveQuotaViewModel
    {
        public int LeaveTypeID { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public decimal MaxDays { get; set; }
    }

    public class HRStaffUpsertRequest
    {
        public int StaffID { get; set; }
        public int UserID { get; set; }
        public Dictionary<string, string> FieldValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public string StaffCode { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string MotherName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
        public string EmergencyMobileNo { get; set; } = string.Empty;
        public DateTime? DOB { get; set; }
        public DateTime? DOJ { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string MaritalStatus { get; set; } = string.Empty;
        
        // Photo
        public string? PhotoBase64 { get; set; }
        public string? PhotoDocType { get; set; }
        public string? PhotoDocName { get; set; }

        public string CurrentAddress { get; set; } = string.Empty;
        public string PermanentAddress { get; set; } = string.Empty;
        public int? DesignationID { get; set; }
        public int? DepartmentID { get; set; }
        public string Qualification { get; set; } = string.Empty;
        public string WorkExperience { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;

        // Payroll
        public string EPFNo { get; set; } = string.Empty;
        public decimal BasicSalary { get; set; }
        public string ContractType { get; set; } = string.Empty;
        public string WorkShift { get; set; } = string.Empty;
        public string WorkLocation { get; set; } = string.Empty;

        // Leaves
        public List<HRStaffLeaveQuotaRequest> LeaveQuotas { get; set; } = new();

        // Bank
        public string AccountTitle { get; set; } = string.Empty;
        public string BankAccountNo { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string IFSCCode { get; set; } = string.Empty;
        public string BankBranchName { get; set; } = string.Empty;

        // Social
        public string FacebookURL { get; set; } = string.Empty;
        public string TwitterURL { get; set; } = string.Empty;
        public string LinkedinURL { get; set; } = string.Empty;
        public string InstagramURL { get; set; } = string.Empty;

        // Documents (Base64 for Upload)
        public string? ResumeBase64 { get; set; }
        public string? ResumeDocType { get; set; }
        public string? ResumeDocName { get; set; }

        public string? JoiningLetterBase64 { get; set; }
        public string? JoiningLetterDocType { get; set; }
        public string? JoiningLetterDocName { get; set; }

        public string? ResignationLetterBase64 { get; set; }
        public string? ResignationLetterDocType { get; set; }
        public string? ResignationLetterDocName { get; set; }

        public string? OtherDocBase64 { get; set; }
        public string? OtherDocType { get; set; }
        public string? OtherDocName { get; set; }

        public bool IsActive { get; set; }

        // User Management Fields
        public string Username { get; set; } = string.Empty;
        public string? PasswordPlain { get; set; }
        public int UserTypeID { get; set; }
        public List<int> RoleIDs { get; set; } = new();
        public List<int> CompanyIDs { get; set; } = new();
    }

    public class HRStaffChangePasswordRequest
    {
        public int StaffID { get; set; }
        public string NewPassword { get; set; } = string.Empty;
    }


    public class HRStaffPageViewModel
    {
        public string NewStaffCode { get; set; } = string.Empty;
        public List<HRStaffViewModel> StaffList { get; set; } = new();
        public List<HRDesignationViewModel> Designations { get; set; } = new();
        public List<HRDepartmentViewModel> Departments { get; set; } = new();
        public List<MstRoleViewModel> Roles { get; set; } = new();
        public List<MstUserTypeViewModel> UserTypes { get; set; } = new();
        public List<MstCompanyViewModel> Companies { get; set; } = new();
        public List<HRLeaveTypeViewModel> LeaveTypes { get; set; } = new();
        public List<FieldModel> SystemFields { get; set; } = new(); // NEW: For dynamic visibility
        public List<FieldModel> CustomFields { get; set; } = new();
        public HRStaffViewModel? EditStaff { get; set; }
        public string ViewType { get; set; } = "list";
        public string? SearchTerm { get; set; }
        public string? SelectedRole { get; set; }
        public string? SelectedDesignation { get; set; }
        public string? SelectedDepartment { get; set; }
    }

    public class HRStaffAttendanceViewModel
    {
        public int StaffID { get; set; }
        public string StaffCode { get; set; } = string.Empty;
        public string StaffName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public int StaffAttendanceID { get; set; }
        public string StaffAttendance { get; set; } = string.Empty;
        public string StaffAttendanceSource { get; set; } = "Manual";
        public string StaffAttendanceNote { get; set; } = string.Empty;
        public DateTime? LastUpdated { get; set; }
        public DateTime? StaffAttendanceDate { get; set; }
    }

    public class HRStaffAttendanceUpsertRequest
    {
        public int StaffID { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string Attendance { get; set; } = string.Empty;
        public string Source { get; set; } = "Manual";
        public string Note { get; set; } = string.Empty;
    }
    public class HRApplyLeaveViewModel
    {
        public int ApplyLeaveID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public int StaffID { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public string StaffCode { get; set; } = string.Empty;
        public int LeaveTypeID { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public DateTime ApplyDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = "Pending";
        public int? ApprovedBy { get; set; }
        public string ApprovedByName { get; set; } = string.Empty;
        public byte[]? AttachmentDoc { get; set; }
        public string? AttachmentDocType { get; set; }
        public string? AttachmentDocName { get; set; }
        public string? Note { get; set; }
    }

    public class HRApplyLeaveUpsertRequest
    {
        public int ApplyLeaveID { get; set; }
        public int StaffID { get; set; }
        public int LeaveTypeID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = "Pending";
        public string? Note { get; set; }
        public string? AttachmentBase64 { get; set; }
        public string? AttachmentDocType { get; set; }
        public string? AttachmentDocName { get; set; }
    }

    public class HRApplyLeaveStatusUpdateRequest
    {
        public int ApplyLeaveID { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    public class HRApplyLeavePageViewModel
    {
        public List<HRApplyLeaveViewModel> Leaves { get; set; } = new();
        public List<HRStaffViewModel> StaffList { get; set; } = new();
        public List<HRLeaveTypeViewModel> LeaveTypes { get; set; } = new();
    }

    // --- Payroll Models ---

    public class HRPayrollViewModel
    {
        public int PayrollID { get; set; }
        public int StaffID { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public string StaffCode { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string DesignationName { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetSalary { get; set; }
        public decimal AttendanceDays { get; set; }
        public string Status { get; set; } = "Generated";
        public string? PaymentMode { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? Note { get; set; }
    }

    public class HRPayrollGenerateRequest
    {
        public int StaffID { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class HRPayrollSearchRequest
    {
        public int? RoleID { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class HRPayrollPageViewModel
    {
        public List<HRPayrollViewModel> PayrollList { get; set; } = new();
        public List<HRStaffViewModel> StaffList { get; set; } = new();
        public List<MstRoleViewModel> Roles { get; set; } = new();
        public int SelectedMonth { get; set; }
        public int SelectedYear { get; set; }
    }

    public class HRPayrollGenerationViewModel
    {
        public HRStaffViewModel Staff { get; set; } = new();
        public List<HRAttendanceSummary> AttendanceHistory { get; set; } = new();
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal BasicSalary { get; set; }
    }

    public class HRAttendanceSummary
    {
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Present { get; set; }
        public int Late { get; set; }
        public int Absent { get; set; }
        public int HalfDay { get; set; }
        public int Holiday { get; set; }
        public int Leave { get; set; }
    }

    public class HRPayrollDetailViewModel
    {
        public int ComponentID { get; set; }
        public string ComponentName { get; set; } = string.Empty;
        public string ComponentType { get; set; } = "Earning"; // Earning or Deduction
        public decimal Amount { get; set; }
    }

    public class HRPayrollSaveRequest
    {
        public int StaffID { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetSalary { get; set; }
        public List<HRPayrollDetailViewModel> Details { get; set; } = new List<HRPayrollDetailViewModel>();
    }

    public class HRPayrollPaymentRequest
    {
        public int PayrollID { get; set; }
        public string PaymentMode { get; set; } = "Cash";
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public string? Note { get; set; }
    }

    public class HRPayrollDetailsViewModel
    {
        public HRPayrollViewModel Summary { get; set; } = new();
        public List<HRPayrollDetailViewModel> Details { get; set; } = new();
    }

    public class HRAttendanceHistoryViewModel
    {
        public List<HRAttendanceSummary> Summaries { get; set; } = new();
        public List<HRAttendanceDayStatus> Days { get; set; } = new();
    }

    public class HRAttendanceDayStatus
    {
        public int Day { get; set; }
        public int Month { get; set; }
        public string Status { get; set; } = string.Empty; // P, L, A, F, H
    }

    public class HRStaffTimelineViewModel
    {
        public int TimelineID { get; set; }
        public int StaffID { get; set; }
        public string TimelineTitle { get; set; } = string.Empty;
        public DateTime TimelineDate { get; set; }
        public string? TimelineDescription { get; set; }
        public string? TimelineAttachDocName { get; set; }
        public bool TimelineVisible { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class HRStaffTimelineUpsertRequest
    {
        public int TimelineID { get; set; }
        public int StaffID { get; set; }
        public string TimelineTitle { get; set; } = string.Empty;
        public DateTime TimelineDate { get; set; }
        public string? TimelineDescription { get; set; }
        public bool TimelineVisible { get; set; }
        
        // Attachment
        public string? AttachDocBase64 { get; set; }
        public string? AttachDocName { get; set; }
        public string? AttachDocType { get; set; }
    }

    public class HRStaffStatusToggleRequest
    {
        public int StaffId { get; set; }
        public bool IsActive { get; set; }
        public DateTime? StatusDate { get; set; }
    }

    public class DepartmentListResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<HRDepartmentViewModel> Data { get; set; }
    }
}
