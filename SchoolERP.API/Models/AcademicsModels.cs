using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolERP.API.Models
{
    public class TimeTableViewModel
    {
        public int TimeTableID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public int ClassID { get; set; }
        public string? ClassName { get; set; }
        public int SectionID { get; set; }
        public string? SectionName { get; set; }
        public int SubjectID { get; set; }
        public string? SubjectName { get; set; }
        public int StaffID { get; set; }
        public string? StaffName { get; set; }
        public string Day { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string? RoomNo { get; set; }
        public bool IsActive { get; set; }
    }

    public class TimeTableUpsertRequest
    {
        public int TimeTableID { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int SubjectID { get; set; }
        public int StaffID { get; set; }
        public string Day { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string? RoomNo { get; set; }
    }

    public class AddTimeTablePageViewModel
    {
        public List<MstClassViewModel> Classes { get; set; } = new();
        public List<MstSectionViewModel> Sections { get; set; } = new();
        public List<MstSubjectViewModel> Subjects { get; set; } = new();
        public List<HRStaffViewModel> Staff { get; set; } = new();
        public List<MstSubjectGroupViewModel> SubjectGroups { get; set; } = new();
        public List<TimeTableViewModel> TimeTableSlots { get; set; } = new();
        public int SelectedClassId { get; set; }
        public int SelectedSectionId { get; set; }
        public int SelectedSubjectGroupId { get; set; }
    }

    public class TeacherTimeTablePageViewModel
    {
        public List<HRStaffViewModel> Staff { get; set; } = new();
        public List<TimeTableViewModel> TimeTableSlots { get; set; } = new();
        public int SelectedStaffId { get; set; }
    }

    public class ClassTeacherViewModel
    {
        public int ClassTeacherID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public int ClassID { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int SectionID { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int StaffID { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class ClassTeacherUpsertRequest
    {
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public List<int> StaffIDs { get; set; } = new();
    }

    public class AssignClassTeacherPageViewModel
    {
        public List<ClassTeacherViewModel> Assignments { get; set; } = new();
        public List<MstClassViewModel> Classes { get; set; } = new();
        public List<HRStaffViewModel> Staff { get; set; } = new();
    }

    public class StudentPromotionViewModel
    {
        public int StudentID { get; set; }
        public string AdmissionNo { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public DateTime? DOB { get; set; }
        public string Result { get; set; } = "Pass"; // Pass, Fail
        public string NextStatus { get; set; } = "Continue"; // Continue, Leave
    }

    public class PromotionRequest
    {
        public int CurrentClassID { get; set; }
        public int CurrentSectionID { get; set; }
        public int NextSessionID { get; set; }
        public int NextClassID { get; set; }
        public int NextSectionID { get; set; }
        public List<StudentPromotionItem> Students { get; set; } = new();
    }

    public class StudentPromotionItem
    {
        public int StudentID { get; set; }
        public string Result { get; set; } = "Pass";
        public string NextStatus { get; set; } = "Continue";
    }

    public class PromoteStudentsPageViewModel
    {
        public List<StudentPromotionViewModel> Students { get; set; } = new();
        public List<MstClassViewModel> Classes { get; set; } = new();
        public List<MstSessionViewModel> Sessions { get; set; } = new();
        
        // Filter values
        public int? SelectedClassId { get; set; }
        public int? SelectedSectionId { get; set; }
        public int? NextSessionId { get; set; }
        public int? NextClassId { get; set; }
        public int? NextSectionId { get; set; }
    }
}
