using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Shared.Models
{
    public class HomeworkViewModel
    {
        public int RESULT { get; set; }
        public string MESSAGE { get; set; }
        public int TOTALRECORDS { get; set; }
        public int TOTALPAGES { get; set; }
        public int CURRENTPAGE { get; set; }
        public int PAGESIZE { get; set; }

        public int HomeworkID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public int ClassID { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int SectionID { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int SubjectGroupID { get; set; }
        public string SubjectGroupName { get; set; } = string.Empty;
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public DateTime HomeworkDate { get; set; }
        public DateTime SubmissionDate { get; set; }
        public DateTime? EvaluationDate { get; set; }
        public decimal? MaxMarks { get; set; }
        public string? AttachmentPath { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
    }

    public class HomeworkUpsertRequest
    {
        public int HomeworkID { get; set; }

        [Required(ErrorMessage = "Class is required")]
        public int ClassID { get; set; }

        [Required(ErrorMessage = "Section is required")]
        public int SectionID { get; set; }

        [Required(ErrorMessage = "Subject Group is required")]
        public int SubjectGroupID { get; set; }

        [Required(ErrorMessage = "Subject is required")]
        public int SubjectID { get; set; }

        [Required(ErrorMessage = "Homework Date is required")]
        public DateTime HomeworkDate { get; set; }

        [Required(ErrorMessage = "Submission Date is required")]
        public DateTime SubmissionDate { get; set; }

        public decimal? MaxMarks { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Attachment will be handled separately via file upload if needed, 
        // or passed as a path string.
        public string? AttachmentPath { get; set; }
    }

    public class HomeworkPageViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public List<HomeworkViewModel> Homeworks { get; set; } = new List<HomeworkViewModel>();
        public List<MstClassViewModel> Classes { get; set; } = new List<MstClassViewModel>();
        public List<MstCompanyViewModel> Companies { get; set; } = new();
        public List<MstSessionViewModel> Sessions { get; set; } = new();

        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public int? CompanyId { get; set; }
        public int? SessionId { get; set; }
        public int? SectionID { get; set; }
    }

    public class HomeworkAddViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public HomeworkViewModel Homeworks { get; set; } = new HomeworkViewModel();
        public HomeworkViewModel EditHomeworks { get; set; } = new HomeworkViewModel();
        public List<MstClassViewModel> Classes { get; set; } = new List<MstClassViewModel>();
    }
}
