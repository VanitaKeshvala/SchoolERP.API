using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Shared.Models
{
    // ─── PURPOSE ────────────────────────────────────────────────
    public class MstFOPurposeViewModel
    {
        public int PurposeID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class MstFOPurposeUpsertRequest
    {
        public int PurposeID { get; set; }

        [Required(ErrorMessage = "Purpose name is required")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // ─── COMPLAINT TYPE ─────────────────────────────────────────
    public class MstFOComplaintTypeViewModel
    {
        public int ComplaintTypeID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class MstFOComplaintTypeUpsertRequest
    {
        public int ComplaintTypeID { get; set; }

        [Required(ErrorMessage = "Complaint Type name is required")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // ─── SOURCE ─────────────────────────────────────────────────
    public class MstFOSourceViewModel
    {
        public int SourceID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class MstFOSourceUpsertRequest
    {
        public int SourceID { get; set; }

        [Required(ErrorMessage = "Source name is required")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // ─── REFERENCE ──────────────────────────────────────────────
    public class MstFOReferenceViewModel
    {
        public int ReferenceID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class MstFOReferenceUpsertRequest
    {
        public int ReferenceID { get; set; }

        [Required(ErrorMessage = "Reference name is required")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // ─── COMPLAINT ──────────────────────────────────────────────
    public class FOComplaintViewModel
    {
        public int ComplaintID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public int ComplaintTypeID { get; set; }
        public int SourceID { get; set; }
        
        public string ComplaintTypeName { get; set; } = string.Empty;
        public string SourceName { get; set; } = string.Empty;
        
        public string ComplaintBy { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateTime ComplaintDate { get; set; }
        
        public string? Description { get; set; }
        public string? ActionTaken { get; set; }
        public string? AssignedTo { get; set; }
        public string? Note { get; set; }
        public string? Attachment { get; set; }
        
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class FOComplaintUpsertRequest
    {
        public int ComplaintID { get; set; }

        [Required(ErrorMessage = "Complaint Type is required")]
        public int ComplaintTypeID { get; set; }

        [Required(ErrorMessage = "Source is required")]
        public int SourceID { get; set; }

        [Required(ErrorMessage = "Complaint By is required")]
        [StringLength(150)]
        public string ComplaintBy { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Complaint Date is required")]
        public DateTime ComplaintDate { get; set; }

        public string? Description { get; set; }
        public string? ActionTaken { get; set; }
        
        [StringLength(100)]
        public string? AssignedTo { get; set; }
        
        public string? Note { get; set; }
        
        [StringLength(500)]
        public string? Attachment { get; set; }
        
        public bool IsActive { get; set; } = true;
    }

    // ─── POSTAL RECEIVE ────────────────────────────────────────
    public class FOPostalReceiveViewModel
    {
        public int PostalReceiveID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public string FromTitle { get; set; } = string.Empty;
        public string? ToTitle { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Address { get; set; }
        public string? Note { get; set; }
        public DateTime Date { get; set; }
        public byte[]? Attachment { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class FOPostalReceiveUpsertRequest
    {
        public int PostalReceiveID { get; set; }

        [Required(ErrorMessage = "From Title is required")]
        [StringLength(200)]
        public string FromTitle { get; set; } = string.Empty;

        [StringLength(200)]
        public string? ToTitle { get; set; }

        [StringLength(100)]
        public string? ReferenceNo { get; set; }

        public string? Address { get; set; }
        public string? Note { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        public byte[]? Attachment { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // ─── POSTAL DISPATCH ───────────────────────────────────────
    public class FOPostalDispatchViewModel
    {
        public int PostalDispatchID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public string ToTitle { get; set; } = string.Empty;
        public string? FromTitle { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Address { get; set; }
        public string? Note { get; set; }
        public DateTime Date { get; set; }
        public byte[]? Attachment { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class FOPostalDispatchUpsertRequest
    {
        public int PostalDispatchID { get; set; }

        [Required(ErrorMessage = "To Title is required")]
        [StringLength(200)]
        public string ToTitle { get; set; } = string.Empty;

        [StringLength(200)]
        public string? FromTitle { get; set; }

        [StringLength(100)]
        public string? ReferenceNo { get; set; }

        public string? Address { get; set; }
        public string? Note { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        public byte[]? Attachment { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // ─── PHONE CALL LOG ───────────────────────────────────────
    public class FOPhoneCallLogViewModel
    {
        public int PhoneCallLogID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public DateTime? NextFollowUpDate { get; set; }
        public string? CallDuration { get; set; }
        public string? Note { get; set; }
        public string? CallType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class FOPhoneCallLogUpsertRequest
    {
        public int PhoneCallLogID { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        public string? Description { get; set; }
        public DateTime? NextFollowUpDate { get; set; }
        public string? CallDuration { get; set; }
        public string? Note { get; set; }
        public string? CallType { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // ─── VISITOR BOOK ──────────────────────────────────────────
    public class FOVisitorBookViewModel
    {
        public int VisitorBookID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public int PurposeID { get; set; }
        public string PurposeName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? IDCard { get; set; }
        public int NoOfPersons { get; set; }
        public DateTime Date { get; set; }
        public string? InTime { get; set; }
        public string? OutTime { get; set; }
        public string? Note { get; set; }
        public byte[]? Attachment { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }

        public string? MeetingWith { get; set; }
        public int? StudentID { get; set; }
        public int? StaffID { get; set; }
        public string? StudentName { get; set; }
        public string? StaffName { get; set; }
    }

    public class FOVisitorBookUpsertRequest
    {
        public int VisitorBookID { get; set; }

        [Required(ErrorMessage = "Purpose is required")]
        public int PurposeID { get; set; }

        [Required(ErrorMessage = "Visitor name is required")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? IDCard { get; set; }

        public int NoOfPersons { get; set; } = 1;

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        [StringLength(20)]
        public string? InTime { get; set; }

        [StringLength(20)]
        public string? OutTime { get; set; }

        public string? Note { get; set; }
        public byte[]? Attachment { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public bool IsActive { get; set; } = true;

        public string? MeetingWith { get; set; }
        public int? StudentID { get; set; }
        public int? StaffID { get; set; }
    }

    // ─── ADMISSION INQUIRY ──────────────────────────────────────
    public class FOAdmissionInquiryViewModel
    {
        public int InquiryID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
        public string? Note { get; set; }
        public DateTime Date { get; set; }
        public DateTime? NextFollowUpDate { get; set; }
        public int? AssignedTo { get; set; }
        public string? AssignedToName { get; set; }
        public int? ReferenceID { get; set; }
        public string? ReferenceName { get; set; }
        public int? SourceID { get; set; }
        public string? SourceName { get; set; }
        public int? ClassID { get; set; }
        public string? ClassName { get; set; }
        public int NoOfChild { get; set; }
        public string Status { get; set; } = "Active";
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastFollowUpDate { get; set; }

        public List<FOInquiryFollowUpViewModel> FollowUps { get; set; } = new();
    }

    public class FOAdmissionInquiryUpsertRequest
    {
        public int InquiryID { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required")]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        public string? Address { get; set; }
        public string? Description { get; set; }
        public string? Note { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        public DateTime? NextFollowUpDate { get; set; }
        public int? AssignedTo { get; set; }
        public int? ReferenceID { get; set; }
        public int? SourceID { get; set; }
        public int? ClassID { get; set; }
        public int NoOfChild { get; set; } = 1;

        [StringLength(20)]
        public string Status { get; set; } = "Active";
        public bool IsActive { get; set; } = true;
    }

    public class FOInquiryFollowUpViewModel
    {
        public int FollowUpID { get; set; }
        public int InquiryID { get; set; }
        public DateTime FollowUpDate { get; set; }
        public DateTime NextFollowUpDate { get; set; }
        public string Response { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class FOInquiryFollowUpSaveRequest
    {
        public int InquiryID { get; set; }

        [Required(ErrorMessage = "Follow Up Date is required")]
        public DateTime FollowUpDate { get; set; }

        [Required(ErrorMessage = "Next Follow Up Date is required")]
        public DateTime NextFollowUpDate { get; set; }

        [Required(ErrorMessage = "Response is required")]
        public string Response { get; set; } = string.Empty;

        public string? Note { get; set; }
    }

    // ─── PAGE VIEW MODEL ────────────────────────────────────────
    public class FrontOfficeSetupPageViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public List<MstFOPurposeViewModel> Purposes { get; set; } = new();
        public List<MstFOComplaintTypeViewModel> ComplaintTypes { get; set; } = new();
        public List<MstFOSourceViewModel> Sources { get; set; } = new();
        public List<MstFOReferenceViewModel> References { get; set; } = new();
    }

    public class FOComplaintPageViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public List<FOComplaintViewModel> Complaints { get; set; } = new();
        public List<MstFOComplaintTypeViewModel> ComplaintTypes { get; set; } = new();
        public List<MstFOSourceViewModel> Sources { get; set; } = new();
    }

    public class FOPostalReceivePageViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public List<FOPostalReceiveViewModel> Items { get; set; } = new();
    }

    public class FOPostalDispatchPageViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public List<FOPostalDispatchViewModel> Items { get; set; } = new();
    }

    public class FOPhoneCallLogPageViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public List<FOPhoneCallLogViewModel> Items { get; set; } = new();
    }

    public class FOVisitorBookPageViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public List<FOVisitorBookViewModel> Visitors { get; set; } = new();
        public List<MstFOPurposeViewModel> Purposes { get; set; } = new();
        public List<MstClassViewModel> Classes { get; set; } = new();
        public List<HRStaffViewModel> Staff { get; set; } = new();
    }

    public class FOAdmissionInquiryPageViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public List<FOAdmissionInquiryViewModel> Inquiries { get; set; } = new();
        public List<MstClassViewModel> Classes { get; set; } = new();
        public List<MstFOSourceViewModel> Sources { get; set; } = new();
        public List<MstFOReferenceViewModel> References { get; set; } = new();
        public List<HRStaffViewModel> Staff { get; set; } = new();
    }
}
