using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

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
        Task<ApiResponse<UpsertBookResponse>> UpsertBookAsync(BookUpsertRequest request);

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
        Task<ApiResponse<bool>> ToggleBookStatusAsync(StatusUpdateRequest request);

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
            Task<ApiResponse<bool>> ReturnBook(ReturnBookIssue req);

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
            int? staffId, string? modeType);

        /// <summary>
        /// Retrieves a paginated list of classes based on the specified search criteria.
        /// Automatically resolves the current user's Company and Session if they are not provided.
        /// </summary>
        Task<ApiResponse<PagedResult<BookViewModel>>> GetAllLibraryWithPageAsync(LibrarySearchRequest request);
        Task<ApiResponse<PagedResult<LibraryMemberViewModel>>> GetAllStudentsMembershipWithPageAsync(StudentsMembershipSearchRequest request);
        Task<ApiResponse<LibraryMember>> GetLibraryMemberByID(int id);
        Task<ApiResponse<PagedResult<StaffLibraryMember>>> GetAllStaffMembershipWithPageAsync(StaffLibraryMemberSearchModel request);
        Task<ApiResponse<PagedResult<IssueReturnViewModel>>> GetAllIssuedBooksWithPageIndexAsync(IssueBookSearchModel request);
        Task<ApiResponse<PagedResult<LibraryMemberViewModel>>> GetAllLibraryMemberWithPageIndexAsync(MemberSearchModel request);

        #region All Libary Master Table Dropdwonbind
        Task<ApiResponse<List<DropdownModel>>> GetLibraryDocumentTypeDropdownListAsync(int companyId);
        Task<ApiResponse<List<DropdownModel>>> GetLibraryDocumentStatusDropdownListAsync(int companyId);
        Task<ApiResponse<List<DropdownModel>>> GetLibraryCategoryDropdownListAsync(int companyId);
        Task<ApiResponse<List<DropdownModel>>> GetLibraryLanguageDropdownListAsync(int companyId);
        Task<ApiResponse<List<DropdownModel>>> GetLibrarySupplierDropdownList(int companyId);
        Task<ApiResponse<List<DropdownModel>>> GetLibrarySeriesDropdownListAsync(int companyId);
        Task<ApiResponse<List<DropdownModel>>> GetLibraryBudgetDropdownListAsync(int companyId);
        Task<ApiResponse<List<DropdownModel>>> GetLibraryPublisherDropdownListAsync(int companyId);
        Task<ApiResponse<List<DropdownModel>>> GetLibrarySubjectDropdownListAsync(int companyId, int categoryId);
        Task<ApiResponse<List<DropdownModel>>> GetLibraryAuthorDropdownListAsync(int companyId);
        Task<ApiResponse<List<BookViewModel>>> SearchBookTitleAsync(string term);
        #endregion

        Task<ApiResponse<dynamic>> UpsertBooksFrontPageAttachmentFileAsync(BooksAttachmentUpsertRequest req);
        Task<ApiResponse<PagedResult<AccessionDetailsModel>>> GetAllAccessionWithPageAsync(LibrarySearchRequest request);
        Task<ApiResponse<AccessionDetailsModel>> GetAccessionById(int id, int companyId);
        Task<ApiResponse<AccessionDetailsModel>> GetAccessionByNo(string accessionNo, int companyid);
        Task<ApiResponse<SpResult>> UpsertAccessionStatusAsync(AccessionStatusUpsertRequest req);
        Task<ApiResponse<List<AccessionDetailsModel>>> GetAccessionForLabelsAsync(AccessionSearchRequest request);
        Task<ApiResponse<IssueNoResponse>> GetIssueNoAsync(int companyID);
        Task<ApiResponse<SpResult>> SaveIssueBook(LibraryIssueSaveRequest request);
        Task<ApiResponse<object>> ReturnIssueBook(LibraryReturnRequest request);
        Task<ApiResponse<BookDetailsResult>> GetBookDetailsAsync(int bookId);
        Task<ApiResponse<object>> AddStudentsMembershipBulkWithCards(
            BulkMembershipRequest request,
            int companyId,
            int userId);

        Task<ApiResponse<List<LibraryMemberSearchResult>>> SearchMemberAsync(int companyId, string memberType, string? searchText);
        Task<ApiResponse<List<AccessionDetailsModel>>> SearchAccessionNoAsync(int companyId, string? searchText);
    }
}
