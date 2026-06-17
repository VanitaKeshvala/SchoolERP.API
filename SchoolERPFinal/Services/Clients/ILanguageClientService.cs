using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This interface defines the rules for how the application communicates with the main system to manage system language settings.
    /// </summary>
    public interface ILanguageClientService
    {
        /// <summary>
        /// Asks the main system for a list of all available languages.
        /// </summary>
        Task<ApiResponse<List<MstLanguageViewModel>>> GetAllAsync(bool includeDeleted = false);

        /// <summary>
        /// Asks the main system for details about a specific language using its ID.
        /// </summary>
        Task<ApiResponse<MstLanguageViewModel>> GetByIDAsync(int id);

        /// <summary>
        /// Sends information to the main system to either add a new language or update an existing one.
        /// </summary>
        Task<ApiResponse<dynamic>> UpsertAsync(MstLanguageUpsertRequest request);

        /// <summary>
        /// Tells the main system to remove a specific language.
        /// </summary>
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> id);

        /// <summary>
        /// Tells the main system to turn a language's active status on or off.
        /// </summary>
        Task<ApiResponse<dynamic>> ToggleStatusAsync(int id, bool isActive);
    }
}
