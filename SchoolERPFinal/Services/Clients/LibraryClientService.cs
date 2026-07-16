using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using static SchoolERP.Net.Services.Clients.LibraryClientService;

namespace SchoolERP.Net.Services.Clients
{
    public class LibraryClientService:BaseApiClient, ILibraryClientService
    {
        public LibraryClientService(HttpClient httpClient) : base(httpClient)
        {
        }

        /// <summary>
        /// Retrieves the list of books for the specified company.
        /// This method calls the Library API and returns all matching books.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="searchTerm">Optional search text.</param>
        /// <returns>A collection of books.</returns>
        public async Task<ApiResponse<List<BookViewModel>>> GetBookListAsync(
            int companyId,
            string? searchTerm)
        {
            return await GetAsync<List<BookViewModel>>(
                $"api/LibraryApi/GetBookList?companyId={companyId}&searchTerm={Uri.EscapeDataString(searchTerm ?? string.Empty)}");
        }

        /// <summary>
        /// Retrieves the details of a specific book.
        /// This method calls the Library API using the book identifier.
        /// </summary>
        /// <param name="id">The book identifier.</param>
        /// <returns>The requested book details.</returns>
        public async Task<ApiResponse<BookViewModel>> GetBookByIdAsync(int id)
        {
            return await GetAsync<BookViewModel>(
                $"api/LibraryApi/GetBookById?id={id}");
        }

        /// <summary>
        /// Sends a request to create or update a library book.
        /// </summary>
        /// <param name="request">Book information to save.</param>
        /// <returns>
        /// API response containing operation result.
        /// </returns>
        public async Task<ApiResponse<UpsertBookResponse>> UpsertBookAsync(BookUpsertRequest request)
        {
            return await PostAsync<UpsertBookResponse>("api/LibraryApi/UpsertBook",request);
        }

        /// <summary>
        /// Sends a request to delete a library book.
        /// </summary>
        /// <param name="id">Book identifier.</param>
        /// <returns>
        /// API response containing operation result.
        /// </returns>
        public async Task<ApiResponse<bool>> DeleteBookAsync(List<int> id)
        {
            return await PostAsync<bool>(
                $"api/LibraryApi/DeleteBook",id);
        }

        /// <summary>
        /// Sends a request to change the status of a library book.
        /// </summary>
        /// <param name="id">Book identifier.</param>
        /// <returns>
        /// API response containing operation result.
        /// </returns>
       
        public async Task<ApiResponse<bool>> ToggleBookStatusAsync(StatusUpdateRequest request)
        {
            return await PostAsync<bool>($"api/LibraryApi/ToggleBookStatus", request);
        }

        /// <summary>
        /// Retrieves library members based on the supplied filters.
        /// Used to populate the library member management screen.
        /// </summary>
        public async Task<ApiResponse<List<LibraryMemberViewModel>>> GetMemberListAsync(
            string memberType,
            int companyId,
            int? classId,
            int? sectionId,
            int? departmentId,
            string? search)
        {
            return await GetAsync<List<LibraryMemberViewModel>>(
                $"api/LibraryApi/GetMemberList" +
                $"?memberType={Uri.EscapeDataString(memberType)}" +
                $"&companyId={companyId}" +
                $"&classId={classId}" +
                $"&sectionId={sectionId}" +
                $"&departmentId={departmentId}" +
                $"&search={Uri.EscapeDataString(search ?? string.Empty)}");
        }

        /// <summary>
        /// Searches students or staff members for library membership registration.
        /// </summary>
        public async Task<ApiResponse<List<MembershipSearchViewModel>>> SearchForMembershipAsync(
            string memberType,
            int companyId,
            int? classId,
            int? sectionId,
            int? departmentId,
            string? search)
        {
            return await GetAsync<List<MembershipSearchViewModel>>(
                $"api/LibraryApi/SearchForMembership" +
                $"?memberType={Uri.EscapeDataString(memberType)}" +
                $"&companyId={companyId}" +
                $"&classId={classId}" +
                $"&sectionId={sectionId}" +
                $"&departmentId={departmentId}" +
                $"&search={Uri.EscapeDataString(search ?? string.Empty)}");
        }

        /// <summary>
        /// Registers a new library member.
        /// Used when assigning a library card to a student or staff member.
        /// </summary>
        public async Task<ApiResponse<object>> AddMemberAsync(
            LibraryMemberUpsertRequest request,
            int companyId,
            int userId)
        {
            return await PostAsync<object>(
                $"api/LibraryApi/AddMember?companyId={companyId}&userId={userId}",
                request);
        }

        /// <summary>
        /// Deletes a library member.
        /// </summary>
        /// <param name="id">
        /// The library member identifier.
        /// </param>
        /// <param name="userId">
        /// The user performing the delete operation.
        /// </param>
        /// <returns>
        /// Returns the result of the delete operation.
        /// </returns>
        public async Task<ApiResponse<bool>> DeleteMember(int id, int userId)
        {
            return await DeleteAsync<bool>(
                $"api/LibraryApi/DeleteMember?id={id}&userId={userId}");
        }

        /// <summary>
        /// Retrieves all books currently issued to a library member.
        /// </summary>
        /// <param name="memberId">
        /// The library member ID.
        /// </param>
        /// <param name="companyId">
        /// The company or school identifier.
        /// </param>
        /// <returns>
        /// Returns a collection of issued books.
        /// </returns>
        public async Task<ApiResponse<List<IssueReturnViewModel>>> GetIssuedBooks(
            int memberId,
            int companyId)
        {
            return await GetAsync<List<IssueReturnViewModel>>(
                $"api/LibraryApi/GetIssuedBooks?memberId={memberId}&companyId={companyId}");
        }

        

            /// <summary>
            /// Issues a book to a library member.
            /// This method is used when a member borrows a book from the library.
            /// </summary>
            /// <param name="request">Book issue details.</param>
            /// <returns>A response indicating whether the issue operation succeeded.</returns>
            public async Task<ApiResponse<bool>> IssueBook(IssueReturnUpsertRequest request)
            {
                return await PostAsync<bool>(
                    "api/LibraryApi/IssueBook",
                    request);
            }

            /// <summary>
            /// Records the return of a previously issued book.
            /// This method updates the transaction with the actual return date.
            /// </summary>
            /// <param name="request">Return book information.</param>
            /// <returns>A response indicating whether the return operation succeeded.</returns>
            public async Task<ApiResponse<bool>> ReturnBook(ReturnBookIssue req)
            {
                return await PostAsync<bool>($"api/LibraryApi/ReturnBook", req);
            }

            /// <summary>
            /// Retrieves complete details of a library member.
            /// This information is typically displayed before issuing a book.
            /// </summary>
            /// <param name="memberId">Library member identifier.</param>
            /// <param name="companyId">Company identifier.</param>
            /// <returns>Member details including card number, member type, and contact information.</returns>
            public async Task<ApiResponse<MemberDetailsViewModel>> GetMemberDetails(
                int memberId,
                int companyId)
            {
                return await GetAsync<MemberDetailsViewModel>(
                    $"api/LibraryApi/GetMemberDetails?memberId={memberId}&companyId={companyId}");
            }

        /// <summary>
        /// Deletes a library member.
        /// </summary>
        /// <param name="id">Library member identifier.</param>
        /// <param name="studentId">Student identifier.</param>
        /// <param name="staffId">Staff identifier.</param>
        /// <returns>Operation result.</returns>
        public Task<ApiResponse<dynamic>> DeleteMemberExAsync(
            int id,
            int? studentId,
            int? staffId, string? modeType)
        {
            return DeleteAsync<dynamic>(
                $"api/LibraryApi/DeleteMemberEx?id={id}&studentId={studentId}&staffId={staffId}&modeType={modeType}");
        }

        /// <summary>
        /// Retrieves a paginated list of classes based on the specified search criteria.
        /// Automatically resolves the current user's Company and Session if they are not provided.
        /// </summary>
        public async Task<ApiResponse<PagedResult<BookViewModel>>> GetAllLibraryWithPageAsync(LibrarySearchRequest request)
        {
            return await PostAsync<PagedResult<BookViewModel>>("api/LibraryApi/GetAllLibraryWithPage", request);
        }

        public async Task<ApiResponse<PagedResult<LibraryMemberViewModel>>> GetAllStudentsMembershipWithPageAsync(StudentsMembershipSearchRequest request)
        {
            return await PostAsync<PagedResult<LibraryMemberViewModel>>("api/LibraryApi/GetAllStudentsMembershipWithPage", request);
        }

        public async Task<ApiResponse<LibraryMember>> GetLibraryMemberByID(int id)
        {
            return await GetAsync<LibraryMember>($"api/LibraryApi/GetLibraryMemberByID/{id}");
        }

        public async Task<ApiResponse<PagedResult<StaffLibraryMember>>> GetAllStaffMembershipWithPageAsync(StaffLibraryMemberSearchModel request)
        {
            return await PostAsync<PagedResult<StaffLibraryMember>>($"api/LibraryApi/GetAllStaffMembershipWithPage", request);
        }

        public async Task<ApiResponse<PagedResult<IssueReturnViewModel>>> GetAllIssuedBooksWithPageIndexAsync(IssueBookSearchModel request)
        {
            return await PostAsync<PagedResult<IssueReturnViewModel>>($"api/LibraryApi/GetAllIssuedBooksWithPageIndex", request);
        }

        public async Task<ApiResponse<PagedResult<LibraryMemberViewModel>>> GetAllLibraryMemberWithPageIndexAsync(MemberSearchModel request)
        {
            return await PostAsync<PagedResult<LibraryMemberViewModel>>($"api/LibraryApi/GetAllLibraryMemberWithPageIndex", request);
        }

        #region All Libary Master Table Dropdwonbind
        public async Task<ApiResponse<List<DropdownModel>>> GetLibraryDocumentTypeDropdownListAsync(int companyId)
        {
            return await GetAsync<List<DropdownModel>>(
                $"api/LibraryApi/GetLibraryDocumentTypeDropdownList?companyId={companyId}");
        }
        public async Task<ApiResponse<List<DropdownModel>>> GetLibraryDocumentStatusDropdownListAsync(int companyId)
        {
            return await GetAsync<List<DropdownModel>>(
                $"api/LibraryApi/GetLibraryDocumentStatusDropdownList?companyId={companyId}");
        }
        public async Task<ApiResponse<List<DropdownModel>>> GetLibraryCategoryDropdownListAsync(int companyId)
        {
            return await GetAsync<List<DropdownModel>>(
                $"api/LibraryApi/GetLibraryCategoryDropdownList?companyId={companyId}");
        }
        public async Task<ApiResponse<List<DropdownModel>>> GetLibraryLanguageDropdownListAsync(int companyId)
        {
            return await GetAsync<List<DropdownModel>>(
                $"api/LibraryApi/GetLibraryLanguageDropdownList?companyId={companyId}");
        }
        public async Task<ApiResponse<List<DropdownModel>>> GetLibrarySupplierDropdownList(int companyId)
        {
            return await GetAsync<List<DropdownModel>>(
                $"api/LibraryApi/GetLibrarySupplierDropdownList?companyId={companyId}");
        }
        public async Task<ApiResponse<List<DropdownModel>>> GetLibrarySeriesDropdownListAsync(int companyId)
        {
            return await GetAsync<List<DropdownModel>>(
                $"api/LibraryApi/GetLibrarySeriesDropdownList?companyId={companyId}");
        }
        public async Task<ApiResponse<List<DropdownModel>>> GetLibraryBudgetDropdownListAsync(int companyId)
        {
            return await GetAsync<List<DropdownModel>>(
                $"api/LibraryApi/GetLibraryBudgetDropdownList?companyId={companyId}");
        }
        public async Task<ApiResponse<List<DropdownModel>>> GetLibraryPublisherDropdownListAsync(int companyId)
        {
            return await GetAsync<List<DropdownModel>>(
                $"api/LibraryApi/GetLibraryPublisherDropdownList?companyId={companyId}");
        }

        public async Task<ApiResponse<List<DropdownModel>>> GetLibrarySubjectDropdownListAsync(int companyId, int categoryId)
        {
            return await GetAsync<List<DropdownModel>>(
                $"api/LibraryApi/GetLibrarySubjectDropdownList?companyId={companyId}&categoryId={categoryId}");
        }
        public async Task<ApiResponse<List<BookViewModel>>> SearchBookTitleAsync(string term)
        {
            return await GetAsync<List<BookViewModel>>($"api/LibraryApi/SearchBookTitle?term={term}");
        }
        public async Task<ApiResponse<List<DropdownModel>>> GetLibraryAuthorDropdownListAsync(int companyId)
        {
            return await GetAsync<List<DropdownModel>>(
                $"api/LibraryApi/GetLibraryAuthorDropdownList?companyId={companyId}");
        }
        #endregion

        public Task<ApiResponse<dynamic>> UpsertBooksFrontPageAttachmentFileAsync(BooksAttachmentUpsertRequest req)
           => PostAsync<dynamic>("api/LibraryApi/UpsertBooksFrontPageAttachmentFile", req);


        public async Task<ApiResponse<PagedResult<AccessionDetailsModel>>> GetAllAccessionWithPageAsync(LibrarySearchRequest request)
        {
            return await PostAsync<PagedResult<AccessionDetailsModel>>("api/LibraryApi/GetAllAccessionWithPage", request);
        }
        public async Task<ApiResponse<AccessionDetailsModel>> GetAccessionById(int id, int companyId)
        {
            return await GetAsync<AccessionDetailsModel>($"api/LibraryApi/GetAccessionById?id={id}&companyId={companyId}");
        }
        public async Task<ApiResponse<AccessionDetailsModel>> GetAccessionByNo(string accessionNo, int companyid)
        {
            return await GetAsync<AccessionDetailsModel>($"api/LibraryApi/GetAccessionByNo?accessionNo={accessionNo}&companyid={companyid}");
        }

        public Task<ApiResponse<SpResult>> UpsertAccessionStatusAsync(AccessionStatusUpsertRequest req)
           => PostAsync<SpResult>("api/LibraryApi/UpsertAccessionStatus", req);

        public async Task<ApiResponse<List<AccessionDetailsModel>>> GetAccessionForLabelsAsync(AccessionSearchRequest request)
        {
            return await PostAsync<List<AccessionDetailsModel>>("api/LibraryApi/GetAccessionForLabels", request);
        }

        public async Task<ApiResponse<IssueNoResponse>> GetIssueNoAsync(int companyID)
        {
            return await GetAsync<IssueNoResponse>(
                $"api/LibraryApi/GetIssueNo?companyID={companyID}");
        }

        public async Task<ApiResponse<SpResult>> SaveIssueBook(LibraryIssueSaveRequest request)
        {
            return await PostAsync<SpResult>(
                $"api/LibraryApi/SaveIssueBook",request);
        }

        public async Task<ApiResponse<object>> ReturnIssueBook(LibraryReturnRequest request)
        {
            return await PostAsync<object>(
                $"api/LibraryApi/ReturnIssueBook", request);
        }

        public async Task<ApiResponse<BookDetailsResult>> GetBookDetailsAsync(int bookId)
        {
            return await GetAsync<BookDetailsResult>(
                $"api/LibraryApi/GetBookDetails?bookId={bookId}");
        }

        public async Task<ApiResponse<object>> AddStudentsMembershipBulkWithCards(
            BulkMembershipRequest request,
            int companyId,
            int userId)
        {
            return await PostAsync<object>(
                $"api/LibraryApi/AddStudentsMembershipBulkWithCards?companyId={companyId}&userId={userId}",
                request);
        }

        public async Task<ApiResponse<List<LibraryMemberSearchResult>>> SearchMemberAsync(int companyId, string memberType, string? searchText)
        {
            return await GetAsync<List<LibraryMemberSearchResult>>(
                $"api/LibraryApi/SearchMember?companyId={companyId}&memberType={memberType}&searchText={Uri.EscapeDataString(searchText ?? string.Empty)}");
        }

        public async Task<ApiResponse<List<AccessionDetailsModel>>> SearchAccessionNoAsync(int companyId, string? searchText)
        {
            return await GetAsync<List<AccessionDetailsModel>>(
                $"api/LibraryApi/SearchAccessionNo?companyId={companyId}&searchText={Uri.EscapeDataString(searchText ?? string.Empty)}");
        }
    }
}
