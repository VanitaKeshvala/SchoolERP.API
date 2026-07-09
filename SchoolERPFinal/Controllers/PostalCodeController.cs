using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SchoolERP.Net.Helpers;
using SchoolERP.Net.Services.Clients;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using System.ComponentModel.Design;
using System.Data;

namespace SchoolERP.Net.Controllers
{
    public class PostalCodeController : BaseController
    {
        private readonly ICountryClientService _countryClientService;
        private readonly IStateClientService _stateClientService;
        private readonly ICityClientService _cityClientService;
        private readonly IPostalCodeClienService _postalCodeClientService;
        private readonly ICompanyClientService _companyService;
        private readonly ISessionClientService _sessionService;
        public PostalCodeController(ICountryClientService client,
            ICompanyClientService companyService, ISessionClientService sessionService, PermissionHelper permHelper,
            IStateClientService stateClientService,
            ICityClientService cityClientService,
            IPostalCodeClienService postalCodeClienService) : base(permHelper)
        {
            _countryClientService = client;
            _stateClientService = stateClientService;
            _cityClientService = cityClientService;
            _postalCodeClientService = postalCodeClienService;
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
        int? sessionID,
        int? countryId,
        int? stateId,
        int? cityId)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Country/Index"
               );

                var request = new SerachPostalCode
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionID = sessionID ?? await GetSessionId()
                };

                var sessionId = await GetSessionId();
                var classesResponse = _postalCodeClientService.GetAllPostalCodeWithPageAsync(request);

                var sessionTask = _sessionService.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();
                var country=await _countryClientService.GetAllAsync(companyId ?? await GetCompanyId(), sessionId);
                var state=await _stateClientService.GetAllAsync(companyId ?? await GetCompanyId(), sessionId);
                var city = await _cityClientService.GetAllAsync(companyId ?? await GetCompanyId(), sessionId);
                await Task.WhenAll(classesResponse, sessionTask, companiesTask);

                var pagedResult = await classesResponse;

                var model = new PostalCodePageViewModel
                {
                    PostalCode = pagedResult.Success ? pagedResult.Data.Data : new List<PostalCodeListViewModel>(),
                    Companies = (await companiesTask).Data ?? new(),
                    Sessions = (await sessionTask).Data ?? new(),
                    Country = country.Data ?? new(),
                    State = state.Data ?? new(),
                    City = city.Data.Data ?? new(),
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
                var country = await _countryClientService.GetAllAsync(await GetCompanyId(), await GetSessionId());
                var state = await _stateClientService.GetAllAsync( await GetCompanyId(), await GetSessionId());
                var city = await _cityClientService.GetAllAsync(await GetCompanyId(), await GetSessionId());
                var model = new PostalCodeAddViewModel();
                if (id.HasValue && id.Value > 0)
                {
                    var response = await _postalCodeClientService.GetByIDAsync(id.Value);
                    if (response.Success)
                    {
                        model.PostalCode = response.Data;
                        model.EditPostalCode = response.Data;
                    }
                }
                else
                {
                    model.EditPostalCode = null;
                }
                model.Country = country.Data ?? new();
                model.State = state.Data ?? new();
                model.City = city.Data.Data ?? new();
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
        }


        [HttpPost]
        public async Task<IActionResult> SavePostalCode([FromBody] PostalCodeEditModel request)
        {
            try
            {
                var isCreate = request.CompanyID <= 0;
                request.CompanyID = await GetCompanyId();
                request.SessionID = await GetSessionId();
                var response = await _postalCodeClientService.UpsertPostalCodAsync(request);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }

        [HttpPost]
        public async Task<IActionResult> DeletePostalCode([FromBody] List<int> ids)
        {
            try
            {
                var response = await _postalCodeClientService.DeleteAsync(ids);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }

        [HttpPost]
        public async Task<IActionResult> TogglePostalCode([FromBody] StatusUpdateRequest request)
        {
            try
            {
                var response = await _postalCodeClientService.ToggleStatusAsync(request);
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
                var response = await _stateClientService.GetAllStateByCountyAsync(await GetCompanyId(), await GetSessionId(), countryID);
                return Json(new { success = response.Success, data = response.Data });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCitiesByState(int stateID)
        {
            try
            {
                var response = await _cityClientService.GetAllCityByStateIdWisesync(stateID);
                return Json(new { success = response.Success, data = response.Data });
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
