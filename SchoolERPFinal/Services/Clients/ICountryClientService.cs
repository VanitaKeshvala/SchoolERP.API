using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public interface ICountryClientService
    {
        Task<ApiResponse<dynamic>> UpsertCountryAsync(CountryRequestModel request);
        Task<ApiResponse<CountryModel>> GetByIDAsync(int id);
        Task<ApiResponse<List<CountryModel>>> GetAllAsync(int? companyId = null, int? sessionId = null);
        Task<ApiResponse<PagedResult<CountryModel>>> GetAllCountryWithPageAsync(HostelTypeSearchRequest request);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request);
    }
}
