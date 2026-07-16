using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Helpers;
using SchoolERP.Net.Services.Clients;
using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Controllers
{
    public class LibrarySupplierController : BaseController
    {
        private readonly ILibrarySupplierClientService _client;
        private readonly ICompanyClientService _companyService;
        private readonly ISessionClientService _sessionService;
        private readonly IPostalCodeClienService _postalCodeService;
        private readonly ICityClientService _cityService;
        private readonly IStateClientService _stateService;
        private readonly ICountryClientService _countryService;
        public LibrarySupplierController(ILibrarySupplierClientService client,
            ICompanyClientService companyService,
            ISessionClientService sessionService,
            IPostalCodeClienService postalCodeService,
            ICityClientService cityClientService,
            IStateClientService stateClientService,
            ICountryClientService countryClientService,
            PermissionHelper permHelper) : base(permHelper)
        {
            _client = client;
            _companyService = companyService;
            _sessionService = sessionService;
            _postalCodeService = postalCodeService;
            _cityService = cityClientService;
            _stateService = stateClientService;
            _countryService = countryClientService;
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
        public async Task<IActionResult> Index(int? pageIndex,
        int? pageSize,
        string? search,
        int? companyId)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/LibraryCategory/Index"
               );

                var request = new SearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId()
                };

                var budgetResponse = _client.GetAllLibrarySupplierWithPageAsync(request);
                var companiesTask = _companyService.GetAllAsync();

                await Task.WhenAll(budgetResponse, companiesTask);

                var pagedResult = await budgetResponse;

                var model = new LibrarySupplierPageViewModel
                {
                    Supplier = pagedResult.Success ? pagedResult.Data.Data : new List<LibrarySupplierModel>(),
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

        public async Task<IActionResult> Add(int? id)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/LibraryCategory/Index"
               );
                var model = new LibrarySupplierAddViewModel();
                var country = await _countryService.GetAllAsync(await GetCompanyId(), await GetSessionId());
                if (id.HasValue && id.Value > 0)
                {
                    var response = await _client.GetByIDAsync(id.Value);
                    if (response.Success)
                    {
                        model.Supplier = response.Data;
                        model.EditSupplier = response.Data;
                    }
                }
                else
                {
                    model.EditSupplier = null;
                }
                model.Country = country.Data;
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveLibrarySupplier([FromBody] LibrarySupplierRequest request)
        {
            try
            {
                var isCreate = request.CompanyID <= 0;
                request.CompanyID = await GetCompanyId();
                var response = await _client.UpsertLibrarySupplierAsync(request);
                return Json(new { success = response.Data.Result, message = response.Data.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }

        [HttpPost]
        public async Task<IActionResult> DeleteLibrarySupplier([FromBody] List<int> ids)
        {
            try
            {
                var response = await _client.DeleteAsync(ids);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }

        [HttpPost]
        public async Task<IActionResult> ToggleLibrarySupplier([FromBody] StatusUpdateRequest request)
        {
            try
            {
                var response = await _client.ToggleStatusAsync(request);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }

    }
}
