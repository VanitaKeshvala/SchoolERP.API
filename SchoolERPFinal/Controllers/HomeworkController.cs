using Microsoft.AspNetCore.Mvc;
using SchoolERP.Shared.Models;
using SchoolERP.Net.Services;
using SchoolERP.Net.Services.Clients;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Net.Helpers;

namespace SchoolERP.Net.Controllers
{
    public class HomeworkController : BaseController
    {
        private readonly IHomeworkClientService _homeworkClient;
        private readonly IClassClientService _classClient;
        private readonly ISubjectGroupClientService _subjectGroupClient;
        private readonly ISubjectClientService _subjectClient;
        private readonly IUserMenuPermissionClientService _menuPerm;

        private const string MenuPath = "/Homework/Add";

        public HomeworkController(
            IHomeworkClientService homeworkClient, 
            IClassClientService classClient, 
            ISubjectGroupClientService subjectGroupClient,
            ISubjectClientService subjectClient,
            IUserMenuPermissionClientService menuPerm, PermissionHelper permHelper) : base(permHelper)
        {
            _homeworkClient = homeworkClient;
            _classClient = classClient;
            _subjectGroupClient = subjectGroupClient;
            _subjectClient = subjectClient;
            _menuPerm = menuPerm;
        }

        public async Task<IActionResult> Add()
        {
            // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
            var perms = await GetPermissions(
               "/Homework/Add"
           );
            var homeworksResponse = await _homeworkClient.GetAllAsync();
            var classesResponse = await _classClient.GetAllAsync();

            var model = new HomeworkPageViewModel
            {
                Homeworks = homeworksResponse.Success ? homeworksResponse.Data : new List<HomeworkViewModel>(),
                Classes = classesResponse.Success ? classesResponse.Data : new List<MstClassViewModel>()
            };
            model.Permissions = perms;
            return View(model);
        }

        #region Homework API Proxy Endpoints

        [HttpGet]
        public async Task<IActionResult> GetSubjectGroups()
        {
            var res = await _subjectGroupClient.GetAllAsync();
            return Json(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetSubjectGroupByID(int id)
        {
            var res = await _subjectGroupClient.GetByIDAsync(id);
            return Json(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetSubjects()
        {
            var res = await _subjectClient.GetAllAsync();
            return Json(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetHomeworkByID(int id)
        {
            var res = await _homeworkClient.GetByIDAsync(id);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> UpsertHomework([FromBody] HomeworkUpsertRequest request)
        {
            var isCreate = request.HomeworkID <= 0;
            if (isCreate && !(await _menuPerm.Has(MenuPath, "Add")).Data    )
                return Json(new { success = false, message = "You do not have permission to add homework." });
            if (!isCreate && !(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit homework." });

            var res = await _homeworkClient.UpsertAsync(request);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteHomework(int id)
        {
            if (!(await _menuPerm.Has(MenuPath, "Delete")).Data)
                return Json(new { success = false, message = "You do not have permission to delete homework." });

            var res = await _homeworkClient.DeleteAsync(id);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleHomeworkStatus(int id, bool isActive)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to change homework status." });

            var res = await _homeworkClient.ToggleStatusAsync(id, isActive);
            return Json(res);
        }

        #endregion
    }
}
