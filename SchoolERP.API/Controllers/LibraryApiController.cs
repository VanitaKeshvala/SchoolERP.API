using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Interfaces;
using SchoolERP.API.Models;
using SchoolERP.API.Models.Common;
using System.Security.Claims;

namespace SchoolERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LibraryApiController : Controller
    {
        private readonly ILibraryService _libraryService;
        private readonly ICompanyService _companyService;
        private readonly ISessionService _sessionService;
        public LibraryApiController(ILibraryService libraryService,
            ICompanyService companyService,
            ISessionService sessionService)
        {
            _libraryService = libraryService;
            _companyService = companyService;
            _sessionService = sessionService;
        }
        private int GetUserId() => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("UserId")?.Value, out var id) ? id : 0;
        private int GetCompanyId() => _companyService.GetUserCurrentCompany(GetUserId()) ?? 0;
        private int GetSessionId() => _sessionService.GetUserCurrentSession(GetUserId()) ?? 0;
        /// <summary>
        /// Retrieves the list of books for the specified company.
        /// Optionally filters the results using a search term.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="searchTerm">Optional search text to filter books.</param>
        /// <returns>A list of books matching the criteria.</returns>
        [HttpGet("GetBookList")]
        public IActionResult GetBookList(int companyId, string? searchTerm)
        {
            var result = _libraryService.GetBookList(companyId, searchTerm);

            return Ok(new ApiResponse<List<BookViewModel>>
            {
                Success = true,
                Data = result
            });
        }

        /// <summary>
        /// Retrieves the details of a specific book by its unique identifier.
        /// </summary>
        /// <param name="id">The book identifier.</param>
        /// <returns>The book details if found; otherwise null.</returns>
        [HttpGet("GetBookById")]
        public IActionResult GetBookById(int id)
        {
            var result = _libraryService.GetBookById(id);

            return Ok(new ApiResponse<BookViewModel>
            {
                Success = true,
                Data = result
            });
        }

        /// <summary>
        /// Creates or updates a library book record.
        /// </summary>
        /// <param name="req">Book information to save.</param>
        /// <returns>
        /// Returns success status and operation message.
        /// </returns>
        [HttpPost("UpsertBook")]
        public IActionResult UpsertBook([FromBody] BookUpsertRequest req)
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();

            var result = _libraryService.UpsertBook(req, companyId, userId);

            return Ok(new ApiResponse<bool>
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Success
            });
        }

        /// <summary>
        /// Deletes a library book.
        /// </summary>
        /// <param name="id">Book identifier.</param>
        /// <returns>
        /// Returns success status and operation message.
        /// </returns>
        [HttpPost("DeleteBook")]
        public IActionResult DeleteBook(List<int> id)
        {
            var userId = GetUserId();

            var result = _libraryService.DeleteBook(id, userId);

            return Ok(new ApiResponse<bool>
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Success
            });
        }

        /// <summary>
        /// Changes the active/inactive status of a library book.
        /// </summary>
        /// <param name="id">Book identifier.</param>
        /// <returns>
        /// Returns success status and operation message.
        /// </returns>
        [HttpPost("ToggleBookStatus")]
        public IActionResult ToggleBookStatus(int id)
        {
            var userId = GetUserId();

            var result = _libraryService.ToggleBookStatus(id, userId);

            return Ok(new ApiResponse<bool>
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Success
            });
        }

        /// <summary>
        /// Retrieves library members based on member type and optional search filters.
        /// This method is used to display registered library members such as students,
        /// staff, or all members.
        /// </summary>
        /// <param name="memberType">
        /// Type of member to retrieve (Student, Staff, or All).
        /// </param>
        /// <param name="companyId">
        /// Company identifier used for data isolation.
        /// </param>
        /// <param name="classId">
        /// Optional class filter for student members.
        /// </param>
        /// <param name="sectionId">
        /// Optional section filter for student members.
        /// </param>
        /// <param name="departmentId">
        /// Optional department filter for staff members.
        /// </param>
        /// <param name="search">
        /// Optional search text for member name, admission number, or card number.
        /// </param>
        /// <returns>
        /// A collection of library members matching the specified criteria.
        /// </returns>
        [HttpGet("GetMemberList")]
        public IActionResult GetMemberList(
            string memberType,
            int companyId,
            int? classId,
            int? sectionId,
            int? departmentId,
            string? search)
        {
            var result = _libraryService.GetMemberList(
                memberType,
                companyId,
                classId,
                sectionId,
                departmentId,
                search);

            return Ok(ApiResponse<List<LibraryMemberViewModel>>.SuccessResponse(result));
        }

        /// <summary>
        /// Searches students or staff members who are eligible for library membership.
        /// This method is typically used before registering a new library member.
        /// </summary>
        /// <param name="memberType">
        /// Member type to search for (Student or Staff).
        /// </param>
        /// <param name="companyId">
        /// Company identifier.
        /// </param>
        /// <param name="classId">
        /// Optional class filter.
        /// </param>
        /// <param name="sectionId">
        /// Optional section filter.
        /// </param>
        /// <param name="departmentId">
        /// Optional department filter.
        /// </param>
        /// <param name="search">
        /// Optional search keyword.
        /// </param>
        /// <returns>
        /// A collection of matching students or staff members.
        /// </returns>
        [HttpGet("SearchForMembership")]
        public IActionResult SearchForMembership(
            string memberType,
            int companyId,
            int? classId,
            int? sectionId,
            int? departmentId,
            string? search)
        {
            var result = _libraryService.SearchForMembership(
                memberType,
                companyId,
                classId,
                sectionId,
                departmentId,
                search);

            return Ok(ApiResponse<List<MembershipSearchViewModel>>.SuccessResponse(result));
        }

        /// <summary>
        /// Registers a new library member and generates a library membership record.
        /// This method supports both student and staff memberships.
        /// </summary>
        /// <param name="req">
        /// Membership information to be saved.
        /// </param>
        /// <param name="companyId">
        /// Company identifier.
        /// </param>
        /// <param name="userId">
        /// User performing the operation.
        /// </param>
        /// <returns>
        /// Success status and operation message.
        /// </returns>
        [HttpPost("AddMember")]
        public IActionResult AddMember(
            [FromBody] LibraryMemberUpsertRequest req,
            int companyId,
            int userId)
        {
            var result = _libraryService.AddMember(
                req,
                companyId,
                userId);

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                result.Success,
                result.Message
            }));
        }

        /// <summary>
        /// Deletes a library member using the specified member ID.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the library member.
        /// </param>
        /// <param name="userId">
        /// The ID of the user performing the delete operation.
        /// </param>
        /// <returns>
        /// Returns the operation result along with a success or failure message.
        /// </returns>
        [HttpDelete("DeleteMember")]
        public IActionResult DeleteMember(int id, int userId)
        {
            var result = _libraryService.DeleteMember(id, userId);

            return Ok(new ApiResponse<bool>
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Success
            });
        }

        /// <summary>
        /// Retrieves all books currently issued to a specific library member.
        /// </summary>
        /// <param name="memberId">
        /// The library member ID.
        /// </param>
        /// <param name="companyId">
        /// The company or school identifier.
        /// </param>
        /// <returns>
        /// Returns the list of issued books.
        /// </returns>
        [HttpGet("GetIssuedBooks")]
        public IActionResult GetIssuedBooks(
            int memberId,
            int companyId)
        {
            var data = _libraryService.GetIssuedBooks(
                memberId,
                companyId);

            return Ok(new ApiResponse<List<IssueReturnViewModel>>
            {
                Success = true,
                Data = data
            });
        }

        /// <summary>
        /// Issues a book to a library member and records the issue and due return dates.
        /// </summary>
        /// <param name="request">Book issue details.</param>
        /// <returns>A response indicating whether the book was issued successfully.</returns>
        [HttpPost("IssueBook")]
        public IActionResult IssueBook(IssueReturnUpsertRequest request)
        {
            var result = _libraryService.IssueBook(
                request,
                GetCompanyId(),
                GetUserId());

            return Ok(new ApiResponse<bool>
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Success
            });
        }

        /// <summary>
        /// Records the return of a previously issued book.
        /// </summary>
        /// <param name="request">Book return information.</param>
        /// <returns>A response indicating whether the return was recorded successfully.</returns>
        [HttpPost("ReturnBook")]
        public IActionResult ReturnBook(int issueId, DateTime returnDate)
        {
            var result = _libraryService.ReturnBook(
                issueId,
                returnDate,
                GetCompanyId(),
                GetUserId());

            return Ok(new ApiResponse<bool>
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Success
            });
        }

        /// <summary>
        /// Retrieves detailed information for a library member.
        /// </summary>
        /// <param name="memberId">Library member identifier.</param>
        /// <param name="companyId">Company identifier.</param>
        /// <returns>Member information including card number and contact details.</returns>
        [HttpGet("GetMemberDetails")]
        public IActionResult GetMemberDetails(int memberId, int companyId)
        {
            var result = _libraryService.GetMemberDetails(memberId, companyId);

            return Ok(new ApiResponse<MemberDetailsViewModel>
            {
                Success = true,
                Data = result
            });
        }

        /// <summary>
        /// Deletes a library member.
        /// </summary>
        /// <param name="id">Library member identifier.</param>
        /// <param name="studentId">Student identifier.</param>
        /// <param name="staffId">Staff identifier.</param>
        /// <returns>Operation result.</returns>
        [HttpDelete]
        [Route("DeleteMemberEx")]
        public IActionResult DeleteMemberEx(
            int id,
            int? studentId,
            int? staffId)
        {
            try
            {
                int userId = GetUserId();

                var result = _libraryService.DeleteMemberEx(
                    id,
                    studentId,
                    staffId,
                    userId);

                return Ok(new ApiResponse<object>
                {
                    Success = result.Success,
                    Message = result.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}
