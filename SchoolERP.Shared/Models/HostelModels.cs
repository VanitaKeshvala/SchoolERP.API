using System;

namespace SchoolERP.Shared.Models
{
    public class RoomTypeViewModel
    {
        public int Result { get; set; }       // maps to RESULT
        public string Message { get; set; }   // maps to MESSAGE

        public int RoomTypeID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public string RoomTypeTitle { get; set; } = string.Empty;
        public string? RoomTypeDescription { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
    }

    public class RoomTypeUpsertRequest
    {
        public int RoomTypeID { get; set; }
        public string RoomTypeTitle { get; set; } = string.Empty;
        public string? RoomTypeDescription { get; set; }
        public bool IsActive { get; set; }
    }

    public class RoomTypePageViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public List<RoomTypeViewModel> Items { get; set; } = new();
    }

    // ─── HOSTEL ────────────────────────────────────────────────
    public class HostelViewModel
    {
        public int Result { get; set; }       // maps to RESULT
        public string Message { get; set; }   // maps to MESSAGE

        public int HostelID { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public string HostelName { get; set; } = string.Empty;
        public int RoomTypeID { get; set; }
        public string? RoomTypeTitle { get; set; } // Joined from RoomType table
        public string? HostelAddress { get; set; }
        public int HostelIntake { get; set; }
        public string? HostelDescription { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
    }

    public class HostelUpsertRequest
    {
        public int HostelID { get; set; }
        public string HostelName { get; set; } = string.Empty;
        public int RoomTypeID { get; set; }
        public string? HostelAddress { get; set; }
        public int HostelIntake { get; set; }
        public string? HostelDescription { get; set; }
        public bool IsActive { get; set; }
    }

    public class HostelPageViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public List<HostelViewModel> Items { get; set; } = new();
        public List<RoomTypeViewModel> RoomTypes { get; set; } = new();
    }

    // Hostel Room
    public class HostelRoomViewModel
    {
        public int Result { get; set; }       // maps to RESULT
        public string Message { get; set; }   // maps to MESSAGE

        public int RoomId { get; set; }
        public int HostelID { get; set; }
        public string? HostelName { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public int RoomTypeID { get; set; }
        public string? RoomTypeTitle { get; set; }
        public string RoomTitle { get; set; } = string.Empty;
        public int NoOfBed { get; set; }
        public decimal CostPerBed { get; set; }
        public string? RoomDescription { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class HostelRoomUpsertRequest
    {
        public int RoomId { get; set; }
        public int HostelID { get; set; }
        public int RoomTypeID { get; set; }
        public string RoomTitle { get; set; } = string.Empty;
        public int NoOfBed { get; set; }
        public decimal CostPerBed { get; set; }
        public string? RoomDescription { get; set; }
        public bool IsActive { get; set; }
    }

    public class HostelRoomPageViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public List<HostelRoomViewModel> Items { get; set; } = new();
        public List<HostelViewModel> Hostels { get; set; } = new();
        public List<RoomTypeViewModel> RoomTypes { get; set; } = new();
    }
}
