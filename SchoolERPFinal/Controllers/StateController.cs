using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Helpers;
using SchoolERP.Net.Services.Clients;
using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Controllers
{
    public class StateController : BaseController
    {
        private readonly ICountryClientService _client;
        private readonly IStateClientService _stateclientService;
        private readonly ICompanyClientService _companyService;
        private readonly ISessionClientService _sessionService;
        public StateController(ICountryClientService client,
            ICompanyClientService companyService, ISessionClientService sessionService, IStateClientService stateclientService, PermissionHelper permHelper) : base(permHelper)
        {
            _client = client;
            _stateclientService = stateclientService;
            _companyService = companyService;
            _sessionService = sessionService;
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
        int? sessionID)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Country/Index"
               );

                var request = new HostelTypeSearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionID = sessionID ?? await GetSessionId()
                };

                var sessionId = await GetSessionId();
                var classesResponse = _stateclientService.GetAllStateWithPageAsync(request);

                var sessionTask = _sessionService.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();

                await Task.WhenAll(classesResponse, sessionTask, companiesTask);

                var pagedResult = await classesResponse;

                var model = new StatePageViewModel
                {
                    State = pagedResult.Success ? pagedResult.Data.Data : new List<StateModel>(),
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
                var companyId = await GetCompanyId();
                var sessionId = await GetSessionId();
                var model = new StateAddViewModel();
                var countryModel = await _client.GetAllAsync(companyId,sessionId);
                if (id.HasValue && id.Value > 0)
                {
                    var response = await _stateclientService.GetByIDAsync(id.Value);
                    if (response.Success)
                    {
                        model.State = response.Data;
                        model.EditState = response.Data;
                    }
                }
                else
                {
                    model.EditState = null;
                }
                model.Country = countryModel.Data;
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveState([FromBody] StateRequestModel request)
        {
            try
            {
                var isCreate = request.CompanyID <= 0;
                request.CompanyID = await GetCompanyId();
                request.SessionID = await GetSessionId();
                var response = await _stateclientService.UpsertStateAsync(request);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }

        [HttpPost]
        public async Task<IActionResult> DeleteState([FromBody] List<int> ids)
        {
            try
            {
                var response = await _stateclientService.DeleteAsync(ids);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }

        [HttpPost]
        public async Task<IActionResult> ToggleState([FromBody] StatusUpdateRequest request)
        {
            try
            {
                var response = await _stateclientService.ToggleStatusAsync(request);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }

    }
}
