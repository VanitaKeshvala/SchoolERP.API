using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;
using System.Threading.Tasks;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This interface defines the rules for how the application communicates with the main system to manage email server settings.
    /// </summary>
    public interface IEmailConfigClientService
    {
        /// <summary>
        /// Asks the main system for the current email server settings.
        /// </summary>
        Task<ApiResponse<MstEmailConfigViewModel>> GetEmailConfigAsync();

        /// <summary>
        /// Sends information to the main system to update or save new email server settings.
        /// </summary>
        Task<ApiResponse<dynamic>> UpsertEmailConfigAsync(MstEmailConfigUpsertRequest request);
    }
}
