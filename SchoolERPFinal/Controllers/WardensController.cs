using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Helpers;
using SchoolERP.Net.Services.Clients;
using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Controllers
{
    public class WardensController : BaseController
    {
        private readonly ICountryClientService _client;
        private readonly IStateClientService _stateClientService;
        private readonly IWardensClientService _wardensClientService;
        private readonly ICompanyClientService _companyService;
        private readonly ISessionClientService _sessionService;
        private readonly IHostelClientService _hostelClientService;
        private readonly IPhotoUploadService _photoService;
        public WardensController(ICountryClientService client,
            ICompanyClientService companyService,
            ISessionClientService sessionService,
            IStateClientService stateClientService,
            IWardensClientService wardensClientService,
            IHostelClientService hostelClientService,
            IPhotoUploadService photoService,
            PermissionHelper permHelper) : base(permHelper)
        {
            _client = client;
            _companyService = companyService;
            _sessionService = sessionService;
            _stateClientService = stateClientService;
            _wardensClientService= wardensClientService;
            _hostelClientService = hostelClientService;
            _photoService = photoService;
        }
        private async Task<int> GetCompanyId()
        {
            var response = await _companyService.GetUserCurrentCompanyAsync();
            return response?.Data ?? 0;
        }
        private async Task<int> GetSessionId()
        {
            if (CurrentSessionId == null)
            {
                var response = await _sessionService.GetUserCurrentSessionAsync();
                return response?.Data ?? 0;
            }
            return CurrentSessionId;
        }
        /// <summary>
        /// Shows the 'HostelType' management page where you can define the different grades or Hostel Type in the school.
        /// </summary>
        public async Task<IActionResult> Index(int? pageIndex,
        int? pageSize,
        string? search,
        int? companyId,
        int? sessionID,
        int? hostelID)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Wardens/Index"
               );

                var request = new WardensSearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionID = sessionID ?? await GetSessionId(),
                    HostelID = hostelID ?? null
                };

                var sessionId = await GetSessionId();
                var classesResponse = _wardensClientService.GetAllWardensWithPageAsync(request);

                var sessionTask = _sessionService.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();

                await Task.WhenAll(classesResponse, sessionTask, companiesTask);

                var pagedResult = await classesResponse;

                var model = new WardensPageViewModel
                {
                    WardensModel = pagedResult.Success ? pagedResult.Data.Data : new List<WardensModel>(),
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

        public async Task<IActionResult> Add(int? id)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Country/Index"
               );
                var sessionId = await GetSessionId();
                var companyId = await GetCompanyId();
                var model = new WardensAddViewModel();
                var countryRes = await _client.GetAllAsync(companyId,sessionId);
                var stateRes = await _stateClientService.GetAllAsync(companyId,sessionId);
                var hostelres= await _hostelClientService.GetAllHostelsAsync(false,sessionId,companyId);
                if (id.HasValue && id.Value > 0)
                {
                    var response = await _wardensClientService.GetByIDAsync(id.Value);
                    if (response.Success)
                    {
                        model.WardensModel = response.Data;
                        model.EditWardens = response.Data;
                    }
                }
                else
                {
                    model.EditWardens = null;
                }
                model.Country = countryRes.Data;
                model.State = stateRes.Data;
                model.HostelModel = hostelres.Data;
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveWardens([FromBody] WardensRequestModel request)
        {
            try
            {
                var isCreate = request.CompanyID <= 0;
                request.CompanyID = await GetCompanyId();
                request.SessionID = await GetSessionId();
                var response = await _wardensClientService.UpsertWardensAsync(request);
                var WardenId = response.Data.WardenId;
                if (response.Success && WardenId !=null) 
                {
                    PhotoUploadResult photoResult = new PhotoUploadResult();
                    // 3. Only touch the Photo column if it actually changed
                    if (!string.IsNullOrEmpty(request.PhotoBase64) || request.RemovePhoto)
                    {
                        photoResult = await _photoService.SaveBase64PhotoAsync(
                            request.PhotoBase64,
                            request.PhotoFileName ?? "photo.jpg",
                            PhotoModule.Warden,
                            FolderNameModule.Profile,
                            WardenId
                        );
                    }

                    if (photoResult.Success) 
                    {
                        var model = new WardenProfileRequest();
                        model.WardenId = WardenId;
                        model.Photo = photoResult.PhotoUrl;
                        await _wardensClientService.UpdateWardenProfileAsync(model);
                    }
                }

                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }

        [HttpPost]
        public async Task<IActionResult> DeleteWardens([FromBody] List<int> ids)
        {
            try
            {
                var response = await _wardensClientService.DeleteAsync(ids);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }

        [HttpPost]
        public async Task<IActionResult> ToggleWardens([FromBody] WardensRequestModel request)
        {
            try
            {
                var response = await _wardensClientService.ToggleStatusAsync(request);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetStatesByCountry(int countryId)
        {
            try
            {
                var companyId = await GetCompanyId();
                var sessionID = await GetSessionId();
                var response = await _stateClientService.GetAllStateByCountyAsync(companyId, sessionID, countryId);
                return Json(response);
                
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
