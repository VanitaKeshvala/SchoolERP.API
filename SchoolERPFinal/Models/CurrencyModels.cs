using System;
using System.Collections.Generic;

namespace SchoolERP.Net.Models
{
    /// <summary>
    /// This class represents the data structure and schema for MstCurrencyViewModel.
    /// </summary>
    public class MstCurrencyViewModel
    {
        public int CurrencyId { get; set; }
        public string CurrencyTitle { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;
        public decimal CurrencyConvRate { get; set; }
        public bool CurrencyBase { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    /// <summary>
    /// This class represents the data structure and schema for MstCurrencyUpsertRequest.
    /// </summary>
    public class MstCurrencyUpsertRequest
    {
        public int CurrencyId { get; set; }
        public string CurrencyTitle { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;
        public decimal CurrencyConvRate { get; set; }
        public bool CurrencyBase { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// This class represents the data structure and schema for MstCurrencyPageViewModel.
    /// </summary>
    public class MstCurrencyPageViewModel
    {
        public List<MstCurrencyViewModel> Currencies { get; set; } = new();
    }
}
