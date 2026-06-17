using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Interfaces;
using SchoolERP.API.Models;
using SchoolERP.API.Models.Common;
using SchoolERP.API.Services;
using System.Collections.Generic;
using System.Security.Claims;

namespace SchoolERP.Net.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountHeadApiController : ControllerBase
    {
        private readonly IAccountHeadService _accountHeadService;
        private readonly ICompanyService _companySvc;
        private readonly ISessionService _sessionSvc;
        private readonly IUserMenuPermissionService _menuPerm;

        public AccountHeadApiController(IAccountHeadService accountHeadService, ICompanyService companySvc, ISessionService sessionSvc, IUserMenuPermissionService menuPerm)
        {
            _accountHeadService = accountHeadService;
            _companySvc = companySvc;
            _sessionSvc = sessionSvc;
            _menuPerm = menuPerm;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "1");
        private int CompanyId => _companySvc.GetUserCurrentCompany(UserId) ?? 0;
        private int SessionId => _sessionSvc.GetUserCurrentSession(UserId) ?? 0;

        [HttpGet("GetAllAccountHeads")]
        public IActionResult GetAllAccountHeads(string headType, bool includeDeleted = false)
        {
            try
            {
                var data = _accountHeadService.GetAllAccountHeads(CompanyId, SessionId, headType, includeDeleted);
                return Ok(ApiResponse<List<AccountHeadViewModel>>.SuccessResponse(data));
            }
            catch (System.Exception ex)
            {
                return Ok(ApiResponse<List<AccountHeadViewModel>>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("GetAccountHeadByID/{id}")]
        public IActionResult GetAccountHeadByID(int id)
        {
            try
            {
                var data = _accountHeadService.GetAccountHeadByID(id);
                if (data == null) return NotFound(ApiResponse<AccountHeadViewModel>.ErrorResponse("Not found"));
                return Ok(ApiResponse<AccountHeadViewModel>.SuccessResponse(data));
            }
            catch (System.Exception ex)
            {
                return Ok(ApiResponse<AccountHeadViewModel>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("UpsertAccountHead")]
        public async Task<IActionResult> UpsertAccountHead([FromBody] AccountHeadUpsertRequest req)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                string menuPath = (req.HeadType == "Income") ? "/Income/IncomeHead" : "/Expense/ExpenseHead";
                var isCreate = req.AccountHeadID <= 0;
                if (isCreate && !await _menuPerm.Has(User, menuPath, "Add"))
                    return Ok(new { success = false, message = $"You do not have permission to add {req.HeadType.ToLower()} heads." });
                if (!isCreate && !await _menuPerm.Has(User, menuPath, "Edit"))
                    return Ok(new { success = false, message = $"You do not have permission to edit {req.HeadType.ToLower()} heads." });

                var result = _accountHeadService.UpsertAccountHead(req, CompanyId, SessionId, UserId);
                return Ok(new { success = result.Success, message = result.Message });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("DeleteAccountHead")]
        public async Task<IActionResult> DeleteAccountHead(List<int> id)
        {
            try
            {
                foreach (var accountEntryId in id)
                {
                    var entry = _accountHeadService.GetAccountHeadByID(accountEntryId);

                    if (entry == null)
                    {
                        return Ok(new
                        {
                            success = false,
                            message = $"Entry not found. ID : {accountEntryId}"
                        });
                    }

                    string menuPath = entry.HeadType == "Income"
                        ? "/Income"
                        : "/Expense";

                    if (!await _menuPerm.Has(User, menuPath, "Delete"))
                    {
                        return Ok(new
                        {
                            success = false,
                            message = $"You do not have permission to delete {entry.HeadType.ToLower()} heads."
                        });
                    }
                }
                
                var result = _accountHeadService.DeleteAccountHead(id, UserId);
                return Ok(new { success = result.Success, message = result.Message });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("ToggleAccountHeadStatus")]
        public async Task<IActionResult> ToggleAccountHeadStatus(int id, bool isActive)
        {
            try
            {
                var head = _accountHeadService.GetAccountHeadByID(id);
                if (head == null) return Ok(new { success = false, message = "Head not found." });
                string menuPath = (head.HeadType == "Income") ? "/Income/IncomeHead" : "/Expense/ExpenseHead";

                if (!await _menuPerm.Has(User, menuPath, "Edit"))
                    return Ok(new { success = false, message = $"You do not have permission to change {head.HeadType.ToLower()} head status." });

                var result = _accountHeadService.ToggleAccountHeadStatus(id, isActive, UserId);
                return Ok(new { success = result.Success, message = result.Message });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }
    }
}
