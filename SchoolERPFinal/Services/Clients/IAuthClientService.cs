using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This interface defines the rules for how the application talks to the security system to log users in.
    /// </summary>
    public interface IAuthClientService
    {
        /// <summary>
        /// Sends a login request (username and password) to the security system and waits for a response.
        /// </summary>
        Task<ApiResponse<UserSessionModel>> LoginAsync(LoginRequest request);
    }
}
