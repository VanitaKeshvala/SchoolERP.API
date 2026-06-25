using SchoolERP.Shared.Models;
using Common = SchoolERP.Shared.Models.Common;
using static System.Collections.Specialized.BitVector32;

namespace SchoolERP.Net.Services.Clients
{
    public class FieldClientService : BaseApiClient, IFieldClientService
    {
        public FieldClientService(HttpClient httpClient) : base(httpClient) { }

        /// <summary>
        /// Retrieves all custom fields from the API.
        /// </summary>
        public async Task<Common.ApiResponse<List<FieldModel>>> GetAllFieldsAsync(
            int companyId,
            int sessionId,
            bool? isSystemField = null,
            string? belongsTo = null)
        {           
            var url = $"api/Field/GetAllFields?companyId={companyId}&sessionId={sessionId}&isSystemField={isSystemField}&belongsTo={belongsTo}";
            return await GetAsync<List<FieldModel>>(url);
        }

        /// <summary>
        /// Retrieves all ID auto-generation settings from the API
        /// for the specified company and session.
        /// </summary>
        /// <param name="companyId">
        /// The unique identifier of the company.
        /// </param>
        /// <param name="sessionId">
        /// The unique identifier of the academic session.
        /// </param>
        /// <returns>
        /// Returns a list of ID auto-generation settings.
        /// </returns>
        public async Task<Common.ApiResponse<List<IDAutoGenSettings>>> GetIDAutoGenSettingsAsync(
            int companyId,
            int sessionId)
        {
            return await GetAsync<List<IDAutoGenSettings>>(
                $"api/Field/GetIDAutoGenSettings?companyId={companyId}&sessionId={sessionId}");
        }
    }
}
