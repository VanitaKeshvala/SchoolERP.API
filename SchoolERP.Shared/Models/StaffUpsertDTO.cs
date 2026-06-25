using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolERP.Shared.Models
{
    public class StaffUpsertDTO
    {
        public int Result { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StaffID { get; set; }
    }
}
