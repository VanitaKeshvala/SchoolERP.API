using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface ILibraryClientService
    {
        /// <summary>
        /// Retrieves the list of books for the specified company.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="searchTerm">Optional search text.</param>
        /// <returns>A collection of books.</returns>
        Task<ApiResponse<List<BookViewModel>>> GetBookListAsync(
            int companyId,
            string? searchTerm);

        /// <summary>
        /// Retrieves a specific book by its identifier.
        /// </summary>
        /// <param name="id">The book identifier.</param>
        /// <returns>The requested book.</returns>
        Task<ApiResponse<BookViewModel>> GetBookByIdAsync(int id);

        /// <summary>
        /// Creates or updates a library book.
        /// </summary>
        /// <param name="request">Book details.</param>
        /// <returns>
        /// API response indicating whether the operation succeeded.
        /// </returns>
        Task<ApiResponse<bool>> UpsertBookAsync(BookUpsertRequest request);

        /// <summary>
        /// Deletes a library book.
        /// </summary>
        /// <param name="id">Book identifier.</param>
        /// <returns>
        /// API response indicating whether the operation succeeded.
        /// </returns>
        Task<ApiResponse<bool>> DeleteBookAsync(List<int> id);

        /// <summary>
        /// Changes the active/inactive status of a library book.
        /// </summary>
        /// <param name="id">Book identifier.</param>
        /// <returns>
        /// API response indicating whether the operation succeeded.
        /// </returns>
        Task<ApiResponse<bool>> ToggleBookStatusAsync(int id);

        Task<ApiResponse<List<LibraryMemberViewModel>>> GetMemberListAsync(
    string memberType,
    int companyId,
    int? classId,
    int? sectionId,
    int? departmentId,
    string? search);

        Task<ApiResponse<List<MembershipSearchViewModel>>> SearchForMembershipAsync(
            string memberType,
            int companyId,
            int? classId,
            int? sectionId,
            int? departmentId,
            string? search);

        Task<ApiResponse<object>> AddMemberAsync(
            LibraryMemberUpsertRequest request,
            int companyId,
            int userId);

        /// <summary>
        /// Deletes a library member.
        /// </summary>
        Task<ApiResponse<bool>> DeleteMember(
            int id,
            int userId);

       

        /// <summary>
        /// Retrieves all books currently issued to a library member.
        /// </summary>
        Task<ApiResponse<List<IssueReturnViewModel>>> GetIssuedBooks(
            int memberId,
            int companyId);

        /// <summary>
        /// Provides methods for managing library book issue and return operations through API calls.
        /// </summary>
        
            /// <summary>
            /// Issues a book to a library member.
            /// </summary>
            Task<ApiResponse<bool>> IssueBook(IssueReturnUpsertRequest request);

            /// <summary>
            /// Records the return of an issued book.
            /// </summary>
            Task<ApiResponse<bool>> ReturnBook(int issueId, DateTime returnDate);

            /// <summary>
            /// Retrieves detailed information about a library member.
            /// </summary>
            Task<ApiResponse<MemberDetailsViewModel>> GetMemberDetails(int memberId, int companyId);

        /// <summary>
        /// Deletes a library member.
        /// </summary>
        /// <param name="id">Library member identifier.</param>
        /// <param name="studentId">Student identifier.</param>
        /// <param name="staffId">Staff identifier.</param>
        /// <returns>Operation result.</returns>
        Task<ApiResponse<dynamic>> DeleteMemberExAsync(
            int id,
            int? studentId,
            int? staffId);
    }
}
