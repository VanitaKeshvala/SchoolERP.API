using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Models;
using SchoolERP.Net.Services;
using SchoolERP.Net.Services.Clients;
using System.Security.Claims;

namespace SchoolERP.Net.Controllers
{
    public class DownloadCenterController : Controller
    {
        private readonly IDownloadCenterClientService _service;
        private readonly ICompanyClientService _companyService;
        private readonly IClassClientService _classService;
        private readonly ISectionClientService _sectionService;
        private readonly ISessionClientService _sessionService;
        private readonly IWebHostEnvironment _env;

        public DownloadCenterController(
            IDownloadCenterClientService service,
            ICompanyClientService companyService,
            IClassClientService classService,
            ISectionClientService sectionService,
            ISessionClientService sessionService,
            IWebHostEnvironment env)
        {
            _service = service;
            _companyService = companyService;
            _classService = classService;
            _sectionService = sectionService;
            _sessionService = sessionService;
            _env = env;
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

        #region Content Type
        public IActionResult ContentType()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetContentTypeList(string? search)
        {
            var response =await _service.GetContentTypeList(search);
            return Json(new { response.Success, response.Data });
        }

        [HttpGet]
        public IActionResult GetContentType(int id)
        {
            var data = _service.GetContentTypeById(id);
            return Json(new { success = data != null, data });
        }

        [HttpPost]
        public async Task<IActionResult> UpsertContentType([FromBody] ContentTypeUpsertRequest req)
        {
            var companyId = await GetCompanyId();
            if (companyId <= 0) return Json(new { success = false, message = "Valid Company ID required." });

            var res =await _service.UpsertContentType(req);
            return Json(new { success = res.Success, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteContentType([FromBody] List<int> id)
        {
            var res =await _service.DeleteContentType(id);
            return Json(new
            {
                success = res.Success,
                message = res.Message
            });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var res =await _service.ToggleContentTypeStatus(id);
            return Json(new { success = res.Success, message = res.Message });
        }
        #endregion

        #region Video Tutorial
        public async Task<IActionResult> VideoTutorial()
        {
            var companyId = await GetCompanyId();
            var sessionId = await GetSessionId();
            ViewBag.Classes =(await _classService.GetAllAsync()).Data;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetVideoTutorialList(int? classId, int? sectionId, string? search)
        {
            var data =await _service.GetVideoTutorialList(classId, sectionId, search);
            return Json(new { success = true, data });
        }

        [HttpGet]
        public IActionResult GetVideoTutorial(int id)
        {
            var data = _service.GetVideoTutorialById(id);
            return Json(new { success = data != null, data });
        }

        [HttpPost]
        public async Task<IActionResult> UpsertVideoTutorial([FromBody] VideoTutorialUpsertRequest req)
        {
            var companyId = await GetCompanyId();
            if (companyId <= 0) return Json(new { success = false, message = "Valid Company ID required." });

            var res =await _service.UpsertVideoTutorial(req);
            return Json(new { success = res.Success, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVideoTutorial(int id)
        {
            var res =await _service.DeleteVideoTutorial(id);
            return Json(new { success = res.Success, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleVideoStatus(int id)
        {
            var res =await _service.ToggleVideoTutorialStatus(id);
            return Json(new { success = res.Success, message = res.Message });
        }
        #endregion

        #region Upload Content
        public async Task<IActionResult> UploadContent()
        {
            var companyId = await GetCompanyId();
            ViewBag.ContentTypes =(await _service.GetContentTypeList()).Data;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetContentList(string? search)
        {
            var res =await _service.GetContentList(search);
            return Json(new { res.Success, res.Data });
        }

        [HttpPost]
        public async Task<IActionResult> SaveUploadContent(string title, int contentTypeId, string? videoLink, IFormFile? file)
        {
            var companyId = await GetCompanyId();
            var userId = GetUserId();

            string fileType = "Link";
            string fileName = "";
            string filePath = videoLink ?? "";
            string fileSize = "N/A";

            if (file != null)
            {
                fileType = "File";
                fileName = file.FileName;
                fileSize = (file.Length / 1024.0 / 1024.0).ToString("0.##") + " MB";

                var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "download_center");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var path = Path.Combine(uploadDir, uniqueFileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                filePath = "/uploads/download_center/" + uniqueFileName;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                return Json(new { success = false, message = "Please upload a file or provide a video link." });
            }

            var res =await _service.SaveUploadContentAsync(title, contentTypeId, fileType, fileName, filePath, fileSize);
            return Json(new { success = res.Success, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteContent(int id)
        {
            var res =await _service.DeleteContent(id);
            return Json(new { success = res.Success, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> GenerateSharedLink([FromBody] SharedLinkUpsertRequest req)
        {
            var res =await _service.GenerateSharedLink(req);
            return Json(new { success = res.Success, message = res.Message, token = res.Data });
        }

        

        [HttpGet]
        public async Task<IActionResult> GetSharedLinkList()
        {
            var res =await _service.GetSharedLinkList();
            return Json(new { success = true, res.Data });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSharedLink([FromBody] List<int> id)
        {
            var res =await _service.DeleteSharedLink(id);
            return Json(new { success = res.Success, message = res.Message });
        }
        #endregion

        [HttpGet]
        public async Task<IActionResult> GetSections(int classId)
        {
            var sections =(await _sectionService.GetByClassAsync(classId)).Data;
            return Json(sections);
        }

        public IActionResult ContentShareList()
        {
            return View();
        }
    }
}
