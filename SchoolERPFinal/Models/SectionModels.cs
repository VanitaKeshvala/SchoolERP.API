using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Net.Models
{
    public class MstSectionViewModel
    {
        public int SectionID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
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
        public List<MstSectionViewModel> Sections { get; set; } = new List<MstSectionViewModel>();
    }
}
