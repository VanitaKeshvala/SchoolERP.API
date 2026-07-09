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
}
