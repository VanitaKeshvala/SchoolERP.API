using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This interface defines the rules for how the application communicates with the main system to manage the different categories of users (like 'Student', 'Teacher', or 'Admin').
    /// </summary>
    public interface IUserTypeClientService
    {
        /// <summary>
        /// Asks the main system for a list of all user categories.
        /// </summary>
        Task<ApiResponse<List<MstUserTypeViewModel>>> GetAllAsync();

        /// <summary>
        /// Asks the main system for details about a specific user category using its ID.
        /// </summary>
        Task<ApiResponse<MstUserTypeViewModel>> GetByIdAsync(int id);

        /// <summary>
        /// Sends information to the main system to save or update a user category.
        /// </summary>
        Task<ApiResponse<bool>> SaveAsync(MstUserTypeUpsertRequest request);

        /// <summary>
        /// Tells the main system to turn a user category on or off.
        /// </summary>
        Task<ApiResponse<bool>> ToggleStatusAsync(int typeId, bool isActive);
    }
}
