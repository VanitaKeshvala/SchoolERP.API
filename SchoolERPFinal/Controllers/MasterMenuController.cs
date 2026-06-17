using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Models;
using SchoolERP.Net.Services;
using SchoolERP.Net.Services.Clients;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolERP.Net.Controllers
{
    /// <summary>
    /// This controller manages the system's navigation menus, allowing you to organize the sidebar and set up menu items.
    /// </summary>
    public class MasterMenuController : Controller
    {
        private readonly IMenuClientService _menuClient;
        private readonly IUserMenuPermissionClientService _menuPerm;
        private const string MenuPath = "/MasterMenu";

        public MasterMenuController(IMenuClientService menuClient, IUserMenuPermissionClientService menuPerm)
        {
            _menuClient = menuClient;
            _menuPerm = menuPerm;
        }

        /// <summary>
        /// Shows the setup page where you can see all available menus and their sub-menus.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var response = await _menuClient.GetAllMenusAsync();
            var menus = response.Success ? response.Data : new System.Collections.Generic.List<MenuViewModel>();

            var model = new MenusPageViewModel
            {
                Menus = menus,
                ParentMenus = menus // Provide all menus to support multi-level selection
            };
            return View(model);
        }

        /// <summary>
        /// Gets the details of a specific menu item so you can view its settings or edit them.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMenu(int menuId)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit menus." });

            var response = await _menuClient.GetMenuByIdAsync(menuId);
            if (!response.Success) return Json(new { success = false, message = response.Message });
            return Json(new { success = true, menu = response.Data });
        }

        /// <summary>
        /// Saves a new menu item or updates an existing one based on the information you entered.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] MenuUpsertRequest request)
        {
            var isCreate = request.MenuID <= 0;
            if (isCreate && !(await _menuPerm.Has(MenuPath, "Add")).Data)
                return Json(new { success = false, message = "You do not have permission to add menus." });
            if (!isCreate && !(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit menus." });

            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { success = false, message = errors });
            }

            var response = await _menuClient.SaveMenuAsync(request);
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Turns a menu item on or off, determining if it appears in the sidebar.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int menuId, bool isActive)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to change menu status." });

            var response = await _menuClient.ToggleStatusAsync(menuId, isActive);
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Changes the order in which menu items appear in the sidebar.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateOrder([FromBody] List<MenuOrderRequest> orders)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to reorder menus." });

            var response = await _menuClient.UpdateMenuOrderAsync(orders);
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Deletes a menu item.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] List<int> id)
        {
            if (!(await _menuPerm.Has(MenuPath, "Delete")).Data)
                return Json(new { success = false, message = "You do not have permission to delete menus." });

            var response = await _menuClient.DeleteMenuAsync(id);
            return Json(new { success = response.Success, message = response.Message });
        }
    }
}
