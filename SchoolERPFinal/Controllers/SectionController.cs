using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Models;
using SchoolERP.Net.Services;
using SchoolERP.Net.Services.Clients;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchoolERP.Net.Controllers
{
    public class SectionController : Controller
    {
        private readonly ISectionClientService _sectionClient;
        private readonly IUserMenuPermissionClientService _menuPerm;
        
        private const string MenuPath = "/Section";

        public SectionController(ISectionClientService sectionClient, IUserMenuPermissionClientService menuPerm)
        {
            _sectionClient = sectionClient;
            _menuPerm = menuPerm;
            
        }

        public async Task<IActionResult> Index()
        {
            var response = await _sectionClient.GetAllAsync();
            var model = new MstSectionPageViewModel
            {
                Sections = response.Success ? response.Data : new List<MstSectionViewModel>()
            };
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
        public async Task<IActionResult> ToggleStatus(int id, bool isActive)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to change section status." });

            var response = await _sectionClient.ToggleStatusAsync(id, isActive);
            return Json(new { success = response.Success, message = response.Message });
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] List<int> ids)
        {
            if (!(await _menuPerm.Has(MenuPath, "Delete")).Data)
                return Json(new { success = false, message = "You do not have permission to delete sections." });

            var response = await _sectionClient.DeleteAsync(ids);
            return Json(new { success = response.Success, message = response.Message });
        }
    }
}
