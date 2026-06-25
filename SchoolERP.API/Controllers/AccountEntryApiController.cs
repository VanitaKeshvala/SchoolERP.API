using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using SchoolERP.API.Services;

namespace SchoolERP.Net.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountEntryApiController : ControllerBase
    {
        private readonly IAccountEntryService _accountEntryService;
        private readonly ICompanyService _companySvc;
        private readonly ISessionService _sessionSvc;
        private readonly IUserMenuPermissionService _menuPerm;

        public AccountEntryApiController(IAccountEntryService accountEntryService, ICompanyService companySvc, ISessionService sessionSvc, IUserMenuPermissionService menuPerm)
        {
            _accountEntryService = accountEntryService;
            _companySvc = companySvc;
            _sessionSvc = sessionSvc;
            _menuPerm = menuPerm;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "1");
        private int CompanyId => _companySvc.GetUserCurrentCompany(UserId) ?? 0;
        private int SessionId => _sessionSvc.GetUserCurrentSession(UserId) ?? 0;

        [HttpGet("GetAllAccountEntries")]
        public IActionResult GetAllAccountEntries(string entryType, bool includeDeleted = false)
        {
            try
            {
                var data = _accountEntryService.GetAllAccountEntries(CompanyId, SessionId, entryType, includeDeleted);
                return Ok(ApiResponse<List<AccountEntryViewModel>>.SuccessResponse(data));
            }
            catch (System.Exception ex)
            {
                return Ok(ApiResponse<List<AccountEntryViewModel>>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("SearchAccountEntries")]
        public IActionResult SearchAccountEntries([FromBody] AccountEntrySearchRequest req)
        {
            try
            {
                var data = _accountEntryService.SearchAccountEntries(CompanyId, SessionId, req.EntryType, req.SearchType, req.DateFrom, req.DateTo);
                return Ok(ApiResponse<List<AccountEntryViewModel>>.SuccessResponse(data));
            }
            catch (System.Exception ex)
            {
                return Ok(ApiResponse<List<AccountEntryViewModel>>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("GetAccountEntryByID/{id}")]
        public IActionResult GetAccountEntryByID(int id)
        {
            try
            {
                var data = _accountEntryService.GetAccountEntryByID(id);
                if (data == null) return NotFound(ApiResponse<AccountEntryViewModel>.ErrorResponse("Not found"));
                return Ok(ApiResponse<AccountEntryViewModel>.SuccessResponse(data));
            }
            catch (System.Exception ex)
            {
                return Ok(ApiResponse<AccountEntryViewModel>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("UpsertAccountEntry")]
        public async Task<IActionResult> UpsertAccountEntry([FromBody] AccountEntryUpsertRequest req)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                string menuPath = (req.EntryType == "Income") ? "/Income" : "/Expense";
                var isCreate = req.AccountEntryID <= 0;
                if (isCreate && !await _menuPerm.Has(User, menuPath, "Add"))
                    return Ok(new { success = false, message = $"You do not have permission to add {req.EntryType.ToLower()} entries." });
                if (!isCreate && !await _menuPerm.Has(User, menuPath, "Edit"))
                    return Ok(new { success = false, message = $"You do not have permission to edit {req.EntryType.ToLower()} entries." });

                var result = _accountEntryService.UpsertAccountEntry(req, CompanyId, SessionId, UserId);
                return Ok(new { success = result.Success, message = result.Message });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("DeleteAccountEntry")]
        public async Task<IActionResult> DeleteAccountEntry(List<int> id)
        {
            try
            {
                foreach (var accountEntryId in id)
                {
                    var entry = _accountEntryService.GetAccountEntryByID(accountEntryId);

                    if (entry == null)
                    {
                        return Ok(new
                        {
                            success = false,
                            message = $"Entry not found. ID : {accountEntryId}"
                        });
                    }

                    string menuPath = entry.EntryType == "Income"
                        ? "/Income"
                        : "/Expense";

                    if (!await _menuPerm.Has(User, menuPath, "Delete"))
                    {
                        return Ok(new
                        {
                            success = false,
                            message = $"You do not have permission to delete {entry.EntryType.ToLower()} entries."
                        });
                    }
                }
                var result = _accountEntryService.DeleteAccountEntry(id, UserId);
                return Ok(new { success = result.Success, message = result.Message });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("ToggleAccountEntryStatus")]
        public async Task<IActionResult> ToggleAccountEntryStatus(int id, bool isActive)
        {
            try
            {
                var entry = _accountEntryService.GetAccountEntryByID(id);
                if (entry == null) return Ok(new { success = false, message = "Entry not found." });
                string menuPath = (entry.EntryType == "Income") ? "/Income" : "/Expense";

                if (!await _menuPerm.Has(User, menuPath, "Edit"))
                    return Ok(new { success = false, message = $"You do not have permission to change {entry.EntryType.ToLower()} entry status." });

                var result = _accountEntryService.ToggleAccountEntryStatus(id, isActive, UserId);
                return Ok(new { success = result.Success, message = result.Message });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }
    }
}
