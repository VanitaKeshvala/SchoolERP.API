using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Models;
using SchoolERP.Net.Services;
using System.Security.Claims;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using SchoolERP.Net.Services.Clients;

namespace SchoolERP.Net.Controllers
{
    public class LibraryController : Controller
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
            IHumanResourceClientService hrService)
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
        public IActionResult Books()
        {
            return View();
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
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var res =await _service.ToggleBookStatusAsync(id);
            return Json(new { success = res.Success, message = res.Message });
        }
        #endregion

        #region Membership
        public async Task<IActionResult> AddStudents()
        {
            var companyId = await GetCompanyId();
            var sessionId = await GetSessionId();
            ViewBag.Classes =(await _classService.GetAllAsync()).Data;
            return View();
        }

        public async Task<IActionResult> AddStaff()
        {
            var companyId = await GetCompanyId();
            var sessionId = await GetSessionId();
            ViewBag.Departments = (await _hrService.GetAllDepartmentsAsync()).Data;
            return View();
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
        public async Task<IActionResult> DeleteMember(int id, int? studentId, int? staffId)
        {
            if (studentId.HasValue || staffId.HasValue)
            {
                // Custom delete via SP update
                var companyId = GetCompanyId();
                var userId = GetUserId();
                var res =await _service.DeleteMemberExAsync(id, studentId, staffId);
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

        public IActionResult Member()
        {
            return View();
        }

        public IActionResult Issue(int id)
        {
            if (id <= 0) return RedirectToAction("Member");
            ViewBag.LibraryMemberID = id;
            return View();
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
            return Json(new { success = true, data });
        }

        [HttpPost]
        public async Task<IActionResult> IssueBook([FromBody] IssueReturnUpsertRequest req)
        {
            var res = await _service.IssueBook(req);
            return Json(new { success = res.Success, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> ReturnBook(int issueId, DateTime returnDate)
        {
            var res =await _service.ReturnBook(issueId, returnDate);
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
