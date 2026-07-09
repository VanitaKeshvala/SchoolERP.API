using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public interface IPostalCodeClienService
    {
        Task<ApiResponse<dynamic>> UpsertPostalCodAsync(PostalCodeEditModel request);
        Task<ApiResponse<PostalCodeListViewModel>> GetByIDAsync(int id);
        Task<ApiResponse<List<PostalCodeListViewModel>>> GetAllAsync(int? companyId = null, int? sessionId = null);
        Task<ApiResponse<PagedResult<PostalCodeListViewModel>>> GetAllPostalCodeWithPageAsync(SerachPostalCode request);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request);
        Task<ApiResponse<List<PostalCodeListViewModel>>> SearchPostalCodeAsync(string term);
    }
}
