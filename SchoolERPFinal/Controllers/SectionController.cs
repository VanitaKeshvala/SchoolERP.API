using Microsoft.AspNetCore.Mvc;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using SchoolERP.Net.Services;
using SchoolERP.Net.Services.Clients;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Net.Helpers;

namespace SchoolERP.Net.Controllers
{
    public class SectionController : BaseController
    {
        private readonly ISectionClientService _sectionClient;
        private readonly IUserMenuPermissionClientService _menuPerm;
        
        private const string MenuPath = "/Section";

        public SectionController(ISectionClientService sectionClient, IUserMenuPermissionClientService menuPerm , PermissionHelper permHelper) : base(permHelper)
        {
            _sectionClient = sectionClient;
            _menuPerm = menuPerm;
            
        }

        public async Task<IActionResult> Index()
        {
            // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
            var perms = await GetPermissions(
               "/Section"
           );
            var sessionId = CurrentSessionId;
            var response = await _sectionClient.GetAllAsync(false, sessionId);
            if(response.Data != null) 
            {
                
            }
            var model = new MstSectionPageViewModel
            {
                Sections = response.Success ? response.Data : new List<MstSectionViewModel>()
            };
            model.Permissions = perms;
            return View(model);
        }

        public async Task<IActionResult> Add(int? id)
        {
            // Step 1: Initialize a new blank page model.
            var model = new MstSectionViewModel();
           
            // Step 2: If we are editing an existing person (ID is provided), fetch their details.

            if (id.HasValue && id.Value > 0)
            {
                var sectionRes = await _sectionClient.GetByIDAsync(id.Value);
                if (sectionRes.Success)
                {
                    model.EditSections = sectionRes.Data;
                }
            }
            else 
            {
                model.EditSections = null;
            }
                return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetSection(int id)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit sections." });

            var response = await _sectionClient.GetByIDAsync(id);
            if (!response.Success) return Json(new { success = false, message = response.Message });
            return Json(new { success = true, data = response.Data });
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] MstSectionUpsertRequest request)
        {
            var isCreate = request.SectionID <= 0;
            if (isCreate && !(await _menuPerm.Has(MenuPath, "Add")).Data)
                return Json(new { success = false, message = "You do not have permission to add sections." });
            if (!isCreate && !(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit sections." });
            
            var response = await _sectionClient.UpsertAsync(request);
            return Json(new { success = true, message = response.Message });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus([FromBody] StatusUpdateRequest request)
        {
            try
            {
                if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "You do not have permission to change section status." });

                var response = await _sectionClient.ToggleStatusAsync(request);
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
            if (!(await _menuPerm.Has(MenuPath, "Delete")).Data)
                return Json(new { success = false, message = "You do not have permission to delete sections." });

            var response = await _sectionClient.DeleteAsync(ids);
            return Json(new { success = response.Success, message = response.Message });
        }

        [HttpPost]
        public async Task<IActionResult> CopyToSession([FromBody] SectionCopyRequest request)
        {
            try
            {
                if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "You do not have permission to delete sections." });

                var response = await _sectionClient.CopyToSessionAsync(request);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            
        }
    }
}
