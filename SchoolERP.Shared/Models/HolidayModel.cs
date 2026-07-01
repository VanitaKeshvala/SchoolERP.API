using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolERP.Shared.Models
{
    public class HolidayModel
    {
        public int HolidayID { get; set; }

        public int? CompanyID { get; set; }

        public int? SessionID { get; set; }

        public int? HolidayTypeID { get; set; }

        public string? HolidayName { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public int? NoOfDays { get; set; }

        public bool IsActive { get; set; }

        public bool IsDelete { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        public int? ModifiedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }
    }
}
