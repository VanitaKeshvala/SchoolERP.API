using System;
using System.Collections.Generic;

namespace SchoolERP.Net.Models
{
    public class RoutePickupPointViewModel
    {
        public int RoutePickupPointID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public int RouteID { get; set; }
        public int PickupPointID { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public string PickupPointName { get; set; } = string.Empty;
        public decimal? Distance { get; set; }
        public string? PickupTime { get; set; }
        public decimal? MonthlyFees { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class RoutePickupPointUpsertRequest
    {
        public int RoutePickupPointID { get; set; }
        public int RouteID { get; set; }
        public int PickupPointID { get; set; }
        public decimal? Distance { get; set; }
        public string? PickupTime { get; set; }
        public decimal? MonthlyFees { get; set; }
        public bool IsActive { get; set; }
    }

    public class RoutePickupPointPageViewModel
    {
        public List<RoutePickupPointViewModel> Items { get; set; } = new();
        public List<RouteViewModel> Routes { get; set; } = new();
        public List<PickupPointViewModel> PickupPoints { get; set; } = new();
    }
}
