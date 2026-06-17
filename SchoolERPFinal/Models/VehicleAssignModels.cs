using System;
using System.Collections.Generic;

namespace SchoolERP.Net.Models
{
    public class VehicleAssignViewModel
    {
        public int VehicleAssignID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public int RouteID { get; set; }
        public int VehicleID { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public string? VehicleModel { get; set; }
        public string? VehicleDriverName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class VehicleAssignUpsertRequest
    {
        public int RouteID { get; set; }
        public List<int> VehicleIDs { get; set; } = new();
        public bool IsActive { get; set; } = true;
    }

    public class VehicleAssignPageViewModel
    {
        public List<VehicleAssignViewModel> Items { get; set; } = new();
        public List<RouteViewModel> Routes { get; set; } = new();
        public List<VehicleViewModel> Vehicles { get; set; } = new();
    }
}
