using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public interface IAuthorClientService
    {
        Task<ApiResponse<dynamic>> UpsertAuthorAsync(AuthorRequest request);
        Task<ApiResponse<AuthorModel>> GetByIDAsync(int id);
        Task<ApiResponse<PagedResult<AuthorModel>>> GetAllAuthorWithPageAsync(SearchRequest request);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request);
    }
}
