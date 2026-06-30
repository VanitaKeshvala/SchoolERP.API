using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolERP.Shared.Models.Common
{
    public class CopyRequest
    {
        public int FromCompanyId { get; set; }
        public int FromSessionId { get; set; }
        public int ToCompanyId { get; set; }
        public int ToSessionId { get; set; }
        public int UserID { get; set; }

    }
}
