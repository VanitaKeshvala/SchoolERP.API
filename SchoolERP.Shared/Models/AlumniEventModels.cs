using System;

namespace SchoolERP.Shared.Models
{
    public class AlumniEventViewModel
    {
        public int EventID { get; set; }
        public string? EventTitle { get; set; }
        public string? EventDescription { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? Location { get; set; }
        public string? EventPhotoName { get; set; }
        public string? EventPhotoType { get; set; }
        public int HasPhoto { get; set; }
        public int EventFor { get; set; }
        public int? SessionID { get; set; }
        public int? ClassID { get; set; }
        public string? SectionIDs { get; set; }
        public string? SessionName { get; set; }
        public string? ClassName { get; set; }
        public string? SectionNames { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class AlumniEventUpsertRequest
    {
        public int EventID { get; set; }
        public string? EventTitle { get; set; }
        public string? EventDescription { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? Location { get; set; }
        public byte[]? EventPhoto { get; set; }
        public string? EventPhotoType { get; set; }
        public string? EventPhotoName { get; set; }
        public int EventFor { get; set; }
        public int? SessionID { get; set; }
        public int? ClassID { get; set; }
        public string? SectionIDs { get; set; }
    }
}
