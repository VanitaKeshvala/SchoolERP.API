using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Helpers;
using SchoolERP.API.Interfaces;
using SchoolERP.API.Models;
using SchoolERP.API.Models.Common;

namespace SchoolERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MenuApiController : Controller
    {
        private readonly IMenuService _menuService;
        private readonly IUserManagementService _userManagementService;
        private readonly PermissionHelper _permHelper;
        public MenuApiController(IMenuService menuService, IUserManagementService userManagementService, PermissionHelper permHelper)
        {
            _menuService = menuService;
            _userManagementService = userManagementService;
            _permHelper = permHelper;
        }

        /// <summary>
        /// Gets the full list of all registered school companies from the system.
        /// </summary>
        [HttpGet("GetAllMenus")]
        public async Task<IActionResult> GetAllMenus()
        {
            var data =await _menuService.GetAllMenus();
            return Ok(ApiResponse<List<MenuViewModel>>.SuccessResponse(data));
        }

        /// <summary>
        /// Retrieves user permissions based on the specified user ID.
        /// Optionally filters permissions by menu URL prefix and permission name.
        /// Returns a list of permissions assigned to the user.
        /// </summary>
        /// <returns>
        /// Returns a list of user permissions including menu access and assigned permission details.
        /// </returns>
        [HttpPost("GetUserPermissions")]
        public async Task<IActionResult> GetUserPermissions([FromQuery] int userId, [FromQuery] string? menuUrlPrefix = null, [FromQuery] string? permissionName = null)
        {
            var data = await _userManagementService.GetUserPermissions(userId, menuUrlPrefix, permissionName);
            return Ok(ApiResponse<List<UserPermissionViewModel>>.SuccessResponse(data));
        }

    }
}
