namespace SchoolERP.Shared.Models.Common
{
    public class SpResult
    {
        public int Result { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class SpRoomTypeResult
    {
        public int Result { get; set; }        
        public string Message { get; set; } = string.Empty;
        public int RoomTypeID { get; set; }
    }
}
