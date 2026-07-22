using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Helpers;
using SchoolERP.Net.Services.Clients;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using System.Text.Json;

namespace SchoolERP.Net.Controllers
{
    public class MediaController : BaseController
    {
        private readonly IMediaClientService _client;
        private readonly IConfiguration _configuration;
        private readonly IPhotoUploadService _photoService;
        private readonly IWebHostEnvironment _environment;
        private readonly ICompanyClientService _companyService;
        private readonly ISessionClientService _sessionClient;
        public MediaController(
            IMediaClientService client,
            IConfiguration configuration,
            IPhotoUploadService photoService,
            IWebHostEnvironment environment,
            ICompanyClientService companyService,
            ISessionClientService sessionClient, PermissionHelper permHelper) : base(permHelper)
        {
            _client = client;
            _configuration = configuration;
            _photoService = photoService;
            _environment = environment;
            _companyService = companyService;
            _sessionClient = sessionClient;
        }
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
            if (CurrentSessionId == null)
            {
                var response = await _sessionClient.GetUserCurrentSessionAsync();
                return response?.Data ?? 0;
            }
            return CurrentSessionId;
        }
        private int? GetStaffID()
        {
            var staffClaim = User.FindFirst("StaffID")?.Value;
            return int.TryParse(staffClaim, out var staffId) ? staffId : null;
        }


        public async Task<IActionResult> Index(int? pageIndex,
        int? pageSize,
        string? search,
        int? companyId,
        int? sessionID)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Homework/Index"
               );

                var request = new SearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionID = sessionID ?? await GetSessionId()                  
                };

                

                var sessionId = await GetSessionId();
                var classesResponse = _client.GetAllMediaWithPageAsync(request);

                var sessionTask = _sessionClient.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();

                await Task.WhenAll(classesResponse, sessionTask, companiesTask);

                var pagedResult = await classesResponse;

                var model = new MediaPageViewModel
                {
                    Media = pagedResult.Success ? pagedResult.Data.Data : new List<MediaViewModel>(),
                    Companies = (await companiesTask).Data ?? new(),
                    Sessions = (await sessionTask).Data ?? new(),
                    TotalRecords = pagedResult.Data.TotalRecords,
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,
                    SearchTerm = search,
                    CompanyId = companyId,
                    SessionId = sessionId
                };
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IActionResult Add()
        {
            var model = new MediaAddViewModel();
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> GetMediaLibrary(string searchTerm, int? pageIndex = 1, int? pageSize = 5)
        {
            try
            {
                int? companyId = await GetCompanyId();
                int? sessionId = await GetSessionId();
                var request = new SearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = searchTerm,
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionID = sessionId ?? await GetSessionId()
                };

                var res = await _client.GetAllMediaWithPageAsync(request);
                return Json(res); // { result, message, totalRecords, data: [...] }
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> UpsertMedia([FromForm] MediaRequest request, List<IFormFile>? attachmentFiles)
        {
            request.CompanyID = await GetCompanyId();
            request.SessionID = await GetSessionId();

            var attachmentRows = new List<AttachmentDto>();

            // keep whatever the user didn't remove from the existing list
            if (!string.IsNullOrWhiteSpace(request.ExistingAttachmentsJson))
            {
                try
                {
                    var existing = JsonSerializer.Deserialize<List<AttachmentDto>>(request.ExistingAttachmentsJson);
                    if (existing != null) attachmentRows.AddRange(existing);
                }
                catch (JsonException) { /* ignore malformed payload, treat as empty */ }
            }

            if (attachmentFiles != null && attachmentFiles.Count > 0)
            {
                foreach (var file in attachmentFiles)
                {
                    if (file == null || file.Length == 0) continue;

                    using var memoryStream = new MemoryStream();
                    await file.CopyToAsync(memoryStream);

                    var photoResult = await _photoService.SaveBase64PhotoAsync(
                        Convert.ToBase64String(memoryStream.ToArray()),
                        file.FileName,
                        PhotoModule.Homework,
                        FolderNameModule.Documents,
                        request.CompanyID
                    );

                    attachmentRows.Add(new AttachmentDto
                    {
                        Attachment = photoResult.PhotoUrl,
                        FileName = photoResult.FileName,
                        FileType = file.ContentType
                    });
                }
            }

            request.MediaJson = JsonSerializer.Serialize(attachmentRows);

            var res = await _client.UpsertAsync(request);
            return Json(res);
        }
    }
}
