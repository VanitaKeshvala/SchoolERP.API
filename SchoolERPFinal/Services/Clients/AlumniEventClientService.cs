using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class AlumniEventClientService :BaseApiClient, IAlumniEventClientService
    {
        public AlumniEventClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        /// <summary>
        /// Retrieves alumni events.
        /// </summary>
        /// <param name="searchText">Search text.</param>
        /// <returns>List of alumni events.</returns>
        public Task<ApiResponse<List<AlumniEventViewModel>>> GetEventsAsync(
            string? searchText)
        {
            return GetAsync<List<AlumniEventViewModel>>(
                $"api/AlumniEventApi/GetEvents?searchText={Uri.EscapeDataString(searchText ?? string.Empty)}");
        }

        /// <summary>
        /// Creates or updates an alumni event.
        /// </summary>
        /// <param name="request">Event details.</param>
        /// <returns>Operation result.</returns>
        public Task<ApiResponse<dynamic>> UpsertEventAsync(
            AlumniEventUpsertRequest request)
        {
            return PostAsync<dynamic>(
                "api/AlumniEventApi/UpsertEvent",
                request);
        }

        /// <summary>
        /// Deletes an alumni event.
        /// </summary>
        /// <param name="eventId">Event identifier.</param>
        /// <returns>Operation result.</returns>
        public Task<ApiResponse<dynamic>> DeleteEventAsync(int eventId)
        {
            return DeleteAsync<dynamic>(
                $"api/AlumniEventApi/DeleteEvent?eventId={eventId}");
        }

        /// <summary>
        /// Activates or deactivates an alumni event.
        /// </summary>
        /// <param name="eventId">Event identifier.</param>
        /// <param name="isActive">Active status.</param>
        /// <returns>Operation result.</returns>
        public Task<ApiResponse<dynamic>> ToggleEventStatusAsync(
            int eventId,
            bool isActive)
        {
            return PostAsync<dynamic>(
                $"api/AlumniEventApi/ToggleEventStatus?eventId={eventId}&isActive={isActive}",
                null);
        }

        /// <summary>
        /// Retrieves the photo associated with an alumni event.
        /// </summary>
        /// <param name="eventId">Event identifier.</param>
        /// <returns>Photo response.</returns>
        public async Task<HttpResponseMessage> GetEventPhotoAsync(
            int eventId)
        {
            return await _httpClient.GetAsync(
                $"api/AlumniEventApi/GetEventPhoto?eventId={eventId}");
        }
    }
}
