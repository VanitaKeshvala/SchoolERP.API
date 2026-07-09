using Microsoft.AspNetCore.Mvc;
using SchoolERP.Shared.Models;
using SchoolERP.Net.Services.Clients;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Net.Helpers;
using SchoolERP.Shared.Models.Common;
using static System.Collections.Specialized.BitVector32;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.Design;

namespace SchoolERP.Net.Controllers
{
    public class HostelController : BaseController
    {
        private readonly IHostelClientService _client;
        private readonly ICompanyClientService _companyService;
        private readonly ISessionClientService _sessionService;
        private readonly IHostelTypeClientService _hostelTypeService;
        private readonly IRoomCoolingTypeClientService _roomCoolingTypeService;
        private readonly IPostalCodeClienService _postalCodeService;
        private readonly ICityClientService _cityService;
        private readonly IStateClientService _stateService;
        private readonly ICountryClientService _countryService;
        private readonly IRoomCoolingTypeClientService _roomCoolinTypeService;

        public HostelController(IHostelClientService client,
            ICompanyClientService companyService, ISessionClientService sessionService, PermissionHelper permHelper,
            IHostelTypeClientService hostelTypeService, 
            IRoomCoolingTypeClientService roomCoolingTypeService, IPostalCodeClienService postalCodeService,
            ICityClientService cityClientService,
            IStateClientService stateClientService,
            ICountryClientService countryClientService, IRoomCoolingTypeClientService roomCoolinTypeService) : base(permHelper)
        {
            _client = client;
            _companyService = companyService;
            _sessionService = sessionService;
            _hostelTypeService = hostelTypeService;
            _roomCoolingTypeService = roomCoolingTypeService;
            _postalCodeService = postalCodeService;
            _cityService = cityClientService;
            _stateService = stateClientService;
            _countryService = countryClientService;
            _roomCoolinTypeService = roomCoolinTypeService;
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
        /// Displays a list of all available room types so you can see the different options for housing.
        /// </summary>
        public async Task<IActionResult> RoomType(int? pageIndex,
        int? pageSize,
        string? search,
        int? companyId,
        int? sessionID)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Hostel/RoomType"
               );

                var request = new ClassSearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionID = sessionID ?? await GetSessionId()
                };

                
                // Step 1: Ask the system to fetch the full list of room types.
                var roomTypeRes = _client.GetAllRoomTypeWithPageAsync(request);


                var sessionTask = _sessionService.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();

                await Task.WhenAll(roomTypeRes, sessionTask, companiesTask);

                var pagedResult = await roomTypeRes;
                // Step 2: Organize the data found or provide an empty list if nothing was retrieved.
                var model = new RoomTypePageViewModel
                {
                    Items = pagedResult.Success ? pagedResult.Data.Data : new List<RoomTypeViewModel>(),
                    Companies = (await companiesTask).Data ?? new(),
                    Sessions = (await sessionTask).Data ?? new(),
                    TotalRecords = pagedResult.Data.TotalRecords,
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,
                    SearchTerm = search,
                    CompanyId = companyId,
                    SessionId = sessionID
                };
                model.Permissions = perms;
                // Step 3: Send the room type information to the display page.
                return View(model);
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        /// <summary>
        /// Shows the main hostel page, including a list of all hostels and their available room types.
        /// </summary>
        public async Task<IActionResult> Index(int? pageIndex,
        int? pageSize,
        string? search,
        int? roomTypeID,
        int? companyId,
        int? sessionID)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Hostel/Index"
               );

                var request = new HotelSearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    RoomTypeID= roomTypeID ?? 0,
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionID = sessionID ?? await GetSessionId()
                };

                // Step 1: Gather information about all hostels and room types from the system.
                var resHostel =  _client.GetAllHotelWithPageAsync(request);
                var resRoomType = await _client.GetAllRoomTypesAsync(false, sessionID);


                var sessionTask = _sessionService.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();

                await Task.WhenAll(resHostel, sessionTask, companiesTask);

                var pagedResult = await resHostel;

                // Step 2: Prepare the data for the screen, handling cases where data might be missing.
                var model = new HostelPageViewModel
                {
                    Items = pagedResult.Success ? pagedResult.Data.Data : new List<HostelViewModel>(),
                    RoomTypes = resRoomType.Success ? resRoomType.Data : new List<RoomTypeViewModel>(),
                    Companies = (await companiesTask).Data ?? new(),
                    Sessions = (await sessionTask).Data ?? new(),
                    TotalRecords = pagedResult.Data.TotalRecords,
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,
                    SearchTerm = search,
                    CompanyId = companyId,
                    SessionId = sessionID
                };
                model.Permissions = perms;
                // Step 3: Present the dashboard to the user.
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
            
        }



        /// <summary>
        /// Displays details for specific hostel rooms, including which hostel they belong to and their room type.
        /// </summary>
        public async Task<IActionResult> HostelRoom(int? pageIndex,
        int? pageSize,
        string? search,
        int? hostelID,
        int? roomTypeID,
        int? companyId,
        int? sessionID)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Hostel/HostelRoom"
               );

                var request = new HotelSearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    HostelID=hostelID??0,
                    RoomTypeID = roomTypeID ?? 0,
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionID = sessionID ?? await GetSessionId()
                };

                // Step 1: Retrieve all room records, hostel names, and category types.

                // Step 1: Gather information about all hostels and room types from the system.
                var resRoom = _client.GetAllHostelRoomWithPageAsync(request);
                var resRoomType = await _client.GetAllRoomTypesAsync(false, sessionID);
                var resHostel = await _client.GetAllHostelsAsync(false, sessionID);

                var sessionTask = _sessionService.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();

                await Task.WhenAll(resRoom, sessionTask, companiesTask);

                var pagedResult = await resRoom;


                
                
                
                var model = new HostelRoomPageViewModel
                {
                    Items = pagedResult.Success ? pagedResult.Data.Data : new List<HostelRoomViewModel>(),
                    Hostels = resHostel.Success ? resHostel.Data : new List<HostelViewModel>(),
                    RoomTypes = resRoomType.Success ? resRoomType.Data : new List<RoomTypeViewModel>(),
                    Companies = (await companiesTask).Data ?? new(),
                    Sessions = (await sessionTask).Data ?? new(),
                    TotalRecords = pagedResult.Data.TotalRecords,
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,
                    SearchTerm = search,
                    CompanyId = companyId,
                    SessionId = sessionID
                };
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        #region Room Type API Endpoints
        [HttpGet]
        public async Task<IActionResult> GetRoomTypeByID(int id)
        {
            var res = await _client.GetRoomTypeByIDAsync(id);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> UpsertRoomType([FromBody] RoomTypeUpsertRequest req)
        {
            try
            {
                req.SessionID = await GetSessionId();
                req.CompanyID = await GetCompanyId();
                var res = await _client.UpsertRoomTypeAsync(req);
                return Json(new { success = res.Success, message = res.Message });
                
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
          
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRoomType([FromBody] List<int> id)
        {
            var res = await _client.DeleteRoomTypeAsync(id);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleRoomTypeStatus([FromBody] StatusUpdateRequest request)
        {
            try
            {
                var res = await _client.ToggleRoomTypeStatusAsync(request);
                return Json(new { success = res.Success, message = res.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success =false, message = ex.Message });
            }            
        }
        #endregion

        #region Hostel API Endpoints
        [HttpGet]
        public async Task<IActionResult> GetHostelByID(int id)
        {
            var res = await _client.GetHostelByIDAsync(id);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> UpsertHostel([FromBody] HostelUpsertRequest req)
        {
            try
            {
                req.SessionID= await GetSessionId();
                req.CompanyID = await GetCompanyId();
                var res = await _client.UpsertHostelAsync(req);
                return Json(new { success = res.Success, message = res.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> DeleteHostel([FromBody] List<int> id)
        {
            var res = await _client.DeleteHostelAsync(id);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleHostelStatus([FromBody] StatusUpdateRequest request)
        {
            try
            {
                var res = await _client.ToggleHostelStatusAsync(request);
                return Json(new { success = res.Success, message = res.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            
        }
        #endregion

        #region Hostel Room API Endpoints
        [HttpGet]
        public async Task<IActionResult> GetHostelRoomByID(int id)
        {
            var res = await _client.GetHostelRoomByIDAsync(id);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> UpsertHostelRoom([FromBody] HostelRoomUpsertRequest req)
        {
            try
            {
                req.SessionID = await GetSessionId();
                req.CompanyID = await GetCompanyId();
                var res = await _client.UpsertHostelRoomAsync(req);
                return Json(res);
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> DeleteHostelRoom([FromBody] List<int> id)
        {
            var res = await _client.DeleteHostelRoomAsync(id);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleHostelRoomStatus([FromBody] StatusUpdateRequest request)
        {
            var res = await _client.ToggleHostelRoomStatusAsync(request);
            return Json(res);
        }
        #endregion


        public async Task<IActionResult> AddRoomType(int? id)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Academics/Class"
               );
                var sessionId = await GetSessionId();
                var companyId = await GetCompanyId();
                var model = new RoomTypeAddViewModel();

                //RoomCoolingTypeId
                var roomCoolingType = await _roomCoolingTypeService.GetAllAsync(companyId, sessionId);
                if (id.HasValue && id.Value > 0)
                {
                    var response = await _client.GetRoomTypeByIDAsync(id.Value);
                    if (response.Success)
                    {
                        model.Items = response.Data;
                        model.EditRoomType = response.Data;
                    }
                }
                else
                {
                    model.EditRoomType = null;
                }
                model.RoomCoolingType = roomCoolingType.Data;
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IActionResult> AddHostel(int? id)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Academics/Class"
               );
                var sessionId = await GetSessionId();
                var model = new HostelAddViewModel();

                var country = await _countryService.GetAllAsync( await GetCompanyId(), sessionId);
                var state = await _stateService.GetAllAsync( await GetCompanyId(), sessionId);
                var city = await _cityService.GetAllAsync( await GetCompanyId(), sessionId);
                var resRoomType = await _client.GetAllRoomTypesAsync(false, sessionId);
                var resHostelRoom = await _hostelTypeService.GetAllAsync(await GetCompanyId(), sessionId);
                model.HostelTypes = resHostelRoom.Data?? new List<HostelTypeModel>();
                if (id.HasValue && id.Value > 0)
                {
                    var response = await _client.GetHostelByIDAsync(id.Value);
                    if (response.Success)
                    {
                        model.Items = response.Data;
                        model.EditHostel = response.Data;
                    }
                }
                else
                {
                    model.EditHostel = null;
                }
                model.Country = country.Data;
                model.State = state.Data;
                model.City = city.Data.Data;
                model.RoomTypes = resRoomType.Data?? new List<RoomTypeViewModel>();
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IActionResult> AddHostelRoom(int? id)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Academics/Class"
               );
                var sessionId = await GetSessionId();
                var model = new HostelRoomAddViewModel();


                var resHostel = await _client.GetAllHostelsAsync(false, sessionId, await GetCompanyId());
                var resRoomType = await _roomCoolinTypeService.GetAllAsync(await GetCompanyId(),sessionId);
                var resRoomOccupancyType= await _client.GetAllRoomTypesAsync(false, sessionId);
                if (id.HasValue && id.Value > 0)
                {
                    var response = await _client.GetHostelRoomByIDAsync(id.Value);
                    if (response.Success)
                    {
                        model.Items = response.Data;
                        model.EditHostelRoom = response.Data;
                    }
                }
                else
                {
                    model.EditHostelRoom = null;
                }
                model.Hostels = resHostel.Success ? resHostel.Data : new List<HostelViewModel>();
                model.RoomTypes = resRoomType.Success ? resRoomType.Data : new List<RoomCoolingType>();
                model.RoomOccupancyTypes = resRoomOccupancyType.Success ? resRoomOccupancyType.Data : new List<RoomTypeViewModel>();
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetHostelRoomRateByID(int id)
        {
            try
            {
                var res = await _client.GetHostelRoomRateByIDAsync(id);
                return Json(res);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            
        }

        #region Hostel Type Section
        /// <summary>
        /// Shows the 'HostelType' management page where you can define the different grades or Hostel Type in the school.
        /// </summary>
        public async Task<IActionResult> HostelType(int? pageIndex,
        int? pageSize,
        string? search,
        int? companyId,
        int? sessionID)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Hostel/HostelType"
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
                var classesResponse = _hostelTypeService.GetAllHostelTypeWithPageAsync(request);

                var sessionTask = _sessionService.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();

                await Task.WhenAll(classesResponse, sessionTask, companiesTask);

                var pagedResult = await classesResponse;

                var model = new HostelTypePageViewModel
                {
                    HostelType = pagedResult.Success ? pagedResult.Data.Data : new List<HostelTypeModel>(),
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


        public async Task<IActionResult> AddHostelType(int? id)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Hostel/HostelType"
               );
                var model = new HostelTypeAddViewModel();
                if (id.HasValue && id.Value > 0)
                {
                    var response = await _hostelTypeService.GetByIDAsync(id.Value);
                    if (response.Success)
                    {
                        model.HostelType = response.Data;
                        model.EditHostelType = response.Data;
                    }
                }
                else
                {
                    model.EditHostelType = null;
                }

                model.Permissions = perms;
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveHostelType([FromBody] HostelTypeUpsertRequest request)
        {
            try
            {
                var isCreate = request.HostelTypeID <= 0;
                request.CompanyID = await GetCompanyId();
                request.SessionID = await GetSessionId();                
                var response = await _hostelTypeService.UpsertHostelTypeAsync(request);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }

        [HttpPost]
        public async Task<IActionResult> DeleteHostelType([FromBody] List<int> ids)
        {
            try
            {
                var response = await _hostelTypeService.DeleteAsync(ids);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> ToggleHostelType([FromBody] StatusUpdateRequest request)
        {
            try
            {
                var response = await _hostelTypeService.ToggleStatusAsync(request);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            
        }


        [HttpGet]
        public async Task<IActionResult> SearchPostalCode(string term)
        {
            try
            {
                var response = await _postalCodeService.SearchPostalCodeAsync(term);
                return Json(new { success = response.Success, data = response.Data });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoomOccupancy(int roomTypeId)
        {
            try
            {
                var response = await _client.GetAllRoomOccupancyByRoomTypesWiseAsync(roomTypeId);
                return Json(new { success = response.Success, data = response.Data });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetHostelSummary(int hostelId)
        {
            try
            {
                var response = await _client.GetHostelSummary(hostelId);
                return Json(new { success = response.Success, data = response.Data });
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion


        // ------------------------------------------------------------------
        // GET /Hostel/HostelManageReport
        // Initial page render — browser navigation, filters come from
        // query string (or none at all on first load).
        // ------------------------------------------------------------------
        //[HttpGet]
        //public async Task<IActionResult> HostelManageReportokd([FromQuery] HotelReportSearchRequest request)
        //{
        //    request ??= new HotelReportSearchRequest();
        //    request.Mode = string.IsNullOrWhiteSpace(request.Mode) ? "REPORT" : request.Mode.ToUpperInvariant();
        //    request.PageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        //    request.PageSize = request.PageSize <= 0 ? 10 : request.PageSize;
        //    request.CompanyID = request.CompanyID > 0 ? request.CompanyID : await GetCompanyId();
        //    request.SessionID = request.SessionID > 0 ? request.SessionID : await GetSessionId();

        //    var perms = await GetPermissions("/Academics/Class");
        //    var sessionTask = await _sessionService.GetAllAsync();
        //    var companiesTask = await _companyService.GetAllAsync();
        //    var hostel = await _client.GetAllHostelsAsync(false, request.SessionID, request.CompanyID);
        //    var hostelTypes = await _hostelTypeService.GetAllAsync(request.CompanyID, request.SessionID);
        //    var roomTypes = await _client.GetAllRoomTypesAsync(false, request.SessionID, request.CompanyID);
        //    var roomCoolingTypes = await _roomCoolingTypeService.GetAllAsync(request.CompanyID, request.SessionID);
        //    var data = _client.GetAllHostelReportWithPageAsync(request);

        //    await Task.WhenAll(data);

        //    var pagedResult = await data;

        //    var vm = new HostelManageReportPageViewModel
        //    {
        //        Items = pagedResult.Success ? pagedResult.Data.Data : new List<HostelReportResponse>(),
        //        Permissions = perms,
        //        Companies = companiesTask.Data,
        //        Sessions = sessionTask.Data,
        //        Hostels = hostel.Data,
        //        HostelTypes = hostelTypes.Data,
        //        RoomTypes = roomTypes.Data,
        //        RoomCoolingTypes = roomCoolingTypes.Data,
        //        SearchRequest = request,
        //        TotalRecords = pagedResult.Data.TotalRecords
        //        //PageNumber = pagedResult.Data.PageNumber,
        //        //PageSize = pagedResult.Data.PageSize
        //    };

            

        //    return View(vm); // looks for Views/Hostel/HostelManageReport.cshtml
        //}


        // ------------------------------------------------------------------
        // GET /Hostel/HostelManage
        // Initial page render — dropdown data + first page of grid data.
        // ------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> HostelManageReport(HotelReportSearchRequest request)
        {
            request ??= new HotelReportSearchRequest();
            request.Mode = string.IsNullOrWhiteSpace(request.Mode) ? "REPORT" : request.Mode.ToUpperInvariant();
            request.PageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            request.PageSize = request.PageSize <= 0 ? 10 : request.PageSize;
            request.CompanyID = request.CompanyID > 0 ? request.CompanyID : await GetCompanyId();
            request.SessionID = request.SessionID > 0 ? request.SessionID : await GetSessionId();
            var perms = await GetPermissions("/Academics/Class");
            var sessionTask = await _sessionService.GetAllAsync();
            var companiesTask = await _companyService.GetAllAsync();
            var hostel = await _client.GetAllHostelsAsync(false, request.SessionID, request.CompanyID);
            var hostelTypes = await _hostelTypeService.GetAllAsync(request.CompanyID, request.SessionID);
            var roomTypes = await _client.GetAllRoomTypesAsync(false, request.SessionID, request.CompanyID);
            var roomCoolingTypes = await _roomCoolingTypeService.GetAllAsync(request.CompanyID, request.SessionID);
            var data = await _client.GetAllHostelReportWithPageAsync(request);
            var vm = new HostelManageReportPageViewModel
            {
                Permissions = perms,
                Companies = companiesTask.Data,
                Sessions = sessionTask.Data,
                Hostels = hostel.Data,
                HostelTypes = hostelTypes.Data,
                RoomTypes = roomTypes.Data,
                RoomCoolingTypes = roomCoolingTypes.Data,
                SearchRequest = request,

                // NOTE: adjust the property name below (TOTALRECORDS / TotalRecords / whatever
                // your GetAllHostelReportWithPageAsync row DTO actually exposes) to match your
                // real model. This is the piece that was commented out before, and the view's
                // pagination math (Model.TotalRecords / Model.PageSize) depends on it.
                TotalRecords = data.Data?.FirstOrDefault()?.TOTALRECORDS ?? 0,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                CompanyId=request.CompanyID,
                SessionId=request.SessionID
            };

            
            vm.Items = data.Data;
            //vm.TotalRecords = data.Data.FirstOrDefault()?.TOTALRECORDS ?? 0;

            return View(vm);
        }

        // ------------------------------------------------------------------
        // POST /Hostel/GetHostelManageReport
        // AJAX grid data source — called on filter/search/mode/page change.
        // ------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> GetHostelManageReport([FromBody] HotelReportSearchRequest request)
        {
            try
            {
                if (request == null)
                    return Json(new { success = false, message = "Invalid request." });

                request.Mode = string.IsNullOrWhiteSpace(request.Mode) ? "REPORT" : request.Mode.ToUpperInvariant();
                request.PageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
                request.PageSize = request.PageSize <= 0 ? 10 : request.PageSize;
                request.CompanyID = request.CompanyID > 0 ? request.CompanyID : await GetCompanyId();
                request.SessionID = request.SessionID > 0 ? request.SessionID : await GetSessionId();

                var apiResult = await _client.GetAllHostelReportWithPageAsync(request);

                // Unwrap defensively — never let a null link in the chain reach JSON serialization.
                var rows = apiResult?.Data ?? new List<HostelReportResponse>();
                //var totalRecords = rows.FirstOrDefault()?.TOTALRECORDS ?? 0;
                //var totalPages = rows.FirstOrDefault()?.TotalPages
                //                  ?? (request.PageSize > 0 ? (int)Math.Ceiling((double)totalRecords / request.PageSize) : 0);

                return Json(new
                {
                    success = true,
                    mode = request.Mode,
                    data = rows,
                    //totalRecords,
                    //totalPages,
                    pageNumber = request.PageNumber,
                    pageSize = request.PageSize
                });
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error loading hostel manage report");
                return Json(new { success = false, message = "Failed to load report data." });
            }
        }


    }
}
