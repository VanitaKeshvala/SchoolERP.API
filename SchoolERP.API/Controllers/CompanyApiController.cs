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
    /// <summary>
    /// This controller provides the technical endpoints for managing school companies and branches through the API.
    /// </summary>
    public class CompanyApiController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly IUserMenuPermissionService _menuPerm;

        private const string MenuPath = "/Settings";

        public CompanyApiController(ICompanyService companyService, IUserMenuPermissionService menuPerm)
        {
            _companyService = companyService;
            _menuPerm = menuPerm;
        }

        /// <summary>
        /// Gets the full list of all registered school companies from the system.
        /// </summary>
        [HttpGet("GetAll")]
        public IActionResult GetAll(bool includeDeleted = false)
        {
            var data = _companyService.GetAllCompanies(includeDeleted);
            return Ok(ApiResponse<List<MstCompanyViewModel>>.SuccessResponse(data));
        }

        /// <summary>
        /// Gets the list of companies assigned to the currently logged-in user.
        /// </summary>
        [HttpGet("GetAssignedCompanies")]
        public IActionResult GetAssignedCompanies()
        {
            if (IsSuperAdmin())
            {
                var allData = _companyService.GetAllCompanies(false);
                return Ok(ApiResponse<List<MstCompanyViewModel>>.SuccessResponse(allData));
            }

            int userId = GetCurrentUserId();
            var data = _companyService.GetCompaniesByUserId(userId);
            return Ok(ApiResponse<List<MstCompanyViewModel>>.SuccessResponse(data));
        }

        /// <summary>
        /// Gets the details of one specific school company using its unique ID number.
        /// </summary>
        [HttpGet("GetByID/{id}")]
        public IActionResult GetByID(int id)
        {
            var data = _companyService.GetCompanyByID(id);
            if (data == null) return NotFound(ApiResponse<MstCompanyViewModel>.ErrorResponse("Company not found"));
            return Ok(ApiResponse<MstCompanyViewModel>.SuccessResponse(data));
        }

        /// <summary>
        /// Creates a new school company or updates an existing one with the information provided.
        /// </summary>
        [HttpPost("Upsert")]
        public async Task<IActionResult> Upsert([FromBody] MstCompanyUpsertRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            int userId = GetCurrentUserId();

            var isCreate = request.CompanyId <= 0;
            if (isCreate && !await _menuPerm.Has(User, MenuPath, "Add"))
                return Ok(new { success = false, message = "You do not have permission to add companies." });
            if (!isCreate && !await _menuPerm.Has(User, MenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to edit companies." });

            var (success, message) = _companyService.UpsertCompany(request, userId);
            return Ok(new { success, message });
        }

        /// <summary>
        /// Permanently removes a school company's record from the system.
        /// </summary>
        [HttpPost("Delete")]
        public async Task<IActionResult> Delete(List<int> id)
        {
            if (!await _menuPerm.Has(User, MenuPath, "Delete"))
                return Ok(new { success = false, message = "You do not have permission to delete companies." });

            int userId = GetCurrentUserId();
            var (success, message) = _companyService.DeleteCompany(id, userId);
            return Ok(new { success, message });
        }

        /// <summary>
        /// Turns a school company's active status on or off.
        /// </summary>
        [HttpPost("ToggleStatus")]
        public async Task<IActionResult> ToggleStatus(int id, bool isActive)
        {
            if (!await _menuPerm.Has(User, MenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to change status." });

            int userId = GetCurrentUserId();
            var (success, message) = _companyService.ToggleStatus(id, isActive, userId);
            return Ok(new { success, message });
        }

        /// <summary>
        /// Sets the chosen school company as the currently 'active' one for the user in the database.
        /// </summary>
        [HttpPost("SetCurrent")]
        public IActionResult SetCurrent([FromBody] SetCurrentCompanyRequest request)
        {
            int userId = GetCurrentUserId();
            var (success, message) = _companyService.UpdateUserCurrentCompany(userId, request.CompanyId);
            if (!success) return BadRequest(ApiResponse<dynamic>.ErrorResponse(message));
            return Ok(ApiResponse<dynamic>.SuccessResponse(null, message));
        }

        /// <summary>
        /// Gets the ID of the school company that the user is currently working in.
        /// </summary>
        [HttpGet("GetUserCurrentCompany")]
        public IActionResult GetUserCurrentCompany()
        {
            int userId = GetCurrentUserId();
            var companyId = _companyService.GetUserCurrentCompany(userId);
            return Ok(ApiResponse<int?>.SuccessResponse(companyId));
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        private bool IsSuperAdmin()
        {
            if (User?.Identity?.IsAuthenticated != true) return false;

            return User.Claims.Any(c =>
                (string.Equals(c.Type, ClaimTypes.Role, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(c.Type, "Role", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(c.Type, "UserTypeName", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(c.Type, "UserType", StringComparison.OrdinalIgnoreCase)) &&
                IsSuperAdminValue(c.Value));
        }

        private static bool IsSuperAdminValue(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            var normalized = value.Replace(" ", string.Empty)
                .Replace("-", string.Empty)
                .Replace("_", string.Empty)
                .Trim();
            return string.Equals(normalized, "superadmin", StringComparison.OrdinalIgnoreCase);
        }
    }
}
