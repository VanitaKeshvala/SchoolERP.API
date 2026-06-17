using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This service handles the actual work of sending requests to the main system for general application tasks in the database via an API.
    /// </summary>
    public class UtilityClientService : BaseApiClient, IUtilityClientService
    {
        public UtilityClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        /// <summary>
        /// Sends a request to the server to save the user's preferred language choice.
        /// </summary>
        public async Task<ApiResponse<bool>> SetLanguageAsync(string language)
        {
            return await PostAsync<bool>($"api/UtilityApi/set-language?language={language}", null);
        }

        /// <summary>
        /// Sends a request to the server to retrieve the summary data for the dashboard display.
        /// </summary>
        public async Task<ApiResponse<object>> GetDashboardSummaryAsync()
        {
            return await GetAsync<object>("api/UtilityApi/dashboard-summary");
        }
    }
}
