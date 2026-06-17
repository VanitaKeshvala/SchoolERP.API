using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This interface defines the rules for how the application communicates with the main system to manage currency and exchange rate information.
    /// </summary>
    public interface ICurrencyClientService
    {
        /// <summary>
        /// Asks the main system for a list of all currencies.
        /// </summary>
        Task<ApiResponse<List<MstCurrencyViewModel>>> GetAllAsync(bool includeDeleted = false);

        /// <summary>
        /// Asks the main system for details about a specific currency using its ID.
        /// </summary>
        Task<ApiResponse<MstCurrencyViewModel>> GetByIDAsync(int id);

        /// <summary>
        /// Sends information to the main system to either add a new currency or update an existing one.
        /// </summary>
        Task<ApiResponse<dynamic>> UpsertAsync(MstCurrencyUpsertRequest request);

        /// <summary>
        /// Tells the main system to remove a specific currency.
        /// </summary>
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> id);

        /// <summary>
        /// Tells the main system to turn a currency's active status on or off.
        /// </summary>
        Task<ApiResponse<dynamic>> ToggleStatusAsync(int id, bool isActive);
    }
}
