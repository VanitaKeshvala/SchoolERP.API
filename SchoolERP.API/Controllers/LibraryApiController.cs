using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Interfaces;
using SchoolERP.API.Services;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using System.Security.Claims;
using System.Threading.Tasks;

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
        public async Task<IActionResult> UpsertBook([FromBody] BookUpsertRequest req)
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();
            if (req.CompanyID == null)
            {
                req.CompanyID = GetCompanyId();
            }
            var result =await _libraryService.UpsertBook(req, req.CompanyID.Value, userId);
            return Ok(new { result });
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
        public IActionResult ToggleBookStatus([FromBody] StatusUpdateRequest request)
        {
            var userId = GetUserId();
            request.DoneBy = userId;
            var result = _libraryService.ToggleBookStatus(request);

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
        public IActionResult ReturnBook([FromBody] ReturnBookIssue req)
        {
            var result = _libraryService.ReturnBook(
                req.issueId,
                req.returnDate,
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
            int? staffId, string? modeType)
        {
            try
            {
                int userId = GetUserId();

                var result = _libraryService.DeleteMemberEx(
                    id,
                    studentId,
                    staffId,
                    GetCompanyId(),
                    userId, modeType);

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


        /// <summary>
        /// Retrieves a paginated list of Libary based on the specified search criteria.
        /// Automatically resolves the current user's Company if they are not provided.
        /// </summary>
        [HttpPost("GetAllLibraryWithPage")]
        public async Task<IActionResult> GetAllLibraryWithPage([FromBody] LibrarySearchRequest request)
        {
            try
            {
                int userId = GetUserId();

                if (request.CompanyID == null)
                {
                    request.CompanyID = _companyService.GetUserCurrentCompany(userId) ?? 0;
                }
                if (request.CompanyID == 0)
                    return Ok(ApiResponse<List<BookViewModel>>.SuccessResponse(new List<BookViewModel>()));

                var data = await _libraryService.GetAllLibraryWithPage(request);
                return Ok(ApiResponse<PagedResult<BookViewModel>>.SuccessResponse(data));

            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }

        }

        [HttpPost("GetAllStudentsMembershipWithPage")]
        public async Task<IActionResult> GetAllStudentsMembershipWithPage([FromBody] StudentsMembershipSearchRequest request)
        {
            try
            {
                int userId = GetUserId();

                if (request.CompanyID == null)
                {
                    request.CompanyID = _companyService.GetUserCurrentCompany(userId) ?? 0;
                }
                if (request.CompanyID == 0)
                    return Ok(ApiResponse<List<LibraryMemberViewModel>>.SuccessResponse(new List<LibraryMemberViewModel>()));

                var data = await _libraryService.GetAllStudentsMembershipWithPage(request);
                return Ok(ApiResponse<PagedResult<LibraryMemberViewModel>>.SuccessResponse(data));

            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }

        }

        [HttpGet("GetLibraryMemberByID/{id}")]
        public IActionResult GetLibraryMemberByID(int id)
        {
            try
            {
                var data = _libraryService.GetLibraryMemberByID(id);
                if (data == null) return NotFound(ApiResponse<LibraryMember>.ErrorResponse("Class not found"));
                return Ok(ApiResponse<LibraryMember>.SuccessResponse(data));
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message }); 
            }
            
        }

        [HttpPost("GetAllStaffMembershipWithPage")]
        public async Task<IActionResult> GetAllStaffMembershipWithPage([FromBody] StaffLibraryMemberSearchModel request)
        {
            try
            {
                int userId = GetUserId();

                if (request.CompanyID == null)
                {
                    request.CompanyID = _companyService.GetUserCurrentCompany(userId) ?? 0;
                }
                if (request.CompanyID == 0)
                    return Ok(ApiResponse<List<StaffLibraryMember>>.SuccessResponse(new List<StaffLibraryMember>()));

                var data = await _libraryService.GetAllStaffMembershipWithPage(request);
                return Ok(ApiResponse<PagedResult<StaffLibraryMember>>.SuccessResponse(data));

            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }

        }


        [HttpPost("GetAllIssuedBooksWithPageIndex")]
        public async Task<IActionResult> GetAllIssuedBooksWithPageIndex([FromBody] IssueBookSearchModel request)
        {
            try
            {
                int userId = GetUserId();

                if (request.CompanyID == null)
                {
                    request.CompanyID = _companyService.GetUserCurrentCompany(userId) ?? 0;
                }
                if (request.CompanyID == 0)
                    return Ok(ApiResponse<List<IssueReturnViewModel>>.SuccessResponse(new List<IssueReturnViewModel>()));

                var data = await _libraryService.GetAllIssuedBooksWithPageIndex(request);
                return Ok(ApiResponse<PagedResult<IssueReturnViewModel>>.SuccessResponse(data));

            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }

        }

        [HttpPost("GetAllLibraryMemberWithPageIndex")]
        public async Task<IActionResult> GetAllLibraryMemberWithPageIndex([FromBody] MemberSearchModel request)
        {
            try
            {
                int userId = GetUserId();

                if (request.CompanyID == null)
                {
                    request.CompanyID = _companyService.GetUserCurrentCompany(userId) ?? 0;
                }
                if (request.CompanyID == 0)
                    return Ok(ApiResponse<List<LibraryMemberViewModel>>.SuccessResponse(new List<LibraryMemberViewModel>()));

                var data = await _libraryService.GetAllLibraryMemberWithPageIndex(request);
                return Ok(ApiResponse<PagedResult<LibraryMemberViewModel>>.SuccessResponse(data));

            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }

        }


        #region Bind AlL Library Master Table DropDwon 
        [HttpGet("GetLibraryDocumentTypeDropdownList")]
        public IActionResult GetLibraryDocumentTypeDropdownList(int companyId)
        {
            try
            {
                int userId = GetUserId();
                var result = _libraryService.GetLibraryDocumentTypeDropdownList(companyId, userId);

                return Ok(new ApiResponse<List<DropdownModel>>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
            
        }
        [HttpGet("GetLibraryDocumentStatusDropdownList")]
        public IActionResult GetLibraryDocumentStatusDropdownList(int companyId)
        {
            try
            {
                int userId = GetUserId();
                var result = _libraryService.GetLibraryDocumentStatusDropdownList(companyId, userId);

                return Ok(new ApiResponse<List<DropdownModel>>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }

        }
        [HttpGet("GetLibraryCategoryDropdownList")]
        public IActionResult GetLibraryCategoryDropdownList(int companyId)
        {
            try
            {
                int userId = GetUserId();
                var result = _libraryService.GetLibraryCategoryDropdownList(companyId, userId);

                return Ok(new ApiResponse<List<DropdownModel>>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }

        }

        [HttpGet("GetLibraryLanguageDropdownList")]
        public IActionResult GetLibraryLanguageDropdownList(int companyId)
        {
            try
            {
                int userId = GetUserId();
                var result = _libraryService.GetLibraryLanguageDropdownList(companyId, userId);

                return Ok(new ApiResponse<List<DropdownModel>>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }

        }

        [HttpGet("GetLibrarySeriesDropdownList")]
        public IActionResult GetLibrarySeriesDropdownList(int companyId)
        {
            try
            {
                int userId = GetUserId();
                var result = _libraryService.GetLibrarySeriesDropdownList(companyId, userId);

                return Ok(new ApiResponse<List<DropdownModel>>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }

        }

        [HttpGet("GetLibrarySupplierDropdownList")]
        public IActionResult GetLibrarySupplierDropdownList(int companyId)
        {
            try
            {
                int userId = GetUserId();
                var result = _libraryService.GetLibrarySupplierDropdownList(companyId, userId);

                return Ok(new ApiResponse<List<DropdownModel>>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }

        }

        [HttpGet("GetLibraryBudgetDropdownList")]
        public IActionResult GetLibraryBudgetDropdownList(int companyId)
        {
            try
            {
                int userId = GetUserId();
                var result = _libraryService.GetLibraryBudgetDropdownList(companyId, userId);

                return Ok(new ApiResponse<List<DropdownModel>>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }

        }

        [HttpGet("GetLibraryPublisherDropdownList")]
        public IActionResult GetLibraryPublisherDropdownList(int companyId)
        {
            try
            {
                int userId = GetUserId();
                var result = _libraryService.GetLibraryPublisherDropdownList(companyId, userId);

                return Ok(new ApiResponse<List<DropdownModel>>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }

        }

        [HttpGet("GetLibrarySubjectDropdownList")]
        public IActionResult GetLibrarySubjectDropdownList(int companyId, int? categoryId)
        {
            try
            {
                int userId = GetUserId();
                var result = _libraryService.GetLibrarySubjectDropdownList(companyId, categoryId, userId);

                return Ok(new ApiResponse<List<DropdownModel>>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }

        }

        [HttpGet("SearchBookTitle")]
        public async Task<IActionResult> SearchBookTitle([FromQuery] string term)
        {
            try
            {
                if (term == null)
                    return BadRequest(new ApiResponse
                    {
                        Result = 0,
                        Message = "Valid search books title are required."
                    });

                var response = await _libraryService.SearchBookTitle(term);
                return Ok(response);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet("GetLibraryAuthorDropdownList")]
        public IActionResult GetLibraryAuthorDropdownList(int companyId)
        {
            try
            {
                int userId = GetUserId();
                var result = _libraryService.GetLibraryAuthorDropdownList(companyId, userId);

                return Ok(new ApiResponse<List<DropdownModel>>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }

        }
        #endregion

        [HttpPost("UpsertBooksFrontPageAttachmentFile")]
        public IActionResult UpsertBooksFrontPageAttachmentFile([FromBody] BooksAttachmentUpsertRequest req)
        {
            try
            {
                int userId = GetUserId();
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var (success, message) = _libraryService.UpsertBooksFrontPageAttachmentFile(req, userId);
                return Ok(new { success, message });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }

        }


        [HttpPost("GetAllAccessionWithPage")]
        public async Task<IActionResult> GetAllAccessionWithPage([FromBody] LibrarySearchRequest request)
        {
            try
            {
                int userId = GetUserId();

                if (request.CompanyID == null)
                {
                    request.CompanyID = _companyService.GetUserCurrentCompany(userId) ?? 0;
                }
                if (request.CompanyID == 0)
                    return Ok(ApiResponse<List<AccessionDetailsModel>>.SuccessResponse(new List<AccessionDetailsModel>()));

                var data = await _libraryService.GetAllAccessionWithPage(request);
                return Ok(ApiResponse<PagedResult<AccessionDetailsModel>>.SuccessResponse(data));

            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }

        }

        [HttpGet("GetAccessionById")]
        public IActionResult GetAccessionById(int id,int companyId)
        {
            try
            {
                var result = _libraryService.GetAccessionById(id, companyId);

                return Ok(new ApiResponse<AccessionDetailsModel>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
            
        }

        [HttpGet("GetAccessionByNo")]
        public IActionResult GetAccessionByNo(string accessionNo, int companyid)
        {
            try
            {
                var result = _libraryService.GetAccessionByNo(accessionNo, companyid);

                return Ok(new ApiResponse<AccessionDetailsModel>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }

        }

        [HttpGet("GetIssueNo")]
        public async Task<IActionResult> GetIssueNo(int companyid)
        {
            try
            {
                var result =await _libraryService.GetIssueNo(companyid);

                return Ok(new ApiResponse<IssueNoResponse>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }

        }

        [HttpPost("UpsertAccessionStatus")]
        public IActionResult UpsertAccessionStatus([FromBody] AccessionStatusUpsertRequest req)
        {
            try
            {
                int userId = GetUserId();
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var (success, message) = _libraryService.UpsertAccessionStatus(req, userId);                
                return Ok(new { success, message });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }

        }

        [HttpPost("GetAccessionForLabels")]
        public async Task<IActionResult> GetAccessionForLabels(AccessionSearchRequest req)
        {
            try
            {
                var result =await _libraryService.GetAccessionForLabels(req);

                return Ok(new ApiResponse<List<AccessionDetailsModel>>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
            
        }

        [HttpPost("SaveIssueBook")]
        public  IActionResult SaveIssueBook([FromBody] LibraryIssueSaveRequest req)
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();
            if (req.CompanyID == null)
            {
                req.CompanyID = GetCompanyId();
            }
            var result = _libraryService.SaveIssueBook(req,userId);
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                result.success,
                result.message
            }));
        }

        [HttpPost("ReturnIssueBook")]
        public IActionResult ReturnIssueBook([FromBody] LibraryReturnRequest req)
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();
            if (req.CompanyID == null)
            {
                req.CompanyID = GetCompanyId();
            }
            var result = _libraryService.ReturnIssueBook(req, userId);
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                result.success,
                result.message
            }));
        }

        [HttpGet("GetBookDetails")]
        public async Task<IActionResult> GetBookDetails(int bookId)
        {
            try
            {
                var result = await _libraryService.GetBookDetails(bookId);
                if (result.Book == null)
                    return Ok(new { success = false, message = "Book not found." });
                //BookDetailsResult
                return Ok(new ApiResponse<BookDetailsResult>
                {
                    Success = true,
                    Data = result
                });
               // return Ok(new { success = true, data = result.Book, issueHistory = result.IssueHistory });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message=ex.Message});
            }
            
        }

        [HttpPost("AddStudentsMembershipBulkWithCards")]
        public IActionResult AddStudentsMembershipBulkWithCards(
           [FromBody] BulkMembershipRequest req,
           int companyId,
           int userId)
        {
            var address = string.Empty;
            var result = _libraryService.AddStudentsMembershipBulkWithCards(
                req,
                companyId,
                userId, address);

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                result.success,
                result.message
            }));
        }


        [HttpGet("SearchMember")]
        public IActionResult SearchMember(int companyId, string memberType, string? searchText)
        {
            try
            {
                int userId = GetUserId();
                var result = _libraryService.SearchMember(companyId, memberType, searchText);

                return Ok(new ApiResponse<List<LibraryMemberSearchResult>>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }

        }

        [HttpGet("SearchAccessionNo")]
        public IActionResult SearchAccessionNo(int companyId, string? searchText)
        {
            try
            {
                int userId = GetUserId();
                var result = _libraryService.SearchAccessionNo(companyId, searchText);

                return Ok(new ApiResponse<List<AccessionDetailsModel>>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }

        }
    }
}
