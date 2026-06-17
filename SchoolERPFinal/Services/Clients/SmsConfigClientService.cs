using System;
using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This service handles the actual work of sending requests to the main system to manage SMS gateway settings in the database via an API.
    /// </summary>
    public class SmsConfigClientService : BaseApiClient, ISmsConfigClientService
    {
        public SmsConfigClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        /// <summary>
        /// Sends a request to the server to retrieve the current SMS configuration.
        /// </summary>
        public async Task<ApiResponse<MstSmsConfigViewModel>> GetSmsConfigAsync()
        {
            return await GetAsync<MstSmsConfigViewModel>("api/SmsConfigApi/Get");
        }

        /// <summary>
        /// Sends new or updated SMS configuration details to the server to be saved.
        /// </summary>
        public async Task<ApiResponse<dynamic>> UpsertSmsConfigAsync(MstSmsConfigUpsertRequest request)
        {
            return await PostAsync<dynamic>("api/SmsConfigApi/Upsert", request);
        }
    }
}
