using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Helpers;
using SchoolERP.Net.Services.Clients;
using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;
using Microsoft.CodeAnalysis.CSharp;

namespace SchoolERP.Net.Controllers
{
    public class CityController : BaseController
    {
        private readonly ICountryClientService _countryService;
        private readonly IStateClientService _stateService;
        private readonly ICityClientService _cityService;
        private readonly ICompanyClientService _companyService;
        private readonly ISessionClientService _sessionService;
        public CityController(ICountryClientService countryService,
            ICompanyClientService companyService, ISessionClientService sessionService,
            IStateClientService stateService,
            ICityClientService cityService,
            PermissionHelper permHelper) : base(permHelper)
        {
            _countryService = countryService;
            _companyService = companyService;
            _sessionService = sessionService;
            _stateService = stateService;
            _cityService = cityService;
            
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
        int? countryId,
        int? stateId)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Country/Index"
               );

                var request = new CitySearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CountryId=countryId??0,
                    StateId= stateId??0,
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionID = sessionID ?? await GetSessionId()
                };

                var sessionId = await GetSessionId();
                var cityResponse = _cityService.GetAllCityWithPageAsync(request);

                var sessionTask = _sessionService.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();
                var country =await _countryService.GetAllAsync(await GetCompanyId(), sessionId);
                var state =await _stateService.GetAllAsync(await GetCompanyId(), sessionId);
                await Task.WhenAll(cityResponse, sessionTask, companiesTask);

                var pagedResult = await cityResponse;

                var model = new CityPageViewModel
                {
                    City = pagedResult.Success ? pagedResult.Data.Data : new List<CityModel>(),
                    Country=country.Data??new(),
                    State=state.Data??new(),
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
                   "/City/Index"
               );
                var model = new CityAddViewModel();
                var country = await _countryService.GetAllAsync(await GetCompanyId(), await GetSessionId());
                var state = await _stateService.GetAllAsync(await GetCompanyId(), await GetSessionId());
                if (id.HasValue && id.Value > 0)
                {
                    var response = await _cityService.GetByIDAsync(id.Value);
                    if (response.Success)
                    {
                        model.City = response.Data;
                        model.EditCity = response.Data;
                    }
                }
                else
                {
                    model.EditCity = null;
                }
                model.Country = country.Data;
                model.State = state.Data;
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveCity([FromBody] CityUpsertRequest request)
        {
            try
            {
                var isCreate = request.CompanyID <= 0;
                request.CompanyID = await GetCompanyId();
                request.SessionID = await GetSessionId();
                var response = await _cityService.UpsertCityAsync(request);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }

        [HttpPost]
        public async Task<IActionResult> DeleteCity([FromBody] List<int> ids)
        {
            try
            {
                var response = await _cityService.DeleteAsync(ids);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }

        [HttpPost]
        public async Task<IActionResult> ToggleCity([FromBody] StatusUpdateRequest request)
        {
            try
            {
                var response = await _cityService.ToggleStatusAsync(request);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetStatesByCountry(int countryID) 
        {
            try
            {
                var response = await _stateService.GetAllStateByCountyAsync(await GetCompanyId(),await GetSessionId(), countryID);
                return Json(new { success = response.Success, data = response.Data });
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
