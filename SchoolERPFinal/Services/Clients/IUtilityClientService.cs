using System.Threading.Tasks;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This interface defines the rules for how the application communicates with the main system to perform general tasks, like switching languages or fetching dashboard stats.
    /// </summary>
    public interface IUtilityClientService
    {
        /// <summary>
        /// Tells the main system which language the user wants to see the application in.
        /// </summary>
        Task<ApiResponse<bool>> SetLanguageAsync(string language);

        /// <summary>
        /// Asks the main system for the summary information shown on the dashboard (like total counts).
        /// </summary>
        Task<ApiResponse<object>> GetDashboardSummaryAsync();
    }
}
