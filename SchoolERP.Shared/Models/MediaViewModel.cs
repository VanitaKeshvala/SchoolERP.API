using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolERP.Shared.Models
{
    public class MediaViewModel
    {
        public int RESULT { get; set; }
        public string MESSAGE { get; set; }
        public int TOTALRECORDS { get; set; }
        public int TOTALPAGES { get; set; }
        public int CURRENTPAGE { get; set; }
        public int PAGESIZE { get; set; }

        public int MediaID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public string? Attachment { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
    }

    public class MediaRequest 
    {
        public int MediaID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public int CreatedBy { get; set; }
        public string? MediaJson { get; set; }
    }
}
