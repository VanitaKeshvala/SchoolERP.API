using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public class LibraryCategorySubjectClientService : BaseApiClient, ILibraryCategorySubjectClientService
    {
        public LibraryCategorySubjectClientService(HttpClient client) : base(client) { }

        public async Task<ApiResponse<SpResult>> UpsertLibraryCategorySubjectAsync(LibraryCategorySubjectRequest request)
        {
            return await PostAsync<SpResult>("api/LibraryCategorySubjectAPI/Save", request);
        }
        public async Task<ApiResponse<LibraryCategorySubjectModel>> GetByIDAsync(int id)
        {
            return await GetAsync<LibraryCategorySubjectModel>($"api/LibraryCategorySubjectAPI/GetById/{id}");
        }
        public async Task<ApiResponse<PagedResult<LibraryCategorySubjectModel>>> GetAllLibraryCategorySubjectWithPageAsync(LibraryCategorySubjectSearchRequest request)
        {
            return await PostAsync<PagedResult<LibraryCategorySubjectModel>>("api/LibraryCategorySubjectAPI/GetAllLibraryCategorySubjectWithPage", request);
        }
        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids)
        {
            return await PostAsync<dynamic>($"api/LibraryCategorySubjectAPI/Delete", ids);
        }

        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request)
        {
            return await PostAsync<dynamic>($"api/LibraryCategorySubjectAPI/ToggleStatus", request);
        }

        public async Task<ApiResponse<List<LibraryCategoryModel>>> GetAllLibraryCategoryAsync(int? companyId = null)
        {
            return await GetAsync<List<LibraryCategoryModel>>($"api/LibraryCategorySubjectAPI/GetAllLibraryCategory?companyId={companyId}");
        }

        public async Task<ApiResponse<List<LibrarySubjectModel>>> GetAllLibrarySubjectAsync(int? companyId = null)
        {
            return await GetAsync<List<LibrarySubjectModel>>($"api/LibraryCategorySubjectAPI/GetAllLibrarySubject?companyId={companyId}");
        }
    }
}
