using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Interfaces;
using SchoolERP.API.Models;
using SchoolERP.API.Models.Common;
using SchoolERP.API.Services;
using System.Collections.Generic;

namespace SchoolERP.API.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    /// <summary>
    /// This controller provides the technical endpoints for managing system translations and localization settings through the API.
    /// </summary>
    public class SettingsApiController : ControllerBase
    {
        private readonly ILocalizationService _localizationService;
        private readonly IUserManagementService _userMgmtService;
        private readonly IFieldService _fieldService;
        private readonly ICompanyService _companyService;
        private readonly ISessionService _sessionService;
        private readonly IUserMenuPermissionService _menuPerm;

        private const string MenuPath = "/Settings";

        public SettingsApiController(
            ILocalizationService localizationService, 
            IUserManagementService userMgmtService, 
            IFieldService fieldService,
            ICompanyService companyService,
            ISessionService sessionService,
            IUserMenuPermissionService menuPerm)
        {
            _localizationService = localizationService;
            _userMgmtService = userMgmtService;
            _fieldService = fieldService;
            _companyService = companyService;
            _sessionService = sessionService;
            _menuPerm = menuPerm;
        }

        private int UserId
        {
            get
            {
                var idClaim = User.FindFirst("userId") ?? User.FindFirst("UserId") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                return int.TryParse(idClaim?.Value, out int id) ? id : 1;
            }
        }
        private int CompanyId => _companyService.GetUserCurrentCompany(UserId) ?? 0;
        private int SessionId => _sessionService.GetUserCurrentSession(UserId) ?? 0;

        /// <summary>
        /// Gets all the translated words and phrases for a specific language (like English or Hindi).
        /// </summary>
        [HttpGet("translations/{language}")]
        public IActionResult GetTranslations(string language)
        {
            var translations = _localizationService.GetTranslations(language);
            return Ok(ApiResponse<Dictionary<string, string>>.SuccessResponse(translations));
        }

        /// <summary>
        /// Changes the translation for one specific word or phrase in a chosen language.
        /// </summary>
        [HttpPost("translations/update")]
        public async Task<IActionResult> UpdateTranslation([FromBody] TranslationUpdateModel model)
        {
            if (!await _menuPerm.Has(User, MenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to update translations." });

            try
            {
                var translations = _localizationService.GetTranslations(model.Language);

                // Ensure key exists before modifying to prevent uncontrolled registry bloating
                if (translations.ContainsKey(model.Key))
                {
                    translations[model.Key] = model.Value;
                    _localizationService.SaveTranslations(model.Language, translations);
                    return Ok(ApiResponse<bool>.SuccessResponse(true));
                }
                return NotFound(ApiResponse<bool>.ErrorResponse("Key not found"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("fields")]
        public IActionResult GetAllFields(bool? isSystemField = null, string belongsTo = null)
        {
            var fields = _fieldService.GetAllFields(CompanyId, SessionId, isSystemField, belongsTo);
            return Ok(ApiResponse<List<FieldModel>>.SuccessResponse(fields));
        }

        [HttpGet("fields/{id}")]
        public IActionResult GetFieldByID(int id)
        {
            var field = _fieldService.GetFieldByID(id);
            return Ok(ApiResponse<FieldModel>.SuccessResponse(field));
        }

        [HttpPost("fields/upsert")]
        public async Task<IActionResult> UpsertField([FromBody] FieldViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<bool>.ErrorResponse("Invalid model state"));

            var isCreate = model.FieldId <= 0;
            if (isCreate && !await _menuPerm.Has(User, MenuPath, "Add"))
                return Ok(new { success = false, message = "You do not have permission to add fields." });
            if (!isCreate && !await _menuPerm.Has(User, MenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to edit fields." });

            model.CompanyID = CompanyId;
            model.SessionID = SessionId;
            var (success, message) = _fieldService.UpsertField(model, UserId);
            return success ? Ok(ApiResponse<bool>.SuccessResponse(true, message)) : BadRequest(ApiResponse<bool>.ErrorResponse(message));
        }

        [HttpPost("fields/delete")]
        public async Task<IActionResult> DeleteField(List<int> id)
        {
            if (!await _menuPerm.Has(User, MenuPath, "Delete"))
                return Ok(new { success = false, message = "You do not have permission to delete fields." });

            var (success, message) = _fieldService.DeleteField(id, UserId);
            return success ? Ok(ApiResponse<bool>.SuccessResponse(true, message)) : BadRequest(ApiResponse<bool>.ErrorResponse(message));
        }

        [HttpPost("fields/toggle-status")]
        public async Task<IActionResult> ToggleFieldStatus([FromBody] FieldToggleStatusRequest request)
        {
            if (!await _menuPerm.Has(User, MenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to change field status." });

            var (success, message) = _fieldService.ToggleFieldStatus(request.Id, request.IsActive, UserId);
            return success ? Ok(ApiResponse<bool>.SuccessResponse(true, message)) : BadRequest(ApiResponse<bool>.ErrorResponse(message));
        }

        [HttpGet("id-autogen/settings")]
        public async Task<IActionResult> GetIDAutoGenSettings()
        {
            var settings = await _fieldService.GetIDAutoGenSettings(CompanyId, SessionId);
            return Ok(ApiResponse<List<IDAutoGenSettings>>.SuccessResponse(settings));
        }

        [HttpPost("id-autogen/save")]
        public async Task<IActionResult> SaveIDAutoGenSettings([FromBody] IDAutoGenRequest request)
        {
            if (!await _menuPerm.Has(User, MenuPath, "Edit"))
                return Ok(new { success = false, message = "You do not have permission to change ID auto-generation settings." });

            request.CompanyID = CompanyId;
            request.SessionID = SessionId;
            var (success, message) = _fieldService.SaveIDAutoGenSettings(request, UserId);
            return success ? Ok(ApiResponse<bool>.SuccessResponse(true, message)) : BadRequest(ApiResponse<bool>.ErrorResponse(message));
        }
    }
}
