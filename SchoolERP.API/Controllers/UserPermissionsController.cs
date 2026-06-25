using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using System.Data;

namespace SchoolERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserPermissionsController : Controller
    {
        private readonly IUserManagementService _userManagementService;

        public UserPermissionsController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }


        [HttpGet("GetUserPermissions/{userId}")]
        public async Task<IActionResult> GetUserPermissions(int userId)
        {
            var result = await _userManagementService.GetUserPermissionsAsync(userId);
            return Ok(result);
        }
    }


}
