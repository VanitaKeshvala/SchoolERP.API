using System;
using System.Collections.Generic;

namespace SchoolERP.Net.Models
{
    public class RouteViewModel
    {
        public int RouteID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
    }

    public class RouteUpsertRequest
    {
        public int RouteID { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class RoutePageViewModel
    {
        public List<RouteViewModel> Items { get; set; } = new();
    }
}
