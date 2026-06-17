using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This service handles the actual work of sending requests to the main system to manage system settings and translations in the database via an API.
    /// </summary>
    public class SettingsClientService : BaseApiClient, ISettingsClientService
    {
        public SettingsClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        /// <summary>
        /// Sends a request to the server to fetch all translated words and phrases for the chosen language.
        /// </summary>
        public async Task<ApiResponse<Dictionary<string, string>>> GetTranslationsAsync(string language)
        {
            return await GetAsync<Dictionary<string, string>>($"api/SettingsApi/translations/{language}");
        }

        /// <summary>
        /// Sends a request to the server to change a translation for a specific word or phrase.
        /// </summary>
        public async Task<ApiResponse<bool>> UpdateTranslationAsync(TranslationUpdateModel model)
        {
            return await PostAsync<bool>("api/SettingsApi/translations/update", model);
        }

        public async Task<ApiResponse<List<FieldModel>>> GetAllFieldsAsync(bool? isSystemField = null, string belongsTo = null)
        {
            return await GetAsync<List<FieldModel>>($"api/SettingsApi/fields?isSystemField={isSystemField}&belongsTo={belongsTo}");
        }

        public async Task<ApiResponse<FieldModel>> GetFieldByIDAsync(int id)
        {
            return await GetAsync<FieldModel>($"api/SettingsApi/fields/{id}");
        }

        public async Task<ApiResponse<bool>> UpsertFieldAsync(FieldViewModel model)
        {
            return await PostAsync<bool>("api/SettingsApi/fields/upsert", model);
        }

        public async Task<ApiResponse<bool>> DeleteFieldAsync(List<int> id)
        {
            return await PostAsync<bool>($"api/SettingsApi/fields/delete",id);
        }

        public async Task<ApiResponse<bool>> ToggleFieldStatusAsync(int id, bool isActive)
        {
            return await PostAsync<bool>("api/SettingsApi/fields/toggle-status", new { id, isActive });
        }

        public async Task<ApiResponse<List<IDAutoGenSettings>>> GetIDAutoGenSettingsAsync()
        {
            return await GetAsync<List<IDAutoGenSettings>>("api/SettingsApi/id-autogen/settings");
        }

        public async Task<ApiResponse<bool>> SaveIDAutoGenSettingsAsync(IDAutoGenRequest req)
        {
            return await PostAsync<bool>("api/SettingsApi/id-autogen/save", req);
        }
    }
}
