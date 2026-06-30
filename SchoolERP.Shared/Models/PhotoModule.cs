using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolERP.Shared.Models
{
    public enum PhotoModule
    {
        Staff,      // → wwwroot/Staff/Profile/{id}/
        Student,    // → wwwroot/Student/Profile/{id}/
        Employee,   // → wwwroot/Employee/Profile/{id}/
        User,        // → wwwroot/User/Profile/{id}/
        Parent,    // → wwwroot/Student/Profile/{id}/

    }
}
