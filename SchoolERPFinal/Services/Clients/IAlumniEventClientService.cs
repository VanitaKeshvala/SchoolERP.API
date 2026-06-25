using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface IAlumniEventClientService
    {
        /// <summary>
        /// Retrieves alumni events.
        /// </summary>
        /// <param name="searchText">Search text.</param>
        /// <returns>List of alumni events.</returns>
        Task<ApiResponse<List<AlumniEventViewModel>>> GetEventsAsync(
            string? searchText);

        Task<ApiResponse<dynamic>> UpsertEventAsync(
            AlumniEventUpsertRequest request);
        Task<ApiResponse<dynamic>> DeleteEventAsync(int eventId);
        Task<ApiResponse<dynamic>> ToggleEventStatusAsync(
           int eventId,
           bool isActive);
        Task<HttpResponseMessage> GetEventPhotoAsync(
            int eventId);
    }
}
