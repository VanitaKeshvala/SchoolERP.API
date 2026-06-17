using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This interface defines the rules for how the application communicates with the main system to manage organization or campus details.
    /// </summary>
    public interface IOrganisationClientService
    {
        /// <summary>
        /// Asks the main system for a list of all organizations.
        /// </summary>
        Task<ApiResponse<List<OrganisationViewModel>>> GetAllOrganisationsAsync(bool includeDeleted = false);

        /// <summary>
        /// Asks the main system for details about a specific organization using its ID.
        /// </summary>
        Task<ApiResponse<OrganisationViewModel>> GetOrganisationByIDAsync(int id);

        /// <summary>
        /// Sends information to the main system to either add a new organization or update an existing one.
        /// </summary>
        Task<ApiResponse<dynamic>> UpsertOrganisationAsync(OrganisationUpsertRequest request);

        /// <summary>
        /// Tells the main system to remove a specific organization.
        /// </summary>
        Task<ApiResponse<dynamic>> DeleteOrganisationAsync(int id);

        /// <summary>
        /// Tells the main system to turn an organization's active status on or off.
        /// </summary>
        Task<ApiResponse<dynamic>> ToggleStatusAsync(int id, bool isActive);
    }
}
