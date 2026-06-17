using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This interface defines the rules for how the application communicates with the main system to manage user accounts.
    /// </summary>
    public interface IUserClientService
    {
        /// <summary>
        /// Asks the main system for a list of all users.
        /// </summary>
        Task<ApiResponse<List<UserViewModel>>> GetAllUsersAsync();

        /// <summary>
        /// Asks the main system for details about a specific user using their ID.
        /// </summary>
        Task<ApiResponse<UserViewModel>> GetUserByIdAsync(int id);

        /// <summary>
        /// Asks the main system for the list of role IDs assigned to a specific user.
        /// </summary>
        Task<ApiResponse<List<int>>> GetUserRoleIdsAsync(int id);

        /// <summary>
        /// Asks the main system for the list of roles available to be selected in a dropdown menu.
        /// </summary>
        Task<ApiResponse<List<RoleViewModel>>> GetRolesDropdownAsync();

        /// <summary>
        /// Asks the main system for the list of user categories (like 'Admin' or 'Staff') available to be selected in a dropdown menu.
        /// </summary>
        Task<ApiResponse<List<MstUserTypeViewModel>>> GetUserTypesDropdownAsync();

        /// <summary>
        /// Sends information to the main system to save or update a user's details.
        /// </summary>
        Task<ApiResponse<bool>> SaveUserAsync(UserUpsertRequest request);

        /// <summary>
        /// Tells the main system to turn a user's account on or off.
        /// </summary>
        Task<ApiResponse<bool>> ToggleStatusAsync(int userId, bool isActive);

        /// <summary>
        /// Tells the main system to clear any login blocks on a user's account.
        /// </summary>
        Task<ApiResponse<bool>> UnlockUserAsync(int id);

        /// <summary>
        /// Tells the main system to remove a user.
        /// </summary>
        Task<ApiResponse<bool>> DeleteUserAsync(int id);

        Task<ApiResponse<bool>> IsUsernameUniqueAsync(string username, int userId = 0);
        Task<ApiResponse<UserWizardViewModel>> GetUserWizardDataAsync(int userId = 0, string roleIds = "");
        Task<ApiResponse<bool>> SaveUserWizardAsync(UserUpsertRequest request);
    }
}
