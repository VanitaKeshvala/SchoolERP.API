using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Shared.Models
{
    public class MstSubjectViewModel
    {
        public int Result { get; set; }       // maps to RESULT
        public string Message { get; set; }   // maps to MESSAGE

        public int SubjectID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string? SubjectType { get; set; }
        public string? SubjectCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
    }

    public class MstSubjectUpsertRequest
    {
        public int SubjectID { get; set; }

        [Required(ErrorMessage = "Subject name is required")]
        [StringLength(200, ErrorMessage = "Subject name cannot exceed 200 characters")]
        public string SubjectName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Subject type is required")]
        public string? SubjectType { get; set; }
        public string? SubjectCode { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class MstSubjectPageViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public List<MstSubjectViewModel> Subjects { get; set; } = new List<MstSubjectViewModel>();
    }

    public class MstSubjectAddViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public MstSubjectViewModel Subjects { get; set; } = new MstSubjectViewModel();
        public MstSubjectViewModel? EditSubjects { get; set; } = new MstSubjectViewModel();
    }
}
