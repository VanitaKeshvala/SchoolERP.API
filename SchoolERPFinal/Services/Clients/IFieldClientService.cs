using SchoolERP.Shared.Models;
using Common = SchoolERP.Shared.Models.Common;
namespace SchoolERP.Net.Services.Clients
{
    public interface IFieldClientService
    {
        Task<Common.ApiResponse<List<FieldModel>>> GetAllFieldsAsync(
            int companyId,
            int sessionId,
            bool? isSystemField = null,
            string? belongsTo = null);

        Task<Common.ApiResponse<List<IDAutoGenSettings>>> GetIDAutoGenSettingsAsync(
            int companyId,
            int sessionId);
    }
}
