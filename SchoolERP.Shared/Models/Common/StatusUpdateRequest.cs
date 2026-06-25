namespace SchoolERP.Shared.Models.Common
{
    /// <summary>
    /// Generic base request for any toggle-status operation.
    /// Inherit this and set the Ids property with the correct primary key values.
    /// </summary>
    public class StatusUpdateRequest
    {
        /// <summary>
        /// Comma-separated primary key IDs — e.g. "1,5,7"
        /// Works for any table: UserId, SectionId, ClassId, etc.
        /// </summary>
        public string Ids { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public int DoneBy { get; set; }

        public string? IpAddress { get; set; }
    }
}
