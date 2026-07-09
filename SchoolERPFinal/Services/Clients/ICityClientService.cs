using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public interface ICityClientService
    {
        Task<ApiResponse<CityApiResponse>> UpsertCityAsync(CityUpsertRequest request);
        Task<ApiResponse<CityModel>> GetByIDAsync(int id);
        Task<ApiResponse<CityListResponse>> GetAllAsync(int? companyId = null, int? sessionId = null);
        Task<ApiResponse<PagedResult<CityModel>>> GetAllCityWithPageAsync(CitySearchRequest request);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request);
        Task<ApiResponse<List<CityModel>>> GetAllCityByStateIdWisesync(int id);
    }
}
