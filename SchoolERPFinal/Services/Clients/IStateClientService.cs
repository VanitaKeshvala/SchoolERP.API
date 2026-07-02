using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public interface IStateClientService
    {
        Task<ApiResponse<dynamic>> UpsertStateAsync(StateRequestModel request);
        Task<ApiResponse<StateModel>> GetByIDAsync(int id);
        Task<ApiResponse<List<StateModel>>> GetAllAsync(int? companyId = null, int? sessionId = null);
        Task<ApiResponse<PagedResult<StateModel>>> GetAllStateWithPageAsync(HostelTypeSearchRequest request);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request);
        Task<ApiResponse<List<StateModel>>> GetAllStateByCountyAsync(int? companyId = null, int? sessionId = null, int? countryId = null);
    }
}
