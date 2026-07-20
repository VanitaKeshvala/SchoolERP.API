using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Helpers;
using SchoolERP.Net.Services.Clients;
using SchoolERP.Shared.Models;
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


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Add()
        {
            return View();
        }

    }
}
