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
    }

    public class LibraryMemberUpsertRequest
    {
        public string? MemberType { get; set; }
        public int? StudentID { get; set; }
        public int? StaffID { get; set; }
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
    }
}
