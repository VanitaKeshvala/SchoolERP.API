using System;
using System.Collections.Generic;

namespace SchoolERP.Net.Models
{
    /// <summary>
    /// This class represents the data structure and schema for MstLanguageViewModel.
    /// </summary>
    public class MstLanguageViewModel
    {
        public int LanguageId { get; set; }
        public string LanguageTitle { get; set; }
        public string LanguageCode { get; set; }
        public string LanguageIcon { get; set; }
        public bool LanguageIsRtl { get; set; }
        public bool LanguageBase { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    /// <summary>
    /// This class represents the data structure and schema for MstLanguageUpsertRequest.
    /// </summary>
    public class MstLanguageUpsertRequest
    {
        public int LanguageId { get; set; }
        public string LanguageTitle { get; set; }
        public string LanguageCode { get; set; }
        public string LanguageIcon { get; set; }
        public bool LanguageIsRtl { get; set; }
        public bool LanguageBase { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// This class represents the data structure and schema for MstLanguagePageViewModel.
    /// </summary>
    public class MstLanguagePageViewModel
    {
        public List<MstLanguageViewModel> Languages { get; set; } = new List<MstLanguageViewModel>();
    }
}
