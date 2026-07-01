using System;
using System.ComponentModel.Design;

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
        public int BedCapacity { get; set; }
        public decimal RentPerBed { get; set; }
        public int RoomCoolingTypeId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }

        // Must exist — mapped from TOTALCOUNT column in SP
        public int TOTALRECORDS { get; set; }   // ← TOTALCOUNT
        public int CURRENTPAGE { get; set; }   // ← PAGEINDEX
        public int PageSize { get; set; }   // ← PAGESIZE
        public int TotalPages { get; set; }   // ← TOTALPAGES

        public decimal CostPerBed { get; set; }
        public decimal SecurityAmount { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTO { get; set; }

        public string? RoomCoolingTypeName { get; set; }

    }

    public class RoomTypeUpsertRequest
    {
        public int RoomTypeID { get; set; }
        public string RoomTypeTitle { get; set; } = string.Empty;
        public string? RoomTypeDescription { get; set; }
        public bool IsActive { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }

        public int BedCapacity { get; set; }
        public decimal RentPerBed { get; set; }
        public int RoomCoolingTypeId { get; set; }

        public decimal CostPerBed { get; set; }
        public decimal SecurityAmount { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTO { get; set; }
    }

    public class RoomTypePageViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public List<RoomTypeViewModel> Items { get; set; } = new();

        public List<MstCompanyViewModel> Companies { get; set; } = new();
        public List<MstSessionViewModel> Sessions { get; set; } = new();

        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public int? CompanyId { get; set; }
        public int? SessionId { get; set; }
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
        public string? HostelTypeName { get; set; } // Joined from RoomType table
        public string? HostelAddress { get; set; }
        public int HostelIntake { get; set; }
        public string? HostelDescription { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }

        public string HostelCode { get; set; }
        public int HostelTypeID { get; set; }
        public string WardenName { get; set; }
        public string WardenContact { get; set; }
        public string EmergencyContact { get; set; }
        public string HostelEmail { get; set; }
        public string HostelRules { get; set; }



        // Must exist — mapped from TOTALCOUNT column in SP
        public int TOTALRECORDS { get; set; }   // ← TOTALCOUNT
        public int CURRENTPAGE { get; set; }   // ← PAGEINDEX
        public int PageSize { get; set; }   // ← PAGESIZE
        public int TotalPages { get; set; }   // ← TOTALPAGES
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
        public int CompanyID { get; set; }
        public int SessionID { get; set; }

        public string HostelCode { get; set;}
        public int HostelTypeID { get; set; }
        public string WardenName { get; set; }
        public string WardenContact { get; set; }
        public string EmergencyContact { get; set; }
        public string HostelEmail { get; set; }
        public string HostelRules { get; set; }

    }

    public class HostelPageViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public List<HostelViewModel> Items { get; set; } = new();
        public List<RoomTypeViewModel> RoomTypes { get; set; } = new();

        public List<MstCompanyViewModel> Companies { get; set; } = new();
        public List<MstSessionViewModel> Sessions { get; set; } = new();

        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public int? CompanyId { get; set; }
        public int? SessionId { get; set; }
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
        public string HostelTypeName { get; set; } = string.Empty;
        public int NoOfBed { get; set; }
        public decimal CostPerBed { get; set; }
        public string? RoomDescription { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }

        // Must exist — mapped from TOTALCOUNT column in SP
        public int TOTALRECORDS { get; set; }   // ← TOTALCOUNT
        public int CURRENTPAGE { get; set; }   // ← PAGEINDEX
        public int PageSize { get; set; }   // ← PAGESIZE
        public int TotalPages { get; set; }   // ← TOTALPAGES

        public int FloorNumber { get; set; }
        public int AllowExtraBed { get; set; }
        public int MaxExtraBeds { get; set; }
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
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public int? FloorNumber { get; set; }
        public bool? AllowExtraBed { get; set; }
        public int? MaxExtraBeds { get; set; }
    }

    public class HostelRoomPageViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public List<HostelRoomViewModel> Items { get; set; } = new();
        public List<HostelViewModel> Hostels { get; set; } = new();
        public List<RoomTypeViewModel> RoomTypes { get; set; } = new();

        public List<MstCompanyViewModel> Companies { get; set; } = new();
        public List<MstSessionViewModel> Sessions { get; set; } = new();

        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public int? CompanyId { get; set; }
        public int? SessionId { get; set; }
    }

    public class RoomTypeAddViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public RoomTypeViewModel Items { get; set; } = new();
        public RoomTypeViewModel? EditRoomType { get; set; } = new();
        public List<RoomCoolingType>? RoomCoolingType { get; set; } = new();
    }

    public class HostelAddViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public HostelViewModel Items { get; set; } = new();
        public List<RoomTypeViewModel> RoomTypes { get; set; } = new();
        public List<HostelTypeModel> HostelTypes { get; set; } = new();
        public HostelViewModel? EditHostel { get; set; } = new();
    }

    public class HotelSearchRequest
    {
        public int CompanyID { get; set; }
        public int SessionID { get; set; }        
        public string? SearchKeyword { get; set; }
        public int? HostelID { get; set; }
        public int? RoomTypeID { get; set; }
        public int? PageNumber { get; set; }
        public int PageSize { get; set; }
        public int? UserId { get; set; }
        public int? SectionID { get; set; }
    }

    public class HostelRoomAddViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public HostelRoomViewModel Items { get; set; } = new();
        public List<HostelViewModel> Hostels { get; set; } = new();
        public List<RoomTypeViewModel> RoomTypes { get; set; } = new();
        public HostelRoomViewModel EditHostelRoom { get; set; } = new();
    }

    public class HostelRoomRateViewModel 
    {
        public int RateID { get; set; }
        public int RoomTypeID { get; set; }
        public decimal CostPerBed { get; set; }
        public decimal SecurityAmount { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime EffectiveTO { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
    }
}
