using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This interface defines the rules for how the application communicates with the main system to manage academic sessions (like the 2023-2024 school year).
    /// </summary>
    public interface ISessionClientService
    {
        /// <summary>
        /// Asks the main system for a list of all academic sessions.
        /// </summary>
        Task<ApiResponse<List<MstSessionViewModel>>> GetAllAsync(bool includeDeleted = false);

        /// <summary>
        /// Asks the main system for details about a specific session using its ID.
        /// </summary>
        Task<ApiResponse<MstSessionViewModel>> GetByIDAsync(int id);

        /// <summary>
        /// Sends information to the main system to either add a new session or update an existing one.
        /// </summary>
        Task<ApiResponse<dynamic>> UpsertAsync(MstSessionUpsertRequest request);

        /// <summary>
        /// Tells the main system to remove a specific session.
        /// </summary>
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> id);

        /// <summary>
        /// Tells the main system to turn a session's active status on or off.
        /// </summary>
        Task<ApiResponse<dynamic>> ToggleStatusAsync(int id, bool isActive);

        /// <summary>
        /// Tells the main system which academic session the current user wants to work in right now.
        /// </summary>
        Task<ApiResponse<dynamic>> SetCurrentSessionAsync(SetCurrentSessionRequest request);

        /// <summary>
        /// Asks the main system which academic session is currently selected for the logged-in user.
        /// </summary>
        Task<ApiResponse<int?>> GetUserCurrentSessionAsync();
    }
}
