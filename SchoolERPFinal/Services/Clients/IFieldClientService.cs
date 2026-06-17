using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface IFieldClientService
    {
        Task<ApiResponse<List<FieldModel>>> GetAllFieldsAsync(
            int companyId,
            int sessionId,
            bool? isSystemField = null,
            string? belongsTo = null);

        Task<ApiResponse<List<IDAutoGenSettings>>> GetIDAutoGenSettingsAsync(
            int companyId,
            int sessionId);
    }
}
