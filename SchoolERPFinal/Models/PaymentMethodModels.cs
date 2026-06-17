using System;
using System.Collections.Generic;

namespace SchoolERP.Net.Models
{
    /// <summary>
    /// This class represents the data structure and schema for MstPaymentMethodViewModel.
    /// </summary>
    public class MstPaymentMethodViewModel
    {
        public int PaymentId { get; set; }
        public int CompanyId { get; set; }
        public string PaymentKey { get; set; } = string.Empty;
        public string PaymentSecret { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
    }

    /// <summary>
    /// This class represents the data structure and schema for MstPaymentMethodUpsertRequest.
    /// </summary>
    public class MstPaymentMethodUpsertRequest
    {
        public int PaymentId { get; set; }
        public int CompanyId { get; set; }
        public string PaymentKey { get; set; } = string.Empty;
        public string PaymentSecret { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// This class represents the data structure and schema for PaymentMethodsPageViewModel.
    /// </summary>
    public class PaymentMethodsPageViewModel
    {
        public MstPaymentMethodViewModel? PaymentMethod { get; set; }
    }
}
