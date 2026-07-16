namespace SchoolERP.Shared.Models.Common
{
    public class SpResult
    {
        public int Result { get; set; }
        public string Message { get; set; } = string.Empty;
        public string TECHNICALMESSAGE { get; set; } = string.Empty;
    }

    public class SpRoomTypeResult
    {
        public int Result { get; set; }        
        public string Message { get; set; } = string.Empty;
        public int RoomTypeID { get; set; }
    }

    public class SpVisitorResult
    {
        public int? Result { get; set; }
        public string? Message { get; set; } = string.Empty;
        public int? VISITORBOOKID { get; set; }
        public string? TECHNICALMESSAGE { get; set; } = string.Empty;
    }
    public class SpPostalDispatch
    {
        public int? Result { get; set; }
        public string? Message { get; set; } = string.Empty;
        public int? POSTALDISPATCHID { get; set; }
        public string? TECHNICALMESSAGE { get; set; } = string.Empty;
    }
    public class SpPostalReceive
    {
        public int? Result { get; set; }
        public string? Message { get; set; } = string.Empty;
        public int? PostalReceiveID { get; set; }
        public string? TECHNICALMESSAGE { get; set; } = string.Empty;
    }

    public class SpLeaveApplicationResult
    {
        public int Result { get; set; }
        public int LeaveAppID { get; set; }
        public string Message { get; set; } = string.Empty;
        public string TECHNICALMESSAGE { get; set; } = string.Empty;
    }

    public class SearchRequest
    {
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public string? SearchKeyword { get; set; }
        public string? Mode { get; set; }
        public int? PageNumber { get; set; }
        public int PageSize { get; set; }
        public int? UserId { get; set; }
    }

    public class SpBooksResult
    {
        public int Result { get; set; }
        public int BookId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string TECHNICALMESSAGE { get; set; } = string.Empty;
    }

    public class DropdownModel
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
    }
}
