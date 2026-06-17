using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;
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
        public async Task<ApiResponse<bool>> UpsertBookAsync(BookUpsertRequest request)
        {
            return await PostAsync<bool>(
                "api/LibraryApi/UpsertBook",
                request);
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
       
        public async Task<ApiResponse<bool>> ToggleBookStatusAsync(int id)
        {
            return await PostAsync<bool>(
                $"api/LibraryApi/ToggleBookStatus?id={id}",
                null);
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
            public async Task<ApiResponse<bool>> ReturnBook(
            int issueId,
            DateTime returnDate)
            {
                return await PostAsync<bool>(
                    "api/LibraryApi/ReturnBook",
                    new
                    {
                        issueId,
                        returnDate
                    });
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
            int? staffId)
        {
            return DeleteAsync<dynamic>(
                $"api/LibraryApi/DeleteMemberEx?id={id}&studentId={studentId}&staffId={staffId}");
        }

    }
}
