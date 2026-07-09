using Microsoft.AspNetCore.Mvc;
using SchoolERP.Shared.Models;
using SchoolERP.Net.Services;
using System.Security.Claims;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using SchoolERP.Net.Services.Clients;
using SchoolERP.Net.Helpers;
using static System.Collections.Specialized.BitVector32;
using SchoolERP.Shared.Models.Common;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace SchoolERP.Net.Controllers
{
    public class LibraryController : BaseController
    {
        private readonly ILibraryClientService _service;
        private readonly ICompanyClientService _companyService;
        private readonly IClassClientService _classService;
        private readonly ISectionClientService _sectionService;
        private readonly ISessionClientService _sessionService;
        private readonly IHumanResourceClientService _hrService;

        public LibraryController(
            ILibraryClientService service,
            ICompanyClientService companyService,
            IClassClientService classService,
            ISectionClientService sectionService,
            ISessionClientService sessionService,
            IHumanResourceClientService hrService, PermissionHelper permHelper) : base(permHelper)
        {
            _service = service;
            _companyService = companyService;
            _classService = classService;
            _sectionService = sectionService;
            _sessionService = sessionService;
            _hrService = hrService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("UserId")?.Value ?? "0");
        private async Task<int> GetCompanyId()
        {
            var response = await _companyService.GetUserCurrentCompanyAsync();
            return response?.Data ?? 0;
        }
        private async Task<int> GetSessionId()
        {
            var response = await _sessionService.GetUserCurrentSessionAsync();
            return response?.Data ?? 0;
        }

        #region Books
        public async Task<IActionResult> Books(int? pageIndex,
        int? pageSize,
        string? search,
        int? companyId)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Library/Books"
               );

                var request = new LibrarySearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId()
                };


                var sessionId = await GetSessionId();
                var libraryResponse = _service.GetAllLibraryWithPageAsync(request);
                var companiesTask = _companyService.GetAllAsync();

                await Task.WhenAll(libraryResponse,  companiesTask);

                var pagedResult = await libraryResponse;

                var model = new HRDepartmentPageViewModel
                {
                    ItemsBooks = pagedResult.Success ? pagedResult.Data.Data : new List<BookViewModel>(),
                    Companies = (await companiesTask).Data ?? new(),
                    TotalRecords = pagedResult.Data.TotalRecords,
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,
                    SearchTerm = search,
                    CompanyId = companyId
                };
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
            
        }

        public async Task<IActionResult> AddBooks(int? id)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Library/Books"
               );
                var sessionId = await GetSessionId();
                var model = new HRDepartmentAddPageViewModel();

                if (id.HasValue && id.Value > 0)
                {
                    var response = await _service.GetBookByIdAsync(id.Value);
                    if (response.Success)
                    {
                        model.ItemsBooks = response.Data;
                        model.EditBooks = response.Data;
                    }
                }
                else
                {
                    model.EditBooks = null;
                }

                model.Permissions = perms;
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBookList(string? search)
        {
            var response = await _service.GetBookListAsync(await GetCompanyId(), search);
            return Json(new { response.Success , response.Data });
        }

        [HttpGet]
        public async Task<IActionResult> GetBook(int id)
        {
            var response =await _service.GetBookByIdAsync(id);
            return Json(new { success = response.Success != null, response.Data });
        }

        [HttpPost]
        public async Task<IActionResult> UpsertBook([FromBody] BookUpsertRequest req)
        {
            var companyId = await GetCompanyId();
            if (companyId <= 0) return Json(new { success = false, message = "Valid Company ID required." });

            var res = await _service.UpsertBookAsync(req);
            return Json(new { success = res.Success, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBook([FromBody] List<int> id)
        {
            var res =await _service.DeleteBookAsync(id);
            return Json(new { success = res.Success, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus([FromBody] StatusUpdateRequest request)
        {
            try
            {
                var res = await _service.ToggleBookStatusAsync(request);
                return Json(new { success = res.Success, message = res.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            
        }
        #endregion

        #region Membership
        public async Task<IActionResult> AddStudents(int? pageIndex,
        int? pageSize,
        string? search,
        int? companyId,
        int? sessionId,
        int? sectionId,
        int? classId)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Library/AddStudents"
               );


                var request = new StudentsMembershipSearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionId = sessionId ?? await GetSessionId(),
                    SectionId = sectionId ?? null,
                    ClassId=classId??null
                };


                var models = new HRStudentsMembershipPageViewModel { };

                var studentMemberResponse = _service.GetAllStudentsMembershipWithPageAsync(request);

                var sessionTask = _sessionService.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();

                var classResponse = await _classService.GetAllAsync(false, await GetSessionId());


                await Task.WhenAll(studentMemberResponse, sessionTask, companiesTask);

                var pagedResult = await studentMemberResponse;


                var model = new HRStudentsMembershipPageViewModel
                {
                    Items = pagedResult.Success ? pagedResult.Data.Data : new List<LibraryMemberViewModel>(),
                    Classes = classResponse.Success ? classResponse.Data : new List<MstClassViewModel>(),
                    Companies = (await companiesTask).Data ?? new(),
                    TotalRecords = pagedResult.Data.TotalRecords,
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,
                    SearchTerm = search,
                    CompanyId = companyId,
                    SectionId = sectionId,
                    ClassId=classId
                };

                model.Permissions = perms;
                //var companyId = await GetCompanyId();
               // var sessionId = await GetSessionId();
                //ViewBag.Classes = (await _classService.GetAllAsync(false, sessionId)).Data;
                return View(model);
            }
            catch (Exception ex)
            {
                throw;
            }
           
        }



        public async Task<IActionResult> AddLibraryStudentsMembership(int? id, int? studentId)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Library/AddLibraryStudentsMembership"
               );
                var model = new HRStudentsMembershipAddViewModel();
                if (id.HasValue && id.Value > 0)
                {
                    var response = await _service.GetLibraryMemberByID(id.Value);
                    if (response.Success)
                    {
                        model.EditItem = response.Data;
                    }
                }
                else
                {
                    model.EditItem = null;
                }
                model.StudentId = studentId;
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<IActionResult> AddStaff(int? pageIndex,
        int? pageSize,
        string? search,
        int? companyId,
        int? departmentID)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Library/AddStaff"
               );
                var request = new StaffLibraryMemberSearchModel
                {
                    PageIndex = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchTerm = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    DepartmentID = departmentID ?? null
                };


                var models = new StaffMemberPageViewModel { };

                var staffMemberResponse = _service.GetAllStaffMembershipWithPageAsync(request);

                var departmet = await _hrService.GetAllDepartmentsAsync();
                var companiesTask = _companyService.GetAllAsync();


                await Task.WhenAll(staffMemberResponse, companiesTask);

                var pagedResult = await staffMemberResponse;


                var model = new StaffMemberPageViewModel
                {
                    Items = pagedResult.Success ? pagedResult.Data.Data : new List<StaffLibraryMember>(),
                    Departments = departmet.Success ? departmet.Data.Data : new List<HRDepartmentViewModel>(),
                    Companies = (await companiesTask).Data ?? new(),
                    TotalRecords = pagedResult.Data.TotalRecords,
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,
                    SearchTerm = search,
                    CompanyId = companyId,
                    DepartmentID = departmentID
                };

                model.Permissions = perms;
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }            

        }


        public async Task<IActionResult> AddLibraryStaffMembership(int? id, int? staffId)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Library/AddLibraryStaffMembership"
               );
                var model = new HRStudentsMembershipAddViewModel();
                if (id.HasValue && id.Value > 0)
                {
                    var response = await _service.GetLibraryMemberByID(id.Value);
                    if (response.Success)
                    {
                        model.EditItem = response.Data;
                    }
                }
                else
                {
                    model.EditItem = null;
                }
                model.StaffId = staffId;
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMemberList(string type, int? classId, int? sectionId, int? deptId, string? search)
        {
            var response =await _service.GetMemberListAsync(type, await GetCompanyId(), classId, sectionId, deptId, search);
            return Json(new { response.Success, response.Data });
        }

        [HttpGet]
        public async Task<IActionResult> SearchForMembership(string type, int? classId, int? sectionId, int? deptId, string? search)
        {
            var response =await _service.SearchForMembershipAsync(type, await GetCompanyId(), classId, sectionId, deptId, search);
            return Json(new { response.Success, response.Data });
        }

        [HttpPost]
        public async Task<IActionResult> AddMember([FromBody] LibraryMemberUpsertRequest req)
        {
            var res =await _service.AddMemberAsync(req, await GetCompanyId(), GetUserId());
            return Json(new { success = res.Success, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMember(int id, int? studentId, int? staffId,string? modeType)
        {
            if (studentId.HasValue || staffId.HasValue)
            {
                // Custom delete via SP update
                var companyId = GetCompanyId();
                var userId = GetUserId();
                var res =await _service.DeleteMemberExAsync(id, studentId, staffId, modeType);
                return Json(new { success = res.Success, message = res.Message });
            }
            else
            {
                var res =await _service.DeleteMember(id, GetUserId());
                return Json(new { success = res.Success, message = res.Message });
            }
        }
        #endregion

        [HttpGet]
        public async Task<IActionResult> GetSections(int classId)
        {
            var sections = (await _sectionService.GetByClassAsync(classId)).Data;
            return Json(sections);
        }

        public async Task<IActionResult> Member(int? pageIndex,
        int? pageSize,
        string? search,
        int? companyId)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/HumanResource/Staffs"
               );

                var request = new MemberSearchModel
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchkeyWord = search,
                    CompanyID = companyId ?? await GetCompanyId()
                };


                var sessionId = await GetSessionId();
                var libraryResponse = _service.GetAllLibraryMemberWithPageIndexAsync(request);
                var companiesTask = _companyService.GetAllAsync();

                await Task.WhenAll(libraryResponse, companiesTask);

                var pagedResult = await libraryResponse;

                var model = new MemberPageViewModel
                {
                    Items = pagedResult.Success ? pagedResult.Data.Data : new List<LibraryMemberViewModel>(),
                    Companies = (await companiesTask).Data ?? new(),
                    TotalRecords = pagedResult.Data.TotalRecords,
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,
                    SearchTerm = search,
                    CompanyId = companyId
                };
                model.Permissions = perms;
                return View(model);

            }
            catch (Exception)
            {

                throw;
            }
            
        }

        public async Task<IActionResult> Issue(int id, int? pageIndex,
        int? pageSize,
        string? search,
        int? companyId)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Library/Issue"
               );


                var request = new IssueBookSearchModel
                {
                    LibraryMemberID=id,
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 5,
                    SearchkeyWord = search,
                    CompanyID = companyId ?? await GetCompanyId()
                };


                var model = new IssuePageViewModel();
                if (id <= 0) return RedirectToAction("Member");
                if (id > 0)
                {

                    var issueBookResponse = _service.GetAllIssuedBooksWithPageIndexAsync(request);

                    var companiesTask = _companyService.GetAllAsync();

                    var classResponse = await _classService.GetAllAsync(false, await GetSessionId());


                    await Task.WhenAll(issueBookResponse,  companiesTask);

                    var pagedResult = await issueBookResponse;


                    var response =await _service.GetMemberDetails(id,await GetCompanyId());
                    var issueResponse =await _service.GetIssuedBooks(id, await GetCompanyId());                   

                    model = new IssuePageViewModel
                    {
                        IssueReturn = pagedResult.Success ? pagedResult.Data.Data : new List<IssueReturnViewModel>(),
                        MemberDetails = response.Success ? response.Data : new MemberDetailsViewModel(),
                        Companies = (await companiesTask).Data ?? new(),
                        TotalRecords = pagedResult.Data.TotalRecords,
                        PageNumber = pagedResult.Data.PageNumber,
                        PageSize = pagedResult.Data.PageSize,
                        SearchTerm = search,
                        CompanyId = companyId,
                        LibraryMemberID = id
                    };

                }

                model.Permissions = perms;
                ViewBag.LibraryMemberID = id;
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        [HttpGet]
        public async Task<IActionResult> GetMemberDetails(int id)
        {
            var data =await _service.GetMemberDetails(id, await GetCompanyId());
            return Json(new { success = data != null, data });
        }

        [HttpGet]
        public async Task<IActionResult> GetIssuedBooks(int memberId)
        {
            var data = _service.GetIssuedBooks(memberId, await GetCompanyId());
            return Json(new { success = true, data.Result.Data });
        }

        [HttpPost]
        public async Task<IActionResult> IssueBook([FromBody] IssueReturnUpsertRequest req)
        {
            var res = await _service.IssueBook(req);
            return Json(new { success = res.Success, message = res.Message });
        }

        public async Task<IActionResult> AddIssueBook(int? id) 
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Library/AddIssueBook"
               );
                string search = string.Empty;
                var response = await _service.GetBookListAsync(await GetCompanyId(), search);
                var model = new IssueBookAddViewModel
                {
                    BookViewModels = response.Data,
                    Permissions = perms,
                    LibraryMemberID=id
                };
                return View(model);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IActionResult> AddIssueBookReturn(int? issueReturnID, int libraryMemberID)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Library/AddIssueBook"
               );
                string search = string.Empty;
                var response = await _service.GetBookListAsync(await GetCompanyId(), search);
                var model = new IssueBookAddViewModel
                {
                    BookViewModels = response.Data,
                    Permissions = perms,
                    LibraryMemberID = libraryMemberID,
                    IssueReturnID= issueReturnID
                };
                return View(model);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReturnBook(int issueId, DateTime returnDate)
        {
            ReturnBookIssue req = new ReturnBookIssue();
            req.issueId = issueId;
            req.returnDate = returnDate;
            var res =await _service.ReturnBook(req);
            return Json(new { success = res.Success, message = res.Message });
        }

        public IActionResult ImportBook()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ImportBook(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Please select a CSV file." });

            try
            {
                var companyId = await GetCompanyId();
                var userId = GetUserId();

                using var reader = new StreamReader(file.OpenReadStream());
                string? headerLine = await reader.ReadLineAsync();
                if (headerLine == null) return Json(new { success = false, message = "File is empty." });

                var headers = headerLine.Split(',').Select(h => h.Trim().ToLower()).ToList();
                var results = new List<object>();

                while (!reader.EndOfStream)
                {
                    string? line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = line.Split(',').Select(v => v.Trim()).ToList();
                    var req = new BookUpsertRequest();

                    for (int i = 0; i < headers.Count && i < values.Count; i++)
                    {
                        string header = headers[i];
                        string value = values[i];

                        switch (header)
                        {
                            case "book_title": req.BookTitle = value; break;
                            case "book_no": req.BookNo = value; break;
                            case "isbn_no": req.ISBNNo = value; break;
                            case "subject": req.Subject = value; break;
                            case "rack_no": req.RackNo = value; break;
                            case "publish": req.Publisher = value; break;
                            case "author": req.Author = value; break;
                            case "qty": req.TotalQty = int.TryParse(value, out var qty) ? qty : 0; break;
                            case "perunitcost": req.BookPrice = decimal.TryParse(value, out var price) ? price : 0; break;
                            case "postdate": req.PostDate = DateTime.TryParse(value, out var date) ? date : (DateTime?)null; break;
                            case "description": req.Description = value; break;
                        }
                    }

                    var res =await _service.UpsertBookAsync(req);
                    results.Add(new
                    {
                        bookTitle = req.BookTitle,
                        success = res.Success,
                        message = res.Message
                    });
                }
                return Json(new { success = true, results });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
