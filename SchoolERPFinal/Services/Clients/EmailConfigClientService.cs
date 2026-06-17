using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This service handles the actual work of sending requests to the main system to manage email server settings in the database via an API.
    /// </summary>
    public class EmailConfigClientService : BaseApiClient, IEmailConfigClientService
    {
        public EmailConfigClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        /// <summary>
        /// Sends a request to the server to retrieve the current email configuration.
        /// </summary>
        public async Task<ApiResponse<MstEmailConfigViewModel>> GetEmailConfigAsync()
        {
            return await GetAsync<MstEmailConfigViewModel>("api/EmailConfigApi/Get");
        }

        /// <summary>
        /// Sends new or updated email configuration details to the server to be saved.
        /// </summary>
        public async Task<ApiResponse<dynamic>> UpsertEmailConfigAsync(MstEmailConfigUpsertRequest request)
        {
            return await PostAsync<dynamic>("api/EmailConfigApi/Upsert", request);
        }
    }
}
