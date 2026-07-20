using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Helpers;
using SchoolERP.Net.Services.Clients;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Controllers
{
    public class RoomTypeController : BaseController
    {
        private readonly IHostelClientService _client;
        private readonly ICompanyClientService _companyService;
        private readonly ISessionClientService _sessionService;
        private readonly IRoomCoolingTypeClientService _roomCoolingTypeService;
        public RoomTypeController(IHostelClientService client,
            ICompanyClientService companyService, ISessionClientService sessionService, PermissionHelper permHelper, IRoomCoolingTypeClientService roomCoolingTypeService) : base(permHelper)
        {
            _client = client;
            _companyService = companyService;
            _sessionService = sessionService;
            _roomCoolingTypeService = roomCoolingTypeService;
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
                var response = await _sessionService.GetUserCurrentSessionAsync();
                return response?.Data ?? 0;
            }
            return CurrentSessionId;
        }

        /// <summary>
        /// Shows the 'HostelType' management page where you can define the different grades or Hostel Type in the school.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/RoomType/Index"
               );


                var sessionId = await GetSessionId();
                var companyId = await GetCompanyId();
                var response = await _roomCoolingTypeService.GetAllAsync(companyId, sessionId);

                
                var model = new RoomCoolingTypePageViewModel
                {
                    RoomCoolingType= response.Data,
                    Permissions = perms
                };
                
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
                   "/RoomType/Index"
               );
                var model = new RoomCoolingTypeAddViewModel();
                if (id.HasValue && id.Value > 0)
                {
                    var response = await _roomCoolingTypeService.GetByIDAsync(id.Value);
                    if (response.Success)
                    {
                        model.RoomCoolingType = response.Data;
                        model.EditRoomCoolingType = response.Data;
                    }
                }
                else
                {
                    model.EditRoomCoolingType = null;
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
        public async Task<IActionResult> Save([FromBody] RoomCoolingTypeListDto request)
        {
            try
            {
                var isCreate = request.RoomCoolingTypeId <= 0;
                request.CompanyID = await GetCompanyId();
                request.SessionID = await GetSessionId();
                var response = await _roomCoolingTypeService.UpsertRoomCoolingTypeAsync(request);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] List<int> ids)
        {
            try
            {
                var response = await _roomCoolingTypeService.DeleteAsync(ids);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatusChange([FromBody] StatusUpdateRequest request)
        {
            try
            {
                var response = await _roomCoolingTypeService.ToggleStatusAsync(request);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }


    }
}
