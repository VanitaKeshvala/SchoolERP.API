using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.API.Interfaces
{
    public interface IAuthorService
    {
        Task<ApiResponse> UpsertAuthorAsync(AuthorRequest model);
        Task<AuthorModel?> GetAuthorByIdAsync(int authorId);
        Task<PagedResult<AuthorModel>> GetAllAuthorWithPage(SearchRequest req);
        (bool success, string message) DeleteAuthor(List<int> ids, int userId);
        (bool success, string message) ToggleAuthorStatus(StatusUpdateRequest request);
    }
}
