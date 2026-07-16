using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public interface ILibraryLanguageClientService
    {
        Task<ApiResponse<dynamic>> UpsertPublisherAsync(LibraryLanguageRequest request);
        Task<ApiResponse<LibraryLanguageModel>> GetByIDAsync(int id);
        Task<ApiResponse<PagedResult<LibraryLanguageModel>>> GetAllLibraryLanguageWithPageAsync(SearchRequest request);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request);
    }
}
