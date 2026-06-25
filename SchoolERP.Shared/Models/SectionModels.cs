using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Shared.Models
{
    public class MstSectionViewModel
    {
        public int Result { get; set; }       // maps to RESULT
        public string Message { get; set; }   // maps to MESSAGE

        public int SectionID { get; set; }
        // Dapper maps "5,6" here
    public string? SectionIds { get; set; }

    // ✅ Parse "5,6" → [5, 6] for Contains check
    public List<int> SectionIdList =>
        string.IsNullOrWhiteSpace(SectionIds)
            ? new List<int>()
            : SectionIds.Split(',')
                        .Where(x => int.TryParse(x.Trim(), out _))
                        .Select(x => int.Parse(x.Trim()))
                        .ToList();
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }

        public MstSectionViewModel? EditSections { get; set; }
    }

    public class MstSectionUpsertRequest
    {
        public int SectionID { get; set; }

        [Required(ErrorMessage = "Section name is required")]
        [StringLength(200, ErrorMessage = "Section name cannot exceed 200 characters")]
        public string SectionName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class MstSectionPageViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public List<MstSectionViewModel> Sections { get; set; } = new List<MstSectionViewModel>();
    }

    // ── Result model for copy SP ─────────────────────────────────────────────
    public class CopyResult
    {
        public int Result { get; set; }
        public string Message { get; set; } = string.Empty;
        public int Inserted { get; set; }
        public int Skipped { get; set; }
    }

    public class SectionCopyRequest
    {
        public string SectionIds { get; set; } = string.Empty;
        public int TargetSessionId { get; set; }
    }
}
