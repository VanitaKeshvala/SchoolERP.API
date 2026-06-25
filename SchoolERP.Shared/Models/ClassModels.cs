using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Shared.Models
{
    public class MstClassViewModel
    {
        public int Result { get; set; }       // maps to RESULT
        public string Message { get; set; }   // maps to MESSAGE

        public int ClassID { get; set; }
        public int CompanyID { get; set; }
        
        public int SessionID { get; set; }

        public string ClassName { get; set; } = string.Empty;
        public string SectionNames { get; set; } = string.Empty; // From STRING_AGG in SP
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        //public List<int> SectionIds { get; set; } = new List<int>();
        // ✅ MUST be string — Dapper maps "5,6" as plain string
        public string? SectionIds { get; set; }

        // ✅ Computed — Dapper never touches this
        public List<int> SectionIdList =>
            string.IsNullOrWhiteSpace(SectionIds)
                ? new List<int>()
                : SectionIds.Split(',')
                            .Where(x => int.TryParse(x.Trim(), out _))
                            .Select(x => int.Parse(x.Trim()))
                            .ToList();
    }

    public class MstClassUpsertRequest
    {
        public int ClassID { get; set; }

        [Required(ErrorMessage = "Class name is required")]
        [StringLength(200, ErrorMessage = "Class name cannot exceed 200 characters")]
        public string ClassName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public List<int> SectionIds { get; set; } = new List<int>();
    }

    public class MstClassPageViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public List<MstClassViewModel> Classes { get; set; } = new List<MstClassViewModel>();
        public List<MstSectionViewModel> AvailableSections { get; set; } = new List<MstSectionViewModel>();
    }

    public class MstClassAddViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public MstClassViewModel Classes { get; set; } = new MstClassViewModel();
        public MstClassViewModel? EditClasses { get; set; } = new MstClassViewModel();
        public List<MstSectionViewModel> AvailableSections { get; set; } = new List<MstSectionViewModel>();
    }
}
