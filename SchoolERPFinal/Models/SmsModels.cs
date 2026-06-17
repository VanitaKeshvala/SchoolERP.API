using System;
using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Net.Models
{
    /// <summary>
    /// This class represents the data structure and schema for MstSmsConfigViewModel.
    /// </summary>
    public class MstSmsConfigViewModel
    {
        public int SmsId { get; set; }
        public string? SmsGateway { get; set; }
        public string? SmsStatus { get; set; }
        public string? SmsApiUrl { get; set; }
        public string? SmsApiKey { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    /// <summary>
    /// This class represents the data structure and schema for MstSmsConfigUpsertRequest.
    /// </summary>
    public class MstSmsConfigUpsertRequest
    {
        public int SmsId { get; set; }

        [Required(ErrorMessage = "SMS Gateway is required")]
        [StringLength(100)]
        public string? SmsGateway { get; set; }

        [StringLength(100)]
        public string? SmsStatus { get; set; }

        [StringLength(500)]
        public string? SmsApiUrl { get; set; }

        [StringLength(100)]
        public string? SmsApiKey { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
