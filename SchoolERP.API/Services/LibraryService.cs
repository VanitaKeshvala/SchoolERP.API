using Dapper;
using Microsoft.Data.SqlClient;
using SchoolERP.API.Data;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
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

                var result = conn.QueryFirstOrDefault<dynamic>(
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

                return ((int)result.Result == 1, (string)result.Message);
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
        public (bool Success, string Message) ToggleBookStatus(int id, int userId)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var result = conn.QueryFirstOrDefault<dynamic>(
                    "sp_Library_Book_CRUD",
                    new
                    {
                        Action = "TOGGLESTATUS",
                        BookID = id,
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

            var result = conn.Query<dynamic>(
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
                .Select(row => new LibraryMemberViewModel
                {
                    LibraryMemberID = row.LibraryMemberID,
                    StudentID = row.StudentID,
                    StaffID = row.StaffID,
                    LibraryCardNo = row.LibraryCardNo,
                    AdmissionNo = row.AdmissionNo,
                    Name = row.MemberName != null
                            ? row.MemberName
                            : memberType != "Staff"
                                ? row.StudentName
                                : row.StaffName,
                    MemberType = row.MemberType ?? memberType,
                    ClassName = row.ClassDepartment != null
                                ? row.ClassDepartment
                                : row.ClassName,
                    FatherName = row.FatherName,
                    DOB = row.DOB,
                    Gender = row.Gender,
                    MobileNo = row.MobileNo,
                    RegisteredOn = row.RegisteredOn
                })
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
                        StudentID = studentId,
                        StaffID = staffId,
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

                var result = conn.QueryFirstOrDefault<dynamic>(
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

                return ((int)result.Result == 1,
                        (string)result.Message);
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

                var result = conn.QueryFirstOrDefault<dynamic>(
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

                return ((int)result.Result == 1,
                        (string)result.Message);
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
        public MemberDetailsViewModel GetMemberDetails(int memberId, int companyId)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            return conn.QueryFirstOrDefault<MemberDetailsViewModel>(
                "sp_Library_IssueReturn_CRUD",
                new
                {
                    Action = "GET_MEMBER_DETAILS",
                    LibraryMemberID = memberId,
                    CompanyID = companyId
                },
                commandType: CommandType.StoredProcedure);
        }
    }
}
