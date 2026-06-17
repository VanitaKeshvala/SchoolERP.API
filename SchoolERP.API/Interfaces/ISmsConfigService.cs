using SchoolERP.API.Models;

namespace SchoolERP.API.Interfaces
{
    /// <summary>
    /// This interface defines the rules for managing SMS settings, such as the gateway used to send text messages.
    /// </summary>
    public interface ISmsConfigService
    {

        /// <summary>
        /// Gets the current SMS settings from the system.
        /// </summary>
        MstSmsConfigViewModel? GetSmsConfig(int companyId);

        /// <summary>
        /// Updates or saves new SMS settings, like the API URL and security key.
        /// </summary>
        (bool success, string message) UpsertSmsConfig(MstSmsConfigUpsertRequest request, int userId, int companyId);
    }
}
