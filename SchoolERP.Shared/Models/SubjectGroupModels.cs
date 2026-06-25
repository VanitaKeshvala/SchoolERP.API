using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Shared.Models
{
    public class MstSubjectGroupViewModel
    {
        public int Result { get; set; }       // maps to RESULT
        public string Message { get; set; }   // maps to MESSAGE

        public int SubjectGroupID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ClassID { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public string SectionNames { get; set; } = string.Empty;
        public string SubjectNames { get; set; } = string.Empty;

        public string? SectionIds { get; set; } = "";

        public List<int> SectionIdsCsv =>
            string.IsNullOrWhiteSpace(SectionIds)
                ? new List<int>()
                : SectionIds.Split(',')
                               .Select(int.Parse)
                               .ToList();
        //public List<int> SectionIds { get; set; } = new List<int>();
        //public List<int> SubjectIds { get; set; } = new List<int>();

        public string SubjectIds { get; set; } = "";

        public List<int> SubjectIdsCsv =>
            string.IsNullOrWhiteSpace(SubjectIds)
                ? new List<int>()
                : SubjectIds.Split(',')
                               .Select(int.Parse)
                               .ToList();

        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
    }

    public class MstSubjectGroupUpsertRequest
    {
        public int SubjectGroupID { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Class is required")]
        public int ClassID { get; set; }

        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "At least one section is required")]
        public List<int> SectionIds { get; set; } = new List<int>();

        [Required(ErrorMessage = "At least one subject is required")]
        public List<int> SubjectIds { get; set; } = new List<int>();
    }

    public class MstSubjectGroupPageViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public List<MstSubjectGroupViewModel> SubjectGroups { get; set; } = new List<MstSubjectGroupViewModel>();
        public List<MstClassViewModel> Classes { get; set; } = new List<MstClassViewModel>();
        public List<MstSectionViewModel> Sections { get; set; } = new List<MstSectionViewModel>();
        public List<MstSubjectViewModel> Subjects { get; set; } = new List<MstSubjectViewModel>();
    }


    public class MstSubjectGroupAddViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public MstSubjectGroupViewModel SubjectGroups { get; set; } = new MstSubjectGroupViewModel();
        public List<MstClassViewModel> Classes { get; set; } = new List<MstClassViewModel>();
        public List<MstSectionViewModel> Sections { get; set; } = new List<MstSectionViewModel>();
        public List<MstSubjectViewModel> Subjects { get; set; } = new List<MstSubjectViewModel>();
        public MstSubjectGroupViewModel? EditSubjectGroups { get; set; } = new MstSubjectGroupViewModel();
    }
}
