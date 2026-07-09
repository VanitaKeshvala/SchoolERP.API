using Dapper;
using Microsoft.Data.SqlClient;
using SchoolERP.API.Data;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using System.Data;

namespace SchoolERP.API.Services
{
    /// <summary>
    /// This service handles the school library operations.
    /// It manages the catalog of books, student and staff memberships, 
    /// and the process of issuing and returning books.
    /// </summary>
    public class LibraryService: ILibraryService
    {
        private readonly IConfiguration _configuration;
        public LibraryService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Retrieves all books for the company with optional search filtering.
        /// </summary>
        public List<BookViewModel> GetBookList(int companyId, string? searchTerm)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.Query<BookViewModel>(
                "sp_Library_Book_CRUD",
                new
                {
                    Action = "LIST",
                    CompanyID = companyId,
                    SearchTerm = searchTerm
                },
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        /// <summary>
        /// Retrieves a specific book by its ID.
        /// </summary>
        public BookViewModel GetBookById(int id)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.QueryFirstOrDefault<BookViewModel>(
                "sp_Library_Book_CRUD",
                new
                {
                    Action = "GETBYID",
                    BookID = id
                },
                commandType: CommandType.StoredProcedure
            );
        }

        /// <summary>
        /// Adds a new book or updates an existing book.
        /// </summary>
        public (bool Success, string Message) UpsertBook(BookUpsertRequest req, int companyId, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Library_Book_CRUD",
                    new
                    {
                        Action = "SAVE",
                        req.BookID,
                        req.BookTitle,
                        req.BookNo,
                        req.ISBNNo,
                        req.Publisher,
                        req.Author,
                        req.Subject,
                        req.RackNo,
                        req.TotalQty,
                        req.BookPrice,
                        req.PostDate,
                        req.Description,
                        CompanyID = companyId,
                        UserID = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return (
                    result?.Result == 1,
                    result?.Message ?? "Operation completed."
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Deletes a book from the library.
        /// </summary>
        public (bool Success, string Message) DeleteBook(List<int> id, int userId)
        {
            try
            {
                if (id == null || !id.Any())
                {
                    return (false, "No students selected for deletion.");
                }

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string bookIDs = string.Join(",", id);

                var result = conn.QueryFirstOrDefault<dynamic>(
                    "SP_LIBRARY_BOOK_DELETE",
                    new
                    {
                        BookID = bookIDs,
                        UserID = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return ((int)result.Result == 1, (string)result.Message);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Activates or deactivates a book.
        /// </summary>
        public (bool Success, string Message) ToggleBookStatus(StatusUpdateRequest request)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));


                var parameters = new DynamicParameters();
                parameters.Add("@BOOKID", request.Ids);
                parameters.Add("@IsActive", request.IsActive);
                parameters.Add("@USERID", request.DoneBy);

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "SP_LIBRARY_BOOK_TOGGLESTATUS",
                    parameters,
                    commandType: CommandType.StoredProcedure);
                return (
                    result?.Result == 1,
                    result?.Message ?? "Operation completed."
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // Membership
        /// <summary>
        /// Retrieves library members based on member type and filters.
        /// </summary>
        public List<LibraryMemberViewModel> GetMemberList(
     string memberType,
     int companyId,
     int? classId,
     int? sectionId,
     int? departmentId,
     string? search)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var action = memberType == "All" ? "LIST_ALL" : "LIST";

            var result = conn.Query<LibraryMemberViewModel>(
                "sp_Library_Member_CRUD",
                new
                {
                    Action = action,
                    MemberType = memberType == "All" ? null : memberType,
                    CompanyID = companyId,
                    ClassID = classId,
                    SectionID = sectionId,
                    DepartmentID = departmentId,
                    SearchTerm = search
                },
                commandType: CommandType.StoredProcedure)
                .ToList();

            return result;
        }

        /// <summary>
        /// Searches students or staff available for library membership.
        /// </summary>
        public List<MembershipSearchViewModel> SearchForMembership(
            string memberType,
            int companyId,
            int? classId,
            int? sectionId,
            int? departmentId,
            string? search)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var action = memberType == "Student"
                ? "SEARCH_STUDENTS"
                : "SEARCH_STAFF";

            return conn.Query<MembershipSearchViewModel>(
                "sp_Library_Member_CRUD",
                new
                {
                    Action = action,
                    CompanyID = companyId,
                    ClassID = classId,
                    SectionID = sectionId,
                    DepartmentID = departmentId,
                    SearchTerm = search
                },
                commandType: CommandType.StoredProcedure)
                .ToList();
        }
        /// <summary>
        /// Adds a new library member.
        /// </summary>
        public (bool Success, string Message) AddMember(
            LibraryMemberUpsertRequest req,
            int companyId,
            int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<dynamic>(
                    "sp_Library_Member_CRUD",
                    new
                    {
                        Action = "SAVE",
                        LibraryMemberID =req.LibraryMemberID,
                        MemberType = req.MemberType,
                        StudentID = req.StudentID,
                        StaffID = req.StaffID,
                        LibraryCardNo = req.LibraryCardNo,
                        CompanyID = companyId,
                        UserID = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return ((int)result.Result == 1,
                        result.Message?.ToString() ?? "");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        /// <summary>
        /// Deletes a library member.
        /// </summary>
        public (bool Success, string Message) DeleteMember(
            int id,
            int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<dynamic>(
                    "sp_Library_Member_CRUD",
                    new
                    {
                        Action = "DELETE",
                        LibraryMemberID = id,
                        UserID = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return ((int)result.Result == 1,
                        result.Message?.ToString() ?? "");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Deletes a library member using member, student or staff identifiers.
        /// </summary>
        public (bool Success, string Message) DeleteMemberEx(
            int id,
            int? studentId,
            int? staffId,
            int? companyId,
            int userId, string? modeType)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Library_Member_CRUD",
                    new
                    {
                        Action = "DELETE",
                        MemberType = modeType,
                        LibraryMemberID = id,
                        StudentID = studentId,
                        StaffID = staffId,
                        CompanyId= companyId,
                        UserID = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return (
                    result?.Result == 1,
                    result?.Message ?? "Operation completed."
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // Issue/Return
        /// <summary>
        /// Retrieves all books currently issued to a library member.
        /// </summary>
        public List<IssueReturnViewModel> GetIssuedBooks(int memberId, int companyId)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.Query<IssueReturnViewModel>(
                "sp_Library_IssueReturn_CRUD",
                new
                {
                    Action = "LIST",
                    LibraryMemberID = memberId,
                    CompanyID = companyId
                },
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        /// <summary>
        /// Records that a book has been issued to a member.
        /// It sets the issue date and expected return date.
        /// </summary>
        public (bool Success, string Message) IssueBook(
            IssueReturnUpsertRequest req,
            int companyId,
            int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Library_IssueReturn_CRUD",
                    new
                    {
                        Action = "SAVE",
                        LibraryMemberID = req.LibraryMemberID,
                        BookID = req.BookID,
                        IssueDate = req.IssueDate,
                        DueReturnDate = req.DueReturnDate,
                        CompanyID = companyId,
                        UserID = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return (
                    result?.Result == 1,
                    result?.Message ?? "Operation completed."
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        /// <summary>
        /// Records that a book has been returned to the library.
        /// </summary>
        /// <summary>
        /// Records that a book has been returned to the library.
        /// </summary>
        public (bool Success, string Message) ReturnBook(
            int issueId,
            DateTime returnDate,
            int companyId,
            int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Library_IssueReturn_CRUD",
                    new
                    {
                        Action = "RETURN",
                        IssueReturnID = issueId,
                        ReturnDate = returnDate,
                        CompanyID = companyId,
                        UserID = userId
                    },
                    commandType: CommandType.StoredProcedure);

                return (
                    result?.Result == 1,
                    result?.Message ?? "Operation completed."
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Retrieves library member details by Member ID.
        /// </summary>
        /// <param name="memberId">Library Member ID.</param>
        /// <param name="companyId">Company ID.</param>
        /// <returns>Member details if found; otherwise null.</returns>
        public MemberDetailsViewModel? GetMemberDetails(int memberId, int companyId)
        {
            try
            {
                using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

                return conn.QueryFirstOrDefault<MemberDetailsViewModel>(
                    "sp_Library_GetMemberDetails",
                    new
                    {
                        LibraryMemberID = memberId,
                        CompanyID = companyId
                    },
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {

                throw;
            }
            
        }

        /// <summary>
        /// Retrieves a paginated list of classes based on the specified search criteria.
        /// Automatically resolves the current user's Company and Session if they are not provided.
        /// </summary>
        public async Task<PagedResult<BookViewModel>> GetAllLibraryWithPage(LibrarySearchRequest req)
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var param = new DynamicParameters();
                if (req.PageNumber == 0 && req.PageSize == 0)
                {
                    req.PageNumber = 1;
                    req.PageSize = 10;
                }


                param.Add("@CompanyID", req.CompanyID);
                param.Add("@SearchKeyword", req.SearchKeyword);
                param.Add("@PageNumber", req.PageNumber);
                param.Add("@PageSize", req.PageSize);

                var result = (await conn.QueryAsync<BookViewModel>(
                "SP_LIBRARY_BOOK_GETALLWITHPAGEINDEX",
                param,
                commandType: CommandType.StoredProcedure)).ToList();


                int res = result.FirstOrDefault()?.Result ?? 0;
                int totalRecords = result.FirstOrDefault()?.TOTALRECORDS ?? 0;
                int pageIndex = result.FirstOrDefault()?.CURRENTPAGE ?? 0;
                int pageSize = result.FirstOrDefault()?.PageSize ?? 0;

                var userModel = new PagedResult<BookViewModel>
                {
                    Data = result,
                    TotalRecords = totalRecords,
                    PageNumber = pageIndex,
                    PageSize = pageSize
                };

                if (res == 0)
                {
                    userModel = new PagedResult<BookViewModel>
                    {
                        Data = null,
                        TotalRecords = totalRecords,
                        PageNumber = pageIndex,
                        PageSize = pageSize
                    };
                }
                return userModel;

            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public async Task<PagedResult<LibraryMemberViewModel>> GetAllStudentsMembershipWithPage(StudentsMembershipSearchRequest req)
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var param = new DynamicParameters();
                if (req.PageNumber == 0 && req.PageSize == 0)
                {
                    req.PageNumber = 1;
                    req.PageSize = 10;
                }


                param.Add("@COMPANYID", req.CompanyID);
                param.Add("@SESSIONID", req.SessionId);
                param.Add("@CLASSID", req.ClassId);
                param.Add("@SECTIONID", req.SectionId);
                param.Add("@SearchKeyword", req.SearchKeyword);
                param.Add("@PageNumber", req.PageNumber);
                param.Add("@PageSize", req.PageSize);

                var result = (await conn.QueryAsync<LibraryMemberViewModel>(
                "SP_LIBRARY_MEMBER_STUDENT_GETALLWITHPAGEINDEX",
                param,
                commandType: CommandType.StoredProcedure)).ToList();


                int res = result.FirstOrDefault()?.Result ?? 0;
                int totalRecords = result.FirstOrDefault()?.TOTALRECORDS ?? 0;
                int pageIndex = result.FirstOrDefault()?.CURRENTPAGE ?? 0;
                int pageSize = result.FirstOrDefault()?.PageSize ?? 0;

                var userModel = new PagedResult<LibraryMemberViewModel>
                {
                    Data = result,
                    TotalRecords = totalRecords,
                    PageNumber = pageIndex,
                    PageSize = pageSize
                };

                if (res == 0)
                {
                    userModel = new PagedResult<LibraryMemberViewModel>
                    {
                        Data = null,
                        TotalRecords = totalRecords,
                        PageNumber = pageIndex,
                        PageSize = pageSize
                    };
                }
                return userModel;

            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public LibraryMember? GetLibraryMemberByID(int libraryMemberID)
        {
            try
            {
                using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

                return conn.QueryFirstOrDefault<LibraryMember>(
                    "sp_LibraryMember_GetByID",
                    new
                    {
                        LibraryMemberID = libraryMemberID
                    },
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        public async Task<PagedResult<StaffLibraryMember>> GetAllStaffMembershipWithPage(StaffLibraryMemberSearchModel req)
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var param = new DynamicParameters();
                if (req.PageIndex == 0 && req.PageSize == 0)
                {
                    req.PageIndex = 1;
                    req.PageSize = 10;
                }


                param.Add("@COMPANYID", req.CompanyID);
                param.Add("@DEPARTMENTID", req.DepartmentID);
                param.Add("@SEARCHTERM", req.SearchTerm);
                param.Add("@PAGEINDEX", req.PageIndex);
                param.Add("@PAGESIZE", req.PageSize);

                var result = (await conn.QueryAsync<StaffLibraryMember>(
                "SP_LIBRARY_MEMBER_STAFF_LIST",
                param,
                commandType: CommandType.StoredProcedure)).ToList();


                int res = result.FirstOrDefault()?.Result ?? 0;
                int totalRecords = result.FirstOrDefault()?.TOTALRECORDS ?? 0;
                int pageIndex = result.FirstOrDefault()?.CURRENTPAGE ?? 0;
                int pageSize = result.FirstOrDefault()?.PageSize ?? 0;

                var userModel = new PagedResult<StaffLibraryMember>
                {
                    Data = result,
                    TotalRecords = totalRecords,
                    PageNumber = pageIndex,
                    PageSize = pageSize
                };

                if (res == 0)
                {
                    userModel = new PagedResult<StaffLibraryMember>
                    {
                        Data = null,
                        TotalRecords = totalRecords,
                        PageNumber = pageIndex,
                        PageSize = pageSize
                    };
                }
                return userModel;

            }
            catch (Exception ex)
            {
                throw;
            }

        }


        public async Task<PagedResult<IssueReturnViewModel>> GetAllIssuedBooksWithPageIndex(IssueBookSearchModel req)
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var param = new DynamicParameters();
                if (req.PageNumber == 0 && req.PageSize == 0)
                {
                    req.PageNumber = 1;
                    req.PageSize = 5;
                }


                param.Add("@LibraryMemberID", req.LibraryMemberID);
                param.Add("@CompanyID", req.CompanyID);
                param.Add("@SearchKeyword", req.SearchkeyWord);
                param.Add("@PageNumber", req.PageNumber);
                param.Add("@PageSize", req.PageSize);

                var result = (await conn.QueryAsync<IssueReturnViewModel>(
                "sp_Library_GetAllIssuedBooksWithPageIndex",
                param,
                commandType: CommandType.StoredProcedure)).ToList();


                int res = result.FirstOrDefault()?.Result ?? 0;
                int totalRecords = result.FirstOrDefault()?.TOTALRECORDS ?? 0;
                int pageIndex = result.FirstOrDefault()?.CURRENTPAGE ?? 0;
                int pageSize = result.FirstOrDefault()?.PageSize ?? 0;

                var userModel = new PagedResult<IssueReturnViewModel>
                {
                    Data = result,
                    TotalRecords = totalRecords,
                    PageNumber = pageIndex,
                    PageSize = pageSize
                };

                if (res == 0)
                {
                    userModel = new PagedResult<IssueReturnViewModel>
                    {
                        Data = null,
                        TotalRecords = totalRecords,
                        PageNumber = pageIndex,
                        PageSize = pageSize
                    };
                }
                return userModel;

            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public async Task<PagedResult<LibraryMemberViewModel>> GetAllLibraryMemberWithPageIndex(MemberSearchModel req)
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var param = new DynamicParameters();
                if (req.PageNumber == 0 && req.PageSize == 0)
                {
                    req.PageNumber = 1;
                    req.PageSize = 10;
                }

                param.Add("@CompanyID", req.CompanyID);
                param.Add("@SearchKeyword", req.SearchkeyWord);
                param.Add("@PageNumber", req.PageNumber);
                param.Add("@PageSize", req.PageSize);

                var result = (await conn.QueryAsync<LibraryMemberViewModel>(
                "SP_LIBRARY_MEMBER_LIST_PAGED",
                param,
                commandType: CommandType.StoredProcedure)).ToList();


                int res = result.FirstOrDefault()?.Result ?? 0;
                int totalRecords = result.FirstOrDefault()?.TOTALRECORDS ?? 0;
                int pageIndex = result.FirstOrDefault()?.CURRENTPAGE ?? 0;
                int pageSize = result.FirstOrDefault()?.PageSize ?? 0;

                var userModel = new PagedResult<LibraryMemberViewModel>
                {
                    Data = result,
                    TotalRecords = totalRecords,
                    PageNumber = pageIndex,
                    PageSize = pageSize
                };

                if (res == 0)
                {
                    userModel = new PagedResult<LibraryMemberViewModel>
                    {
                        Data = null,
                        TotalRecords = totalRecords,
                        PageNumber = pageIndex,
                        PageSize = pageSize
                    };
                }
                return userModel;

            }
            catch (Exception ex)
            {
                throw;
            }

        }
    }
}
