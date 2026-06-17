using System;
using System.Collections.Generic;

namespace SchoolERP.Net.Models
{
    public class StudentAttendanceViewModel
    {
        public int StudentID { get; set; }
        public string? AdmissionNo { get; set; }
        public string? RollNo { get; set; }
        public string? StudentName { get; set; }
        public int AttendanceStatus { get; set; }
        public string? Note { get; set; }
        public int? AttendanceID { get; set; }
    }

    public class AttendanceUpsertRequest
    {
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public DateTime AttendanceDate { get; set; }
        public List<AttendanceRecordItem> AttendanceData { get; set; } = new();
    }

    public class AttendanceRecordItem
    {
        public int StudentID { get; set; }
        public int Status { get; set; }
        public string? Note { get; set; }
    }
}
