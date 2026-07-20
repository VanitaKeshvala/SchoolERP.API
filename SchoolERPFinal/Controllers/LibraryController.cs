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
using static System.Runtime.InteropServices.JavaScript.JSType;
using Humanizer;

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
        private readonly IConfiguration _configuration;
        private readonly IPhotoUploadService _photoService;
        public LibraryController(
            ILibraryClientService service,
            ICompanyClientService companyService,
            IClassClientService classService,
            ISectionClientService sectionService,
            ISessionClientService sessionService,
            IHumanResourceClientService hrService,
            IConfiguration configuration,
            IPhotoUploadService photoService, PermissionHelper permHelper) : base(permHelper)
        {
            _service = service;
            _companyService = companyService;
            _classService = classService;
            _sectionService = sectionService;
            _sessionService = sessionService;
            _hrService = hrService;
            _configuration = configuration;
            _photoService = photoService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("UserId")?.Value ?? "0");
        private async Task<int> GetCompanyId()
        {
            if (CurrentCompanyId == null)
            {
                var response = await _companyService.GetUserCurrentCompanyAsync();
                return response?.Data ?? 0;
            }
            return CurrentCompanyId;
        }
        private async Task<int> GetSessionId()
        {
            var response = await _sessionService.GetUserCurrentSessionAsync();
            return response?.Data ?? 0;
        }

        #region Books
        // ==========================================================================
        // Books() action - server-side paging + full filter support
        // All filters (publisher, author, subject, category, language, series,
        // documentType, isActive) now flow through query string -> LibrarySearchRequest
        // -> stored procedure, exactly like pageIndex/search already did.
        // ==========================================================================
        public async Task<IActionResult> Books(
            int? pageIndex,
            int? pageSize,
            string? search,
            int? companyId,
            int? publisherId,
            int? authorId,
            int? subjectId,
            int? categoryId,
            int? languageId,
            int? seriesId,
            int? documentId,
            bool? isActive,
            string? sortColumn,
            string? sortDir)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions("/Library/Books");

                var request = new LibrarySearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchText = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    PublisherID = publisherId,
                    AuthorID = authorId,
                    SubjectID = subjectId,
                    CategoryID = categoryId,
                    LanguageID = languageId,
                    SeriesID = seriesId,
                    DocumentID = documentId,
                    IsActive = isActive,
                    SortColumn = string.IsNullOrWhiteSpace(sortColumn) ? "BookID" : sortColumn,
                    SortDir = string.IsNullOrWhiteSpace(sortDir) ? "DESC" : sortDir
                };


                // Fire all lookups (grid data + every filter dropdown's option list) in parallel
                var libraryResponseTask = _service.GetAllLibraryWithPageAsync(request);
                var companiesTask = _companyService.GetAllAsync();
                var publishersTask = _service.GetLibraryPublisherDropdownListAsync(request.CompanyID.Value);
                var authorsTask = _service.GetLibraryAuthorDropdownListAsync(request.CompanyID.Value);
                var subjectsTask = _service.GetLibrarySubjectDropdownListAsync(request.CompanyID.Value,0);   // all subjects, unfiltered by category, for the list-page filter
                var categoriesTask = _service.GetLibraryCategoryDropdownListAsync(request.CompanyID.Value);
                var languagesTask = _service.GetLibraryLanguageDropdownListAsync(request.CompanyID.Value);
                var seriesTask = _service.GetLibrarySeriesDropdownListAsync(request.CompanyID.Value);
                var documentTypesTask = _service.GetLibraryDocumentTypeDropdownListAsync(request.CompanyID.Value);

                await Task.WhenAll(
                    libraryResponseTask, companiesTask, publishersTask, authorsTask,
                    subjectsTask, categoriesTask, languagesTask, seriesTask, documentTypesTask);

                var pagedResult = await libraryResponseTask;

                var model = new HRDepartmentPageViewModel
                {
                    ItemsBooks = pagedResult.Success ? pagedResult.Data.Data : new List<BookViewModel>(),
                    Companies = (await companiesTask).Data ?? new(),
                    Publishers = (await publishersTask).Data ?? new(),
                    Authors = (await authorsTask).Data ?? new(),
                    Subjects = (await subjectsTask).Data ?? new(),
                    Categories = (await categoriesTask).Data ?? new(),
                    Languages = (await languagesTask).Data ?? new(),
                    SeriesList = (await seriesTask).Data ?? new(),
                    DocumentTypes = (await documentTypesTask).Data ?? new(),

                    TotalRecords = pagedResult.Data.TotalRecords,
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,

                    SearchTerm = search,
                    CompanyId = companyId,
                    PublisherId = publisherId,
                    AuthorId = authorId,
                    SubjectId = subjectId,
                    CategoryId = categoryId,
                    LanguageId = languageId,
                    SeriesId = seriesId,
                    DocumentId = documentId,
                    IsActiveFilter = isActive,
                    SortColumn = request.SortColumn,
                    SortDir = request.SortDir
                };
                model.Permissions = perms;

                return View(model);
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        // ==========================================================================
        // LibrarySearchRequest - unchanged from what you already have, included here
        // only for reference (fixed SearchText naming to match what's used above).
        // ==========================================================================
        
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
                var documentType = await _service.GetLibraryDocumentTypeDropdownListAsync(await GetCompanyId());
                var documentStatus = await _service.GetLibraryDocumentStatusDropdownListAsync(await GetCompanyId());
                var category = await _service.GetLibraryCategoryDropdownListAsync(await GetCompanyId());
                var language = await _service.GetLibraryLanguageDropdownListAsync(await GetCompanyId());
                var series = await _service.GetLibrarySeriesDropdownListAsync(await GetCompanyId());
                var publisher = await _service.GetLibraryPublisherDropdownListAsync(await GetCompanyId());
                var author = await _service.GetLibraryAuthorDropdownListAsync(await GetCompanyId());
                var budget = await _service.GetLibraryBudgetDropdownListAsync(await GetCompanyId());
                var supplier = await _service.GetLibrarySupplierDropdownList(await GetCompanyId());
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
                model.DocumentType = documentType.Data;
                model.DocumentStatus = documentStatus.Data;
                model.Category = category.Data;
                model.Language = language.Data;
                model.Series = series.Data;
                model.Publisher = publisher.Data;
                model.Author = author.Data;
                model.Budget = budget.Data;
                model.Supplier = supplier.Data;
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
        public async Task<IActionResult> UpsertBook([FromForm] BookUpsertRequest req, IFormFile? attachmentFile)
        {
            try
            {
                var companyId = await GetCompanyId();
                if (companyId <= 0)
                    return Json(new { success = false, message = "Valid Company ID required." });

                if (attachmentFile != null && attachmentFile.Length > 0)
                {
                    long maxFileSizeMB = _configuration.GetValue<long>("FileUploadSettings:MaxFileSizeMB");
                    long maxBytes = maxFileSizeMB * 1024L * 1024L;
                    if (attachmentFile.Length > maxBytes)
                        return Json(new { success = false, message = $"File size exceeds the {maxFileSizeMB} MB limit." });

                    req.FileName = attachmentFile.FileName;
                    req.FileType = attachmentFile.ContentType;
                }

                var res = await _service.UpsertBookAsync(req);

                if (res.Data?.Result?.Result == 1)
                {
                    int? bookId = res.Data.Result.BookId;
                    if (attachmentFile != null && attachmentFile.Length > 0 && bookId.HasValue)
                    {
                        using var memoryStream = new MemoryStream();
                        await attachmentFile.CopyToAsync(memoryStream);
                        byte[] fileBytes = memoryStream.ToArray();

                        var photoResult = await _photoService.SaveBase64PhotoAsync(
                            Convert.ToBase64String(fileBytes),
                            req.FileName ?? "photo.jpg",
                            PhotoModule.BookCoverPage,
                            FolderNameModule.Documents,
                            bookId.Value
                        );

                        var attachmentReq = new BooksAttachmentUpsertRequest
                        {
                            BookId = bookId.Value,
                            Attachment = photoResult.PhotoUrl,
                            FileName = photoResult.FileName,
                            FileType = attachmentFile.ContentType
                        };
                        await _service.UpsertBooksFrontPageAttachmentFileAsync(attachmentReq);
                    }
                }

                return Json(new { success = res.Success, message = res.Message });
            }
            catch (Exception ex)
            {
                // Log ex — don't rethrow a raw exception out of an AJAX endpoint,
                // the caller will just see a generic 500 with no useful `message`.
                return Json(new { success = false, message = "An unexpected error occurred while saving the book." });
            }
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
                var session = await _sessionService.GetByIDAsync(await GetSessionId());
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
                model.SessionYear = session.Data?.SessionTitle;
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
                var session = await _sessionService.GetByIDAsync(await GetSessionId());
                var classes = await _classService.GetAllAsync(false,await GetSessionId());
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
                model.CurrentSession = session.Data?.SessionTitle;
                model.Classes = classes.Data;
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
                var session = await _sessionService.GetByIDAsync(await GetSessionId());
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
                model.SessionYear = session.Data.SessionTitle;
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
                var documentStatus = await _service.GetLibraryDocumentStatusDropdownListAsync(await GetCompanyId());
                var issueResponce = await _service.GetIssueNoAsync(await GetCompanyId());
                var model = new IssueBookAddViewModel
                {
                    BookViewModels = response.Data,
                    Permissions = perms,
                    LibraryMemberID = id,
                    DocumentStatus = documentStatus.Data,
                    IssueNo= issueResponce.Data.IssueNo
                };
                return View(model);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IActionResult> AddIssueBookReturn(int? issueReturnID, int libraryMemberID, int? pageIndex,
        int? pageSize,
        string? search,
        int? companyId)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Library/AddIssueBook"
               );

                var request = new IssueBookSearchModel
                {
                    LibraryMemberID = libraryMemberID,
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 5,
                    SearchkeyWord = search,
                    CompanyID = companyId ?? await GetCompanyId()
                };


              //  var model = new IssuePageViewModel();
                
               var issueBookResponse = _service.GetAllIssuedBooksWithPageIndexAsync(request);
                await Task.WhenAll(issueBookResponse);

                var pagedResult = await issueBookResponse;
                var response = await _service.GetBookListAsync(await GetCompanyId(), string.Empty);
                var model = new IssueBookAddViewModel
                {
                    BookReturnViewModels = pagedResult.Success ? pagedResult.Data.Data : new List<IssueReturnViewModel>(),
                   // BookViewModels = issueBookResponse.Data,
                    Permissions = perms,
                    LibraryMemberID = libraryMemberID,
                    IssueReturnID= issueReturnID,
                    TotalRecords = pagedResult.Data.TotalRecords,
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,
                    SearchTerm = search,
                    CompanyId = companyId
                };
                return View(model);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<IActionResult> BookDetails(int? id) 
        {
            try
            {
                var model = new BookDetailsResult();
                var perms = await GetPermissions(
                   "/Library/AddIssueBook"
               );

                if (id == null || id <= 0)
                    return RedirectToAction("Books");

                var response = await _service.GetBookDetailsAsync(id.Value);

                if (!response.Success || response.Data?.Book == null)
                {
                    // TempData works well here since the target view has no model
                    // to attach an inline error to before it even renders.
                    TempData["ErrorMessage"] = response.Message ?? "Book not found.";
                    return RedirectToAction("Books");
                }
                
                model = response.Data;
                model.Permissions = perms;
                // View gets the unwrapped BookDetailsResult, not the ApiResponse wrapper —
                // the ApiResponse envelope (Success/Message) is only useful at the point
                // you're deciding whether to render the page at all, which is here.
                return View(model);
            }
            catch (Exception)
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
                            case "subject": req.SubjectId = int.TryParse(value, out var subject) ? subject : 0; break;
                            case "rack_no": req.RackNo = value; break;
                            case "publish": req.PublisherId = int.TryParse(value, out var publish) ? publish : 0; break;
                            //case "author": req.AuthorId = int.TryParse(value, out var AuthorId) ? AuthorId : 0; break;
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


        [HttpGet]
        public async Task<IActionResult> GetLibrarySubjectDropdownList(int categoryId)
        {
            try
            {
                var data = _service.GetLibrarySubjectDropdownListAsync(await GetCompanyId(), categoryId);
                return Json(new { success = true, data.Result.Data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message=ex.Message });
            }
            
        }

        [HttpGet]
        public async Task<IActionResult> SearchBookTitle(string term)
        {
            try
            {
                var response = await _service.SearchBookTitleAsync(term);
                return Json(new { success = response.Success, data = response.Data });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBookDetails(int id)
        {
            try
            {
                var response = await _service.GetBookByIdAsync(id);
                return Json(new { success = response.Success, data = response.Data });
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<IActionResult> Accession(int? pageIndex,
        int? pageSize,
        int? bookId,
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
                    SearchText = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    BookId=bookId??null
                };


                var sessionId = await GetSessionId();
                var libraryResponse = _service.GetAllAccessionWithPageAsync(request);
                var companiesTask = _companyService.GetAllAsync();

                await Task.WhenAll(libraryResponse, companiesTask);

                var pagedResult = await libraryResponse;
                var bookData = await _service.GetBookListAsync(await GetCompanyId(), search);
                var model = new AccessionPageViewModel
                {
                    Items = pagedResult.Success ? pagedResult.Data.Data : new List<AccessionDetailsModel>(),
                    Companies = (await companiesTask).Data ?? new(),
                    TotalRecords = pagedResult.Data.TotalRecords,
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,
                    SearchTerm = search,
                    ItemsBooks = bookData.Data,
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

        public async Task<IActionResult> AddAccessionStatus(int? id)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Library/Books"
               );
                var sessionId = await GetSessionId();
                var model = new AccessionAddViewModel();
                var documentStatus = await _service.GetLibraryDocumentStatusDropdownListAsync(await GetCompanyId());
                if (id.HasValue && id.Value > 0)
                {
                    var response = await _service.GetAccessionById(id.Value,await GetCompanyId());
                    if (response.Success)
                    {
                        model.Items = response.Data;
                    }
                }
                else
                {
                    model.Items = null;
                }
                model.DocumentStatus = documentStatus.Data;
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAccessionById(string accessionNo)
        {
            try
            {
                var response = await _service.GetAccessionByNo(accessionNo,await GetCompanyId());
                return Json(new { success = response.Success, data = response.Data });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //UpdateAccessionStatus
        [HttpPost]
        public async Task<IActionResult> UpdateAccessionStatus([FromBody] AccessionStatusUpsertRequest req)
        {
            try
            {
                req.CompanyID = await GetCompanyId();
                var response = await _service.UpsertAccessionStatusAsync(req);
                return Json(new { success = response.Success,message=response.Message, data = response.Data });
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IActionResult> BarCodeLabels(string? mode,
        string? from,
        string? to,
        string? accessionNo,
        int? companyId)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Library/Books"
               );

                var request = new AccessionSearchRequest
                {
                    Mode = mode ??null,
                    CompanyId = companyId ?? await GetCompanyId(),
                    FromAccessionNo = from,
                    ToAccessionNo = to ?? null,
                    AccessionNo = accessionNo ?? null
                };


                var sessionId = await GetSessionId();
                var libraryResponse = _service.GetAccessionForLabelsAsync(request);
                var companiesTask = _companyService.GetAllAsync();

                await Task.WhenAll(libraryResponse, companiesTask);

                var pagedResult = await libraryResponse;
                var model = new BarCodeLabelsPageViewModel
                {
                    Items = pagedResult.Success ? pagedResult.Data : new List<AccessionDetailsModel>(),
                    Mode = mode ?? null,
                    CompanyId = companyId ?? await GetCompanyId(),
                    FromAccessionNo = from,
                    ToAccessionNo = to ?? null,
                    AccessionNo = accessionNo ?? null
                };
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }

        }

             [HttpGet]
        public async Task<IActionResult> GetAccessionForLabels(string? mode,
        string? from,
        string? to,
        string? accessionNo,
        int? companyId)
        {
            try
            {
                var request = new AccessionSearchRequest
                {
                    Mode = mode ?? null,
                    CompanyId = companyId ?? await GetCompanyId(),
                    FromAccessionNo = from,
                    ToAccessionNo = to ?? null,
                    AccessionNo = accessionNo ?? null
                };
                var response = await _service.GetAccessionForLabelsAsync(request);
                return Json(new { success = response.Success, data = response.Data });
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        [HttpPost]
        public async Task<IActionResult> SaveIssueBook([FromBody] LibraryIssueSaveRequest req)
        {
            try
            {
                req.CompanyID = await GetCompanyId();
                req.IssueDate = DateTime.Now;
                var res = await _service.SaveIssueBook(req);
                return Json(new { success = res.Data.Result, message = res.Data.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> ReturnIssueBook([FromBody] LibraryReturnRequest req)
        {
            try
            {
                req.CompanyID = await GetCompanyId();
                var res = await _service.ReturnIssueBook(req);
                return Json(new { success = res.Success, message = res.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> AddStudentsMembershipBulk([FromBody] BulkMembershipRequest req)
        {
            var res = await _service.AddStudentsMembershipBulkWithCards(req, await GetCompanyId(), GetUserId());
            return Json(new { success = res.Success, message = res.Message });
        }

        [HttpGet]
        public async Task<IActionResult> SearchMember(string memberType, string? searchText)
        {
            try
            {
                var res = await _service.SearchMemberAsync(await GetCompanyId(), memberType, searchText);
                return Json(new { success = res.Success, data = res.Data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            
        }
        [HttpGet]
        public async Task<IActionResult> SearchAccession(string? searchText)
        {
            try
            {
                var res = await _service.SearchAccessionNoAsync(await GetCompanyId(),  searchText);
                return Json(new { success = res.Success, data = res.Data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }
        [HttpGet]
        public async Task<IActionResult> GetIssuedBooksByMember(
    int libraryMemberID,
    string ? accessionNo,
    int? pageIndex,
    int? pageSize,
    string? search)
        {
            try
            {
                if (libraryMemberID <= 0)
                    return Json(new { success = false, message = "Invalid member id." });

                var request = new IssueBookSearchModel
                {
                    LibraryMemberID = libraryMemberID,
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 5,
                    SearchkeyWord = search,
                    CompanyID = await GetCompanyId(),
                    AccessionNo=accessionNo
                };

                var pagedResult = await _service.GetAllIssuedBooksWithPageIndexAsync(request);

                if (!pagedResult.Success)
                    return Json(new { success = false, message = pagedResult.Message ?? "Failed to load issued books." });

                var items = (pagedResult.Data?.Data ?? new List<IssueReturnViewModel>());

                return Json(new
                {
                    success = true,
                    data = items,
                    totalRecords = pagedResult.Data?.TotalRecords ?? 0,
                    pageNumber = pagedResult.Data?.PageNumber ?? 1,
                    pageSize = pagedResult.Data?.PageSize ?? (pageSize ?? 5)
                });
            }
            catch (Exception)
            {
                throw;
            }
        }
        //GetIssuedBooksByMember
    }
}
