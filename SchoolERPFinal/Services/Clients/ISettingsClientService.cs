using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This interface defines the rules for how the application communicates with the main system to manage language translations and general settings.
    /// </summary>
    public interface ISettingsClientService
    {
        /// <summary>
        /// Asks the main system for all text translations for a specific language.
        /// </summary>
        Task<ApiResponse<Dictionary<string, string>>> GetTranslationsAsync(string language);

        /// <summary>
        /// Sends a request to the main system to update a specific translated piece of text.
        /// </summary>
        Task<ApiResponse<bool>> UpdateTranslationAsync(TranslationUpdateModel model);

        Task<ApiResponse<List<FieldModel>>> GetAllFieldsAsync(bool? isSystemField = null, string belongsTo = null);
        Task<ApiResponse<FieldModel>> GetFieldByIDAsync(int id);
        Task<ApiResponse<bool>> UpsertFieldAsync(FieldViewModel model);
        Task<ApiResponse<bool>> DeleteFieldAsync(List<int> id);
        Task<ApiResponse<bool>> ToggleFieldStatusAsync(int id, bool isActive);
        Task<ApiResponse<List<IDAutoGenSettings>>> GetIDAutoGenSettingsAsync();
        Task<ApiResponse<bool>> SaveIDAutoGenSettingsAsync(IDAutoGenRequest req);
    }
}
