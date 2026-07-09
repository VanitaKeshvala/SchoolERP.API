using System;
using System.Collections.Generic;

namespace SchoolERP.Shared.Models
{
    public class BookViewModel
    {
        public int BookID { get; set; }
        public string? BookTitle { get; set; }
        public string? BookNo { get; set; }
        public string? ISBNNo { get; set; }
        public string? Publisher { get; set; }
        public string? Author { get; set; }
        public string? Subject { get; set; }
        public string? RackNo { get; set; }
        public int TotalQty { get; set; }
        public int AvailableQty { get; set; }
        public decimal BookPrice { get; set; }
        public DateTime? PostDate { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }

        public int Result { get; set; }       // maps to RESULT
        public string Message { get; set; }   // maps to MESSAGE
        // Must exist — mapped from TOTALCOUNT column in SP
        public int TOTALRECORDS { get; set; }   // ← TOTALCOUNT
        public int CURRENTPAGE { get; set; }   // ← PAGEINDEX
        public int PageSize { get; set; }   // ← PAGESIZE
        public int TotalPages { get; set; }   // ← TOTALPAGES
    }

    public class LibraryMemberViewModel
    {
        public int LibraryMemberID { get; set; }
        public int? StudentID { get; set; }
        public int? StaffID { get; set; }
        public string? LibraryCardNo { get; set; }
        public string? AdmissionNo { get; set; }
        public string? Name { get; set; }
        public string? MemberType { get; set; }
        public string? ClassName { get; set; }
        public string? FatherName { get; set; }
        public DateTime? DOB { get; set; }
        public string? Gender { get; set; }
        public string? MobileNo { get; set; }
        public DateTime? RegisteredOn { get; set; }

        public int Result { get; set; }       // maps to RESULT
        public string Message { get; set; }   // maps to MESSAGE
        // Must exist — mapped from TOTALCOUNT column in SP
        public int TOTALRECORDS { get; set; }   // ← TOTALCOUNT
        public int CURRENTPAGE { get; set; }   // ← PAGEINDEX
        public int PageSize { get; set; }   // ← PAGESIZE
        public int TotalPages { get; set; }   // ← TOTALPAGES
    }

    public class LibraryMemberUpsertRequest
    {
        public string? MemberType { get; set; }
        public int? StudentID { get; set; }
        public int? StaffID { get; set; }
        public int? LibraryMemberID { get; set; }
        public string? LibraryCardNo { get; set; }
    }

    public class MembershipSearchViewModel
    {
        public int ID { get; set; }
        public string? AdmissionNo { get; set; }
        public string? Name { get; set; }
        public string? ExtraInfo { get; set; }
    }

    public class BookUpsertRequest
    {
        public int BookID { get; set; }
        public string? BookTitle { get; set; }
        public string? BookNo { get; set; }
        public string? ISBNNo { get; set; }
        public string? Publisher { get; set; }
        public string? Author { get; set; }
        public string? Subject { get; set; }
        public string? RackNo { get; set; }
        public int TotalQty { get; set; }
        public decimal BookPrice { get; set; }
        public DateTime? PostDate { get; set; }
        public string? Description { get; set; }
    }

    public class IssueReturnViewModel
    {
        public int IssueReturnID { get; set; }
        public string? BookTitle { get; set; }
        public string? BookNo { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueReturnDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int Status { get; set; }

        public int Result { get; set; }       // maps to RESULT
        public string Message { get; set; }   // maps to MESSAGE
        // Must exist — mapped from TOTALCOUNT column in SP
        public int TOTALRECORDS { get; set; }   // ← TOTALCOUNT
        public int CURRENTPAGE { get; set; }   // ← PAGEINDEX
        public int PageSize { get; set; }   // ← PAGESIZE
        public int TotalPages { get; set; }   // ← TOTALPAGES
    }

    public class IssueReturnUpsertRequest
    {
        public int? IssueReturnID { get; set; }
        public int LibraryMemberID { get; set; }
        public int BookID { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueReturnDate { get; set; }
        public DateTime? ReturnDate { get; set; }
    }

    public class MemberDetailsViewModel
    {
        public int LibraryMemberID { get; set; }
        public string? LibraryCardNo { get; set; }
        public string? MemberType { get; set; }
        public string? AdmissionNo { get; set; }
        public string? MemberName { get; set; }
        public string? Gender { get; set; }
        public string? MobileNo { get; set; }
        public string? StudentPhoto { get; set; }
        public string? StudentFullName { get; set; }
        public string? StudentEmail { get; set; }

        public string? StaffPhoto { get; set; }
        public string? StaffFullName { get; set; }
        public string? StaffEmail { get; set; }
    }

    public class LibraryMember
    {
        public int LibraryMemberID { get; set; }
        public string MemberType { get; set; }      // "Student" or "Staff"
        public int? StudentID { get; set; }          // nullable - only set when MemberType = Student
        public int? StaffID { get; set; }            // nullable - only set when MemberType = Staff
        public string LibraryCardNo { get; set; }
        public int CompanyID { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
    }

    public class LibrarySearchRequest
    {
        public int CompanyID { get; set; }
        public string? SearchKeyword { get; set; }
        public int? PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class StudentsMembershipSearchRequest
    {
        public int CompanyID { get; set; }
        public int SessionId { get; set; }
        public int? ClassId { get; set; }
        public int? SectionId { get; set; }
        public string? SearchKeyword { get; set; }
        public int? PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class StaffLibraryMemberSearchModel
    {
        public int CompanyID { get; set; }
        public int? DepartmentID { get; set; }
        public string? SearchTerm { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class StaffLibraryMember
    {
        public int Result { get; set; }       // maps to RESULT
        public string Message { get; set; }   // maps to MESSAGE

        // Must exist — mapped from TOTALRECORDS column in SP
        public int TOTALRECORDS { get; set; }
        public int CURRENTPAGE { get; set; }   // ← CURRENTPAGE
        public int PageSize { get; set; }      // ← PAGESIZE
        public int TotalPages { get; set; }    // ← TOTALPAGES

        public int StaffID { get; set; }
        public string AdmissionNo { get; set; }
        public string StaffName { get; set; }
        public string DepartmentName { get; set; }
        public string Gender { get; set; }
        public string MobileNo { get; set; }
        public int LibraryMemberID { get; set; }
        public string LibraryCardNo { get; set; }
        public DateTime? RegisteredOn { get; set; }
        public bool? IsActive { get; set; }
    }

    public class StaffMemberPageViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public List<HRDepartmentViewModel> Departments { get; set; } = new();
        public List<StaffLibraryMember> Items { get; set; } = new();
        public List<MstCompanyViewModel> Companies { get; set; } = new();

        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public int? CompanyId { get; set; }
        public int? DepartmentID { get; set; }
    }

    public class IssuePageViewModel 
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public MemberDetailsViewModel MemberDetails { get; set; } = new();
        public List<IssueReturnViewModel> IssueReturn { get; set; } = new();

        public List<MstCompanyViewModel> Companies { get; set; } = new();

        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public int? CompanyId { get; set; }
        public int? LibraryMemberID { get; set; }
    }
    public class IssueBookAddViewModel 
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public List<BookViewModel> BookViewModels { get; set; }
        public BookViewModel EditIssueBooks { get; set; }
        public int? LibraryMemberID { get; set; }
        public int? IssueReturnID { get; set; }
    }
    public class IssueBookSearchModel
    {
        public int LibraryMemberID { get; set; }
        public int CompanyID { get; set; }
        public string? SearchkeyWord{ get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;
    }
    public class MemberSearchModel
    {
        public int CompanyID { get; set; }
        public string? SearchkeyWord { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;
    }
    public class ReturnBookIssue
    {
        public int issueId { get; set; }
        public DateTime returnDate { get; set; }
    }

    public class MemberPageViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public List<LibraryMemberViewModel> Items { get; set; } = new();
        public List<MstCompanyViewModel> Companies { get; set; } = new();

        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public int? CompanyId { get; set; }
    }
}
