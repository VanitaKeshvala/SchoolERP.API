using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This service handles the actual work of communicating with the security system to verify a user's login details and start their session.
    /// </summary>
    public class AuthClientService : BaseApiClient, IAuthClientService
    {
        public AuthClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        /// <summary>
        /// Performs the login action by sending the user's credentials to the security server and retrieving their session information if successful.
        /// </summary>
        public async Task<ApiResponse<UserSessionModel>> LoginAsync(LoginRequest request)
        {
            return await PostAsync<UserSessionModel>("api/auth/login", request);
        }
    }
}
