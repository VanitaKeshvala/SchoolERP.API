using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using System.Security.Claims;

namespace SchoolERP.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RoutePickupPointsApiController : ControllerBase
    {
        private readonly IRoutePickupPointService _rppService;
        private readonly ICompanyService _companySvc;
        private readonly ISessionService _sessionSvc;
        private readonly IUserMenuPermissionService _menuPerm;

        private const string MenuPath = "/Transport/RoutePickupPoints";

        public RoutePickupPointsApiController(IRoutePickupPointService rppService, ICompanyService companySvc, ISessionService sessionSvc, IUserMenuPermissionService menuPerm)
        {
            _rppService = rppService;
            _companySvc = companySvc;
            _sessionSvc = sessionSvc;
            _menuPerm = menuPerm;
        }

        private int GetUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("UserId"), out var id) ? id : 0;
        private int GetCompanyId() => _companySvc.GetUserCurrentCompany(GetUserId()) ?? 0;
        private int GetSessionId() => _sessionSvc.GetUserCurrentSession(GetUserId()) ?? 0;

        [HttpGet("GetAllRoutePickupPoints")]
        public IActionResult GetAllRoutePickupPoints()
        {
            try
            {
                var data = _rppService.GetAllRoutePickupPoints(GetCompanyId(), GetSessionId());
                return Ok(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetRoutePickupPointByID/{id}")]
        public IActionResult GetRoutePickupPointByID(int id)
        {
            try
            {
                var data = _rppService.GetRoutePickupPointByID(id);
                if (data == null) return Ok(new { success = false, message = "Record not found" });
                return Ok(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("UpsertRoutePickupPoint")]
        public async Task<IActionResult> UpsertRoutePickupPoint([FromBody] RoutePickupPointUpsertRequest req)
        {
            var isCreate = req.RoutePickupPointID <= 0;
            if (isCreate && !await _menuPerm.Has(User, MenuPath, "Add"))
                return Ok(new { success = false, message = "You do not have permission to add route pickup points." });
            if (!isCreate && !await _menuPerm.Has(User, MenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to edit route pickup points." });

            var res = _rppService.UpsertRoutePickupPoint(req, GetCompanyId(), GetSessionId(), GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        [HttpPost("DeleteRoutePickupPoint")]
        public async Task<IActionResult> DeleteRoutePickupPoint(List<int> id)
        {
            if (!await _menuPerm.Has(User, MenuPath, "Delete"))
                return Ok(new { success = false, message = "You do not have permission to delete route pickup points." });

            var res = _rppService.DeleteRoutePickupPoint(id, GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }

        [HttpPost("ToggleRoutePickupPointStatus")]
        public async Task<IActionResult> ToggleRoutePickupPointStatus(int id, bool isActive)
        {
            if (!await _menuPerm.Has(User, MenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to change status." });

            var res = _rppService.ToggleRoutePickupPointStatus(id, isActive, GetUserId());
            return Ok(new { success = res.Success, message = res.Message });
        }
    }
}
