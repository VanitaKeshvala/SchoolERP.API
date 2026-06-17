using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

using System.Linq;

namespace SchoolERP.Net.Models
{
    public class AccountEntryViewModel
    {
        public int AccountEntryID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public int AccountHeadID { get; set; }
        public string? HeadName { get; set; }
        public string EntryType { get; set; } = string.Empty; // 'Income' / 'Expense'
        public string Name { get; set; } = string.Empty;
        public string? InvoiceNo { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public byte[]? AttachDoc { get; set; }
        public string? AttachDocType { get; set; }
        public string? AttachDocName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
    }

    public class AccountEntryUpsertRequest
    {
        public int AccountEntryID { get; set; }
        public int AccountHeadID { get; set; }
        public string EntryType { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? InvoiceNo { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public byte[]? AttachDoc { get; set; }
        public string? AttachDocType { get; set; }
        public string? AttachDocName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class AccountEntryPageViewModel
    {
        public List<AccountEntryViewModel> Items { get; set; } = new();
        public List<AccountHeadViewModel> Heads { get; set; } = new();
        public string EntryType { get; set; } = string.Empty;
    }

    public class AccountEntryFormModel
    {
        public int AccountEntryID { get; set; }
        public int AccountHeadID { get; set; }
        public string EntryType { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? InvoiceNo { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public IFormFile? Attachment { get; set; }
    }

    public class AccountEntryAddViewModel
    {
        public AccountEntryViewModel Entry { get; set; } = new();
        public List<AccountHeadViewModel> Heads { get; set; } = new();
        public string EntryType { get; set; } = string.Empty;
    }

    public class AccountEntrySearchRequest
    {
        public string SearchType { get; set; } = string.Empty;
        public string? DateFrom { get; set; }
        public string? DateTo { get; set; }
        public string EntryType { get; set; } = string.Empty;
    }

    public class AccountEntrySearchViewModel
    {
        public List<AccountEntryViewModel> Results { get; set; } = new();
        public string EntryType { get; set; } = string.Empty;
        public string SearchType { get; set; } = string.Empty;
        public string? DateFrom { get; set; }
        public string? DateTo { get; set; }
        public decimal TotalAmount => Results.Sum(x => x.Amount);
    }
}
