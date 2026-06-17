using System;
using System.Collections.Generic;

namespace SchoolERP.Net.Models
{
    public class AccountHeadViewModel
    {
        public int AccountHeadID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public string HeadName { get; set; } = string.Empty;
        public string? HeadDescription { get; set; }
        public string HeadType { get; set; } = string.Empty; // 'Income' or 'Expense'
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
    }

    public class AccountHeadUpsertRequest
    {
        public int AccountHeadID { get; set; }
        public string HeadName { get; set; } = string.Empty;
        public string? HeadDescription { get; set; }
        public string HeadType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class AccountHeadPageViewModel
    {
        public List<AccountHeadViewModel> Items { get; set; } = new();
        public string HeadType { get; set; } = string.Empty; // To distinguish between Income/Expense in UI
    }
}
