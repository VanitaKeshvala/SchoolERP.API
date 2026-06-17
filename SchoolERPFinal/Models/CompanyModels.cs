using System;
using System.Collections.Generic;

namespace SchoolERP.Net.Models
{
    public class MstCompanyViewModel
    {
        public int CompanyId { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string SchoolCode { get; set; }
        public int? ParentCompanyId { get; set; }
        public string? ParentName { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int SessionId { get; set; }
        public string? SessionTitle { get; set; }
        public int CurrencyId { get; set; }
        public string? CurrencyCode { get; set; }
        public string? SessionStartMonth { get; set; }
        public string? DateFormat { get; set; }
        public string? Timezone { get; set; }
        public string? StartDayOfWeek { get; set; }
        public string? CurrencyFormat { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public int Level { get; set; }
        public bool HasChildren { get; set; }
    }

    public class MstCompanyUpsertRequest
    {
        public int CompanyId { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string SchoolCode { get; set; }
        public int? ParentCompanyId { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int SessionId { get; set; }
        public int CurrencyId { get; set; }
        public string? SessionStartMonth { get; set; }
        public string? DateFormat { get; set; }
        public string? Timezone { get; set; }
        public string? StartDayOfWeek { get; set; }
        public string? CurrencyFormat { get; set; }
        public bool IsActive { get; set; }
    }

    public class MstCompanyPageViewModel
    {
        public List<MstCompanyViewModel> Companies { get; set; } = new();
        public List<MstCompanyViewModel> ParentCompanies { get; set; } = new();
        public List<MstSessionViewModel> Sessions { get; set; } = new();
        public List<MstCurrencyViewModel> Currencies { get; set; } = new();
    }

    public class SetCurrentCompanyRequest
    {
        public int CompanyId { get; set; }
    }
}
