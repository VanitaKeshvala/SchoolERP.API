using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Net.Models
{
    public class MstSubjectViewModel
    {
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
        public List<MstSubjectViewModel> Subjects { get; set; } = new List<MstSubjectViewModel>();
    }
}
