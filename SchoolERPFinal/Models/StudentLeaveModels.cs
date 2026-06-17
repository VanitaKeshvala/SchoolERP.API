using System;

namespace SchoolERP.Net.Models
{
    public class StudentLeaveViewModel
    {
        public int LeaveAppID { get; set; }
        public int StudentID { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public string? AdmissionNo { get; set; }
        public string? RollNo { get; set; }
        public string? StudentName { get; set; }
        public string? ClassName { get; set; }
        public string? SectionName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? Reason { get; set; }
        public int Status { get; set; }
        public DateTime ApplyDate { get; set; }
        public string? AttachmentName { get; set; }
        public string? AttachmentType { get; set; }
        public int HasAttachment { get; set; }
    }

    public class LeaveStatusUpdateRequest
    {
        public int LeaveAppID { get; set; }
        public int Status { get; set; }
    }

    public class StudentLeaveUpsertRequest
    {
        public int LeaveAppID { get; set; }
        public int StudentID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime ApplyDate { get; set; }
        public string? Reason { get; set; }
        public byte[]? Attachment { get; set; }
        public string? AttachmentType { get; set; }
        public string? AttachmentName { get; set; }
        public int Status { get; set; }
    }
}
