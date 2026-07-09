using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.API.Interfaces
{
    public interface ILibraryService
    {
        List<BookViewModel> GetBookList(int companyId, string? searchTerm);
        BookViewModel GetBookById(int id);
        (bool Success, string Message) UpsertBook(BookUpsertRequest req, int companyId, int userId);
        (bool Success, string Message) DeleteBook(List<int> id, int userId);
        (bool Success, string Message) ToggleBookStatus(StatusUpdateRequest request);
        /// <summary>
        /// Retrieves a paginated list of classes based on the specified search criteria.
        /// Automatically resolves the current user's Company and Session if they are not provided.
        /// </summary>
        Task<PagedResult<BookViewModel>> GetAllLibraryWithPage(LibrarySearchRequest req);

        // Membership
        List<LibraryMemberViewModel> GetMemberList(string memberType, int companyId, int? classId, int? sectionId, int? departmentId, string? search);
        List<MembershipSearchViewModel> SearchForMembership(string memberType, int companyId, int? classId, int? sectionId, int? departmentId, string? search);
        (bool Success, string Message) AddMember(LibraryMemberUpsertRequest req, int companyId, int userId);
        (bool Success, string Message) DeleteMember(int id, int userId);

        // Issue/Return
        List<IssueReturnViewModel> GetIssuedBooks(int memberId, int companyId);
        (bool Success, string Message) IssueBook(IssueReturnUpsertRequest req, int companyId, int userId);
        (bool Success, string Message) ReturnBook(int issueId, DateTime returnDate, int companyId, int userId);
        MemberDetailsViewModel GetMemberDetails(int memberId, int companyId);
        (bool Success, string Message) DeleteMemberEx(int id, int? studentId, int? staffId, int? companyId, int userId, string? modeType);

        Task<PagedResult<LibraryMemberViewModel>> GetAllStudentsMembershipWithPage(StudentsMembershipSearchRequest req);
        LibraryMember? GetLibraryMemberByID(int libraryMemberID);

        Task<PagedResult<StaffLibraryMember>> GetAllStaffMembershipWithPage(StaffLibraryMemberSearchModel req);
        Task<PagedResult<IssueReturnViewModel>> GetAllIssuedBooksWithPageIndex(IssueBookSearchModel req);
        Task<PagedResult<LibraryMemberViewModel>> GetAllLibraryMemberWithPageIndex(MemberSearchModel req);
    }
}
