using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SchoolERP.API.Data;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using System.ComponentModel.Design;
using System.Data;
using System.Reflection;
using System.Text.Json;

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
                "SP_LIBRARY_BOOK_GETALL",
                new
                {
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
                "SP_LIBRARY_BOOK_GETBYID",
                new
                {
                    BookID = id
                },
                commandType: CommandType.StoredProcedure
            );
        }

        /// <summary>
        /// Adds a new book or updates an existing book.
        /// </summary>
        public async Task<SpBooksResult> UpsertBook(BookUpsertRequest req, int companyId, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var parameters = new DynamicParameters();

                parameters.Add("@BOOKID", req.BookID);
                parameters.Add("@COMPANYID", companyId);
                parameters.Add("@BOOKTITLE", req.BookTitle);
                parameters.Add("@SUBTITLE", req.SubTitle);
                parameters.Add("@BOOKNO", req.BookNo);
                parameters.Add("@ISBNNO", req.ISBNNo);
                parameters.Add("@PUBLISHERID", req.PublisherId);
                //parameters.Add("@AUTHORID", req.AuthorId); // Uncomment if SP accepts AUTHORID
                parameters.Add("@DOCUMENTID", req.DocumentId);
                parameters.Add("@CATEGORYID", req.CategoryId);
                parameters.Add("@SUBJECTID", req.SubjectId);
                parameters.Add("@LANGUAGEID", req.LanguageId);
                parameters.Add("@SERIESID", req.SeriesId);
                parameters.Add("@EDITION", req.Edition);
                parameters.Add("@CLASSNO", req.ClassNo);
                parameters.Add("@VOLUME", req.Volume);
                parameters.Add("@NOOFPAGES", req.NoOfPages);
                parameters.Add("@KEYWORD", req.KeyWord);
                parameters.Add("@COVERPAGEIMAGE", req.CoverPageImage);
                parameters.Add("@REMARKS_TEXT", req.Remarks);
                parameters.Add("@RACKNO", req.RackNo);
                parameters.Add("@TOTALQTY", req.TotalQty);
                parameters.Add("@BOOKPRICE", req.BookPrice);
                parameters.Add("@POSTDATE", req.PostDate);
                parameters.Add("@DESCRIPTION", req.Description);
                parameters.Add("@ISACTIVE", req.IsActive);
                parameters.Add("@USERID", userId);
                parameters.Add("@IPADDRESS", req.IPAddress);

                var result = await conn.QueryFirstOrDefaultAsync<SpBooksResult>(
                    "SP_LIBRARY_BOOK_SAVE",
                    parameters,
                    commandType: CommandType.StoredProcedure);


                if (result != null) 
                {
                    if (result?.Result == 1) 
                    {
                        var bookId = result?.BookId;
                        //add Author

                        parameters = new DynamicParameters();

                        parameters.Add("@BOOKID", bookId);
                        parameters.Add("@COMPANYID", companyId);
                        parameters.Add("@AUTHORIDLIST", req.AuthorId);
                        parameters.Add("@USERID", userId);
                        parameters.Add("@IPADDRESS", req.IPAddress);

                        var results = await conn.QueryFirstOrDefaultAsync<SpResult>(
                            "SP_TBL_MST_MAP_LibraryAuthor_UPSERT",
                            parameters,
                            commandType: CommandType.StoredProcedure);

                        //Add Acces

                        parameters = new DynamicParameters();
                        parameters.Add("@COMPANYID", companyId);
                        parameters.Add("@BOOKID", bookId);                        
                        parameters.Add("@BUDGETID", req.BudgetId);
                        parameters.Add("@QUANTITY", req.Quantity);
                        parameters.Add("@SUPPLIERID", req.SupplierId);
                        parameters.Add("@DocumentStatusId", req.DocumentStatusId);
                        parameters.Add("@BILLNO", req.BillNo);
                        parameters.Add("@BILLDATE", req.BillDate);
                        parameters.Add("@LOCATION", req.Location);
                        parameters.Add("@PRICE", req.Price);
                        parameters.Add("@PRICE", req.Price);
                        parameters.Add("@USERID", userId);
                        parameters.Add("@IPADDRESS", req.IPAddress);

                        var accessio = await conn.QueryFirstOrDefaultAsync<SpResult>(
                            "SP_LIBRARY_ACCESSION_GENERATE",
                            parameters,
                            commandType: CommandType.StoredProcedure);

                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return new SpBooksResult();
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

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "SP_LIBRARY_BOOK_DELETE",
                    new
                    {
                        BookID = bookIDs,
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

                var parameters = new DynamicParameters();
                if (req.PageNumber == 0 && req.PageSize == 0)
                {
                    req.PageNumber = 1;
                    req.PageSize = 10;
                }


                
                parameters.Add("@COMPANYID", req.CompanyID);
                parameters.Add("@SEARCHTEXT", req.SearchText);
                parameters.Add("@PUBLISHERID", req.PublisherID);
                parameters.Add("@AUTHORID", req.AuthorID);
                parameters.Add("@SUBJECTID", req.SubjectID);
                parameters.Add("@CATEGORYID", req.CategoryID);
                parameters.Add("@LANGUAGEID", req.LanguageID);
                parameters.Add("@SERIESID", req.SeriesID);
                parameters.Add("@DOCUMENTID", req.DocumentID);
                parameters.Add("@ISACTIVE", req.IsActive);
                parameters.Add("@SORTCOLUMN", req.SortColumn);
                parameters.Add("@SORTDIR", req.SortDir);
                parameters.Add("@PAGENUMBER", req.PageNumber);
                parameters.Add("@PAGESIZE", req.PageSize);

                var result = (await conn.QueryAsync<BookViewModel>(
                "SP_LIBRARY_BOOK_GETALLWITHPAGEINDEX",
                parameters,
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
                param.Add("@AccessionNo", req.AccessionNo);
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

        #region All Libary Master Table Dropdwonbind
        public List<DropdownModel> GetLibraryDocumentTypeDropdownList(int companyId,int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.Query<DropdownModel>(
                    "sp_TBL_MST_DocumentType_GetAllDropDownBind",
                    new
                    {
                        CompanyID = companyId,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure)
                    .ToList();

                return result;
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        public List<DropdownModel> GetLibraryDocumentStatusDropdownList(int companyId, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.Query<DropdownModel>(
                    "sp_TBL_MST_DocumentStatus_GetAllDropDownBind",
                    new
                    {
                        CompanyID = companyId,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure)
                    .ToList();

                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public List<DropdownModel> GetLibraryCategoryDropdownList(int companyId, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.Query<DropdownModel>(
                    "sp_TBL_MST_LibraryCategory_GetAllDropDownBind",
                    new
                    {
                        CompanyID = companyId,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure)
                    .ToList();

                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public List<DropdownModel> GetLibraryLanguageDropdownList(int companyId, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.Query<DropdownModel>(
                    "sp_TBL_MST_LibraryLanguage_GetAllDropDownBind",
                    new
                    {
                        CompanyID = companyId,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure)
                    .ToList();

                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public List<DropdownModel> GetLibrarySeriesDropdownList(int companyId, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.Query<DropdownModel>(
                    "sp_TBL_MST_LibrarySeries_GetAllDropDownBind",
                    new
                    {
                        CompanyID = companyId,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure)
                    .ToList();

                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public List<DropdownModel> GetLibrarySupplierDropdownList(int companyId, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.Query<DropdownModel>(
                    "sp_TBL_MST_LibrarySupplier_GetAllDropDownBind",
                    new
                    {
                        CompanyID = companyId,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure)
                    .ToList();

                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public List<DropdownModel> GetLibraryBudgetDropdownList(int companyId, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.Query<DropdownModel>(
                    "sp_TBL_MST_LibraryBudget_GetAllDropDownBind",
                    new
                    {
                        CompanyID = companyId,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure)
                    .ToList();

                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public List<DropdownModel> GetLibraryPublisherDropdownList(int companyId, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.Query<DropdownModel>(
                    "sp_TBL_MST_Publisher_GetAllDropDownBind",
                    new
                    {
                        CompanyID = companyId,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure)
                    .ToList();

                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public List<DropdownModel> GetLibraryAuthorDropdownList(int companyId, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.Query<DropdownModel>(
                    "sp_tbl_mst_Author_GetAllDropDownBind",
                    new
                    {
                        CompanyID = companyId,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure)
                    .ToList();

                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public List<DropdownModel> GetLibrarySubjectDropdownList(int companyId,int? categoryId, int userId)
        {
            try
            {
                if (categoryId == 0) 
                {
                    categoryId = null;
                }
                using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.Query<DropdownModel>(
                    "sp_TBL_MST_MAP_LibraryCategorySubject_GetAllDropDownBind",
                    new
                    {
                        CompanyID = companyId,
                        CategoryId= categoryId,
                        UserId = userId
                    },
                    commandType: CommandType.StoredProcedure)
                    .ToList();

                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<List<BookViewModel>> SearchBookTitle(string term)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var parameters = new DynamicParameters();
                parameters.Add("@SEARCHKEYWORD", term);

                var result = conn.Query<BookViewModel>(
                    "SP_tbl_Library_Books_SEARCH",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                // If SP returned no rows at all
                if (!result.Any()) return null;

                // If SP returned rows but RESULT != 1 (failure case)
                if (result.First().Result != 1) return null;

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion


        public (bool success, string message) UpsertBooksFrontPageAttachmentFile(
     BooksAttachmentUpsertRequest req, int userId)
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                var parameters = new DynamicParameters();

                parameters.Add("@BOOKID", req.BookId);
                parameters.Add("@ATTACHMENT", req.Attachment);
                parameters.Add("@FILENAME", req.FileName);
                parameters.Add("@FILETYPE", req.FileType);
                parameters.Add("@USERID", userId);

                var result = conn.QueryFirstOrDefault<dynamic>(
                    "SP_LIBRARY_BOOKS_UPSERT_ATTACHMENT",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return (
                    Convert.ToInt32(result.Result) == 1,
                    Convert.ToString(result.Message) ?? string.Empty
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        //Ass
        public async Task<PagedResult<AccessionDetailsModel>> GetAllAccessionWithPage(LibrarySearchRequest req)
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
                param.Add("@BOOKID", req.BookId);
                param.Add("@SearchKeyword", req.SearchText);
                param.Add("@PageNumber", req.PageNumber);
                param.Add("@PageSize", req.PageSize);

                var result = (await conn.QueryAsync<AccessionDetailsModel>(
                "SP_LIBRARY_ACCESSION_GETALLWITHPAGEINDEX",
                param,
                commandType: CommandType.StoredProcedure)).ToList();


                int res = result.FirstOrDefault()?.Result ?? 0;
                int totalRecords = result.FirstOrDefault()?.TOTALRECORDS ?? 0;
                int pageIndex = result.FirstOrDefault()?.CURRENTPAGE ?? 0;
                int pageSize = result.FirstOrDefault()?.PageSize ?? 0;

                var userModel = new PagedResult<AccessionDetailsModel>
                {
                    Data = result,
                    TotalRecords = totalRecords,
                    PageNumber = pageIndex,
                    PageSize = pageSize
                };

                if (res == 0)
                {
                    userModel = new PagedResult<AccessionDetailsModel>
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
        public AccessionDetailsModel? GetAccessionById(int id,int companyid)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.QueryFirstOrDefault<AccessionDetailsModel>(
                "SP_LIBRARY_ACCESSION_GETBYID",
            new
                {
                    CompanyId=companyid,
                    AccessionId = id
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public AccessionDetailsModel? GetAccessionByNo(string accessionNo, int companyid)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.QueryFirstOrDefault<AccessionDetailsModel>(
                "SP_LIBRARY_ACCESSION_GET_BY_NO",
            new
            {
                CompanyId = companyid,
                AccessionNo = accessionNo
            },
                commandType: CommandType.StoredProcedure
            );
        }

        public (bool success, string message) UpsertAccessionStatus(
     AccessionStatusUpsertRequest req, int userId)
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                var parameters = new DynamicParameters();

                parameters.Add("@COMPANYID", req.CompanyID);
                parameters.Add("@ACCESSIONID", req.AccessionId);
                parameters.Add("@STATUSID", req.DocumentStatusId);
                parameters.Add("@USERID", userId);

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "SP_LIBRARY_ACCESSION_STATUS_UPDATE",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

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

        public async Task<List<AccessionDetailsModel>> GetAccessionForLabels(AccessionSearchRequest req)
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var param = new DynamicParameters();
                


                param.Add("@CompanyId", req.CompanyId);
                param.Add("@Mode", req.Mode);
                param.Add("@FromAccessionNo", req.FromAccessionNo);
                param.Add("@ToAccessionNo", req.ToAccessionNo);
                param.Add("@AccessionNo", req.AccessionNo);

                var result = (await conn.QueryAsync<AccessionDetailsModel>(
                "SP_LIBRARY_ACCESSION_LABEL_LIST",
                param,
                commandType: CommandType.StoredProcedure)).ToList();

                return result;

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IssueNoResponse> GetIssueNo(int companyID)
        {
            try
            {
                using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

                return conn.QueryFirstOrDefault<IssueNoResponse> (
                    "SP_LIBRARY_GET_NEXT_ISSUENO",
                new
                {
                    CompanyId = companyID
                },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public (bool success, string message) SaveIssueBook(
     LibraryIssueSaveRequest req, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var parameters = new DynamicParameters();

                parameters.Add("@LibraryMemberID", req.LibraryMemberID);
                parameters.Add("@IssueDate", req.IssueDate);
                parameters.Add("@IssueNo", req.IssueNo);
                parameters.Add("@CompanyID", req.CompanyID);
                parameters.Add("@UserID", userId);
                parameters.Add("@IssueListJson", req.IssueListJson);

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Library_Issue_Save",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return
                (
                    result?.Result == 1,
                    result?.Message ?? "Operation completed."
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool success, string message) ReturnIssueBook(
    LibraryReturnRequest req, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var parameters = new DynamicParameters();

                parameters.Add("@UserID", userId);
                parameters.Add("@ReturnListJson", req.ReturnListJson);

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "sp_Library_Issue_Return",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return
                (
                    result?.Result == 1,
                    result?.Message ?? "Operation completed."
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }


        public async Task<BookDetailsResult> GetBookDetails(int bookId)
        {
            try
            {
                using var connection = new SqlConnection(
                   _configuration.GetConnectionString("DefaultConnection"));
                var parameters = new DynamicParameters();
                parameters.Add("@BOOKID", bookId);

                using var multi = await connection.QueryMultipleAsync(
                    "SP_LIBRARY_BOOK_DETAILS_GET",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure);

                // ---- result set 1: single book row -------------------------------
                var book = await multi.ReadSingleOrDefaultAsync<BookDetailsDto>();

                if (book != null && !string.IsNullOrWhiteSpace(book.AccessionJson))
                {
                    // FOR JSON PATH gives you a JSON *array* string — deserialize directly.
                    book.Accessions = JsonSerializer.Deserialize<List<AccessionDto>>(
                        book.AccessionJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    ) ?? new List<AccessionDto>();
                }

                // ---- result set 2: issue history ----------------------------------
                var issueHistory = (await multi.ReadAsync<IssueHistoryDto>()).AsList();

                return new BookDetailsResult
                {
                    Book = book,
                    IssueHistory = issueHistory
                };
            }
            catch (Exception)
            {
                throw;
            }
            
        }


        // Same call, but reads both result sets so the grid can show the
        // newly generated card numbers immediately without a reload.
        public (bool success, string message, List<IssuedCard> issuedCards) AddStudentsMembershipBulkWithCards(
            BulkMembershipRequest request,
            int companyId,
            int userId,
            string? ipAddress)
        {
            var issuedCards = new List<IssuedCard>();

            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var table = new DataTable();
                table.Columns.Add("MemberType", typeof(string));
                table.Columns.Add("StudentID", typeof(int));                
                table.Columns.Add("StaffID", typeof(int));
                table.Columns.Add("AdmissionNo", typeof(string));
                table.Columns.Add("NoOfDocuments", typeof(int));
                table.Columns.Add("MaxDays", typeof(int));
                table.Columns.Add("ExpiryDate", typeof(DateTime));
                table.Columns.Add("RenewDate", typeof(DateTime));
                table.Columns.Add("IsExistingMember", typeof(bool));
                table.Columns.Add("LibraryMemberID", typeof(int));
                table.Columns.Add("LibraryCardNo", typeof(string));
                
               

                foreach (var s in request.Students)
                {
                    table.Rows.Add(
                        (object?)s.MemberType ?? DBNull.Value,                       
                        (object?)s.StudentID ?? DBNull.Value,
                        (object?)s.StaffID ?? DBNull.Value,
                        (object?)s.AdmissionNo ?? DBNull.Value,
                        (object?)s.NoOfDocuments ?? DBNull.Value,
                        (object?)s.MaxDays ?? DBNull.Value,
                        (object?)s.ExpiryDate ?? DBNull.Value,
                        (object?)s.RenewDate ?? DBNull.Value,
                        s.IsExistingMember,                        
                        (object?)s.LibraryMemberID ?? DBNull.Value,
                        (object?)s.LibraryCardNo ?? DBNull.Value
                        
                    );
                }

                var tvpParam = new SqlParameter("@Students", table)
                {
                    TypeName = "dbo.TVP_LibraryStudentMembership",
                    SqlDbType = SqlDbType.Structured
                };

                var parameters = new DynamicParameters();
                parameters.Add("@COMPANYID", companyId);
                parameters.Add("@SESSIONYEAR", request.SessionYear);
                parameters.Add("@USERID", userId);
                parameters.Add("@DEFAULTNOOFDOC", request.DefaultNoOfDoc);
                parameters.Add("@DEFAULTMAXDAYS", request.DefaultMaxDays);
                parameters.Add("@IPADDRESS", ipAddress);
                parameters.Add(
                    "@Students",
                    table.AsTableValuedParameter("dbo.TVP_LibraryStudentMembership"));

                using var multi = conn.QueryMultiple(
                    "SP_LIBRARY_STUDENT_MEMBERSHIP_BULK_SAVE",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                var result = multi.ReadFirstOrDefault<SpBulkMemberResult>();

                if (result?.Result == 1 && !multi.IsConsumed)
                    issuedCards = multi.Read<IssuedCard>().ToList();

                return (result?.Result == 1, result?.Message ?? "Operation completed.", issuedCards);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, issuedCards);
            }
        }



        public List<LibraryMemberSearchResult> SearchMember(int companyId, string memberType,string? searchText)
        {
            try
            {
                using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

                var parameters = new DynamicParameters();
                parameters.Add("@CompanyID", companyId);
                parameters.Add("@MemberType", memberType);
                parameters.Add("@SearchText", searchText);
                               
                var result = conn.Query<LibraryMemberSearchResult>(
                   "USP_Library_Member_Search",
                   parameters,
                   commandType: CommandType.StoredProcedure
               ).ToList();
               

                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public List<AccessionDetailsModel> SearchAccessionNo(int companyId, string? searchText)
        {
            try
            {
                using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

                var parameters = new DynamicParameters();
                parameters.Add("@CompanyID", companyId);
                parameters.Add("@SearchText", searchText);

                var result = conn.Query<AccessionDetailsModel>(
                   "SP_LIBRARY_ACCESSION_SEARCH",
                   parameters,
                   commandType: CommandType.StoredProcedure
               ).ToList();


                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }

    }
}

