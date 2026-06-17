using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Interfaces;
using SchoolERP.API.Models;
using SchoolERP.API.Models.Common;
using System.Security.Claims;

namespace SchoolERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HostelApiController : ControllerBase
    {
        private readonly IHostelService _hostelService;
        private readonly ICompanyService _companySvc;
        private readonly ISessionService _sessionSvc;
        private readonly IUserMenuPermissionService _menuPerm;

        private const string RoomTypeMenuPath = "/Hostel/RoomType";
        private const string HostelMenuPath = "/Hostel";
        private const string HostelRoomMenuPath = "/Hostel/HostelRoom";

        public HostelApiController(IHostelService hostelService, ICompanyService companySvc, ISessionService sessionSvc, IUserMenuPermissionService menuPerm)
        {
            _hostelService = hostelService;
            _companySvc = companySvc;
            _sessionSvc = sessionSvc;
            _menuPerm = menuPerm;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "1");
        private int CompanyId => _companySvc.GetUserCurrentCompany(UserId) ?? 0;
        private int SessionId => _sessionSvc.GetUserCurrentSession(UserId) ?? 0;

        [HttpGet("GetAllRoomTypes")]
        public IActionResult GetAllRoomTypes(bool includeDeleted = false)
        {
            try
            {
                var data = _hostelService.GetAllRoomTypes(CompanyId, SessionId, includeDeleted);
                return Ok(ApiResponse<List<RoomTypeViewModel>>.SuccessResponse(data));
            }
            catch (System.Exception ex)
            {
                return Ok(ApiResponse<List<RoomTypeViewModel>>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("GetRoomTypeByID/{id}")]
        public IActionResult GetRoomTypeByID(int id)
        {
            try
            {
                var data = _hostelService.GetRoomTypeByID(id);
                if (data == null) return NotFound(ApiResponse<RoomTypeViewModel>.ErrorResponse("Not found"));
                return Ok(ApiResponse<RoomTypeViewModel>.SuccessResponse(data));
            }
            catch (System.Exception ex)
            {
                return Ok(ApiResponse<RoomTypeViewModel>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("UpsertRoomType")]
        public async Task<IActionResult> UpsertRoomType([FromBody] RoomTypeUpsertRequest req)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var isCreate = req.RoomTypeID <= 0;
                if (isCreate && !await _menuPerm.Has(User, RoomTypeMenuPath, "Add"))
                    return Ok(new { success = false, message = "You do not have permission to add room types." });
                if (!isCreate && !await _menuPerm.Has(User, RoomTypeMenuPath, "Edit"))
                    return Ok(new { success = false, message = "You do not have permission to edit room types." });

                var result = _hostelService.UpsertRoomType(req, CompanyId, SessionId, UserId);
                return Ok(new { success = result.Success, message = result.Message });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("DeleteRoomType")]
        public async Task<IActionResult> DeleteRoomType(List<int> id)
        {
            try
            {
                if (!await _menuPerm.Has(User, RoomTypeMenuPath, "Delete"))
                    return Ok(new { success = false, message = "You do not have permission to delete room types." });

                var result = _hostelService.DeleteRoomType(id, UserId);
                return Ok(new { success = true, message = result.Message });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("ToggleRoomTypeStatus")]
        public async Task<IActionResult> ToggleRoomTypeStatus(int id, bool isActive)
        {
            try
            {
                if (!await _menuPerm.Has(User, RoomTypeMenuPath, "Edit"))
                    return Ok(new { success = false, message = "You do not have permission to change room type status." });

                var result = _hostelService.ToggleRoomTypeStatus(id, isActive, UserId);
                return Ok(new { success = result.Success, message = result.Message });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        // ─── HOSTEL ─────────────────────────────────────────────
        [HttpGet("GetAllHostels")]
        public IActionResult GetAllHostels(bool includeDeleted = false)
        {
            try
            {
                var data = _hostelService.GetAllHostels(CompanyId, SessionId, includeDeleted);
                return Ok(ApiResponse<List<HostelViewModel>>.SuccessResponse(data));
            }
            catch (System.Exception ex)
            {
                return Ok(ApiResponse<List<HostelViewModel>>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("GetHostelByID/{id}")]
        public IActionResult GetHostelByID(int id)
        {
            try
            {
                var data = _hostelService.GetHostelByID(id);
                if (data == null) return NotFound(ApiResponse<HostelViewModel>.ErrorResponse("Not found"));
                return Ok(ApiResponse<HostelViewModel>.SuccessResponse(data));
            }
            catch (System.Exception ex)
            {
                return Ok(ApiResponse<HostelViewModel>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("UpsertHostel")]
        public async Task<IActionResult> UpsertHostel([FromBody] HostelUpsertRequest req)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var isCreate = req.HostelID <= 0;
                if (isCreate && !await _menuPerm.Has(User, HostelMenuPath, "Add"))
                    return Ok(new { success = false, message = "You do not have permission to add hostels." });
                if (!isCreate && !await _menuPerm.Has(User, HostelMenuPath, "Edit"))
                    return Ok(new { success = false, message = "You do not have permission to edit hostels." });

                var result = _hostelService.UpsertHostel(req, CompanyId, SessionId, UserId);
                return Ok(new { success = result.Success, message = result.Message });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("DeleteHostel")]
        public async Task<IActionResult> DeleteHostel(List<int> id)
        {
            try
            {
                if (!await _menuPerm.Has(User, HostelMenuPath, "Delete"))
                    return Ok(new { success = false, message = "You do not have permission to delete hostels." });

                var result = _hostelService.DeleteHostel(id, UserId);
                return Ok(new { success = result.Success, message = result.Message });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("ToggleHostelStatus")]
        public async Task<IActionResult> ToggleHostelStatus(int id, bool isActive)
        {
            try
            {
                if (!await _menuPerm.Has(User, HostelMenuPath, "Edit"))
                    return Ok(new { success = false, message = "You do not have permission to change hostel status." });

                var result = _hostelService.ToggleHostelStatus(id, isActive, UserId);
                return Ok(new { success = result.Success, message = result.Message });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        // ─── HOSTEL ROOM ────────────────────────────────────────
        [HttpGet("GetAllHostelRooms")]
        public IActionResult GetAllHostelRooms(bool includeDeleted = false)
        {
            try
            {
                var data = _hostelService.GetAllHostelRooms(CompanyId, SessionId, includeDeleted);
                return Ok(ApiResponse<List<HostelRoomViewModel>>.SuccessResponse(data));
            }
            catch (System.Exception ex)
            {
                return Ok(ApiResponse<List<HostelRoomViewModel>>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("GetHostelRoomByID/{id}")]
        public IActionResult GetHostelRoomByID(int id)
        {
            try
            {
                var data = _hostelService.GetHostelRoomByID(id);
                if (data == null) return NotFound(ApiResponse<HostelRoomViewModel>.ErrorResponse("Not found"));
                return Ok(ApiResponse<HostelRoomViewModel>.SuccessResponse(data));
            }
            catch (System.Exception ex)
            {
                return Ok(ApiResponse<HostelRoomViewModel>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("UpsertHostelRoom")]
        public async Task<IActionResult> UpsertHostelRoom([FromBody] HostelRoomUpsertRequest req)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var isCreate = req.RoomId <= 0;
                if (isCreate && !await _menuPerm.Has(User, HostelRoomMenuPath, "Add"))
                    return Ok(new { success = false, message = "You do not have permission to add hostel rooms." });
                if (!isCreate && !await _menuPerm.Has(User, HostelRoomMenuPath, "Edit"))
                    return Ok(new { success = false, message = "You do not have permission to edit hostel rooms." });

                var result = _hostelService.UpsertHostelRoom(req, CompanyId, SessionId, UserId);
                return Ok(new { success = result.Success, message = result.Message });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("DeleteHostelRoom")]
        public async Task<IActionResult> DeleteHostelRoom(List<int> id)
        {
            try
            {
                if (!await _menuPerm.Has(User, HostelRoomMenuPath, "Delete"))
                    return Ok(new { success = false, message = "You do not have permission to delete hostel rooms." });

                var result = _hostelService.DeleteHostelRoom(id, UserId);
                return Ok(new { success = result.Success, message = result.Message });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("ToggleHostelRoomStatus")]
        public async Task<IActionResult> ToggleHostelRoomStatus(int id, bool isActive)
        {
            try
            {
                if (!await _menuPerm.Has(User, HostelRoomMenuPath, "Edit"))
                    return Ok(new { success = false, message = "You do not have permission to change hostel room status." });

                var result = _hostelService.ToggleHostelRoomStatus(id, isActive, UserId);
                return Ok(new { success = result.Success, message = result.Message });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }
    }
}
