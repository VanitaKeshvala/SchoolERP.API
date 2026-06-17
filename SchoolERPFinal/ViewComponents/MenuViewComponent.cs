using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using NuGet.Packaging.Signing;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;
using SchoolERP.Net.Services.Clients;
using System.Buffers.Text;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.ComponentModel;

namespace SchoolERP.Net.ViewComponents
{
    /// <summary>
    /// View Component responsible for loading and rendering the application's
    /// dynamic navigation menu based on the authenticated user's permissions.
    /// Retrieves menu data and user permissions from API services,
    /// filters accessible menus, and renders the menu hierarchy.
    /// </summary>
    public class MenuViewComponent : ViewComponent
    {
        private readonly IMenuApiClient _apiClient;

        public MenuViewComponent(IMenuApiClient apiClient)
        {
            _apiClient = apiClient;
        }


        /// <summary>
        /// Added MenuViewComponent for dynamic permission-based menu rendering.
        /// Retrieves authenticated user permissions
        ///Loads active menus from API
        ///Filters menus based on View access
        ///Includes parent menu hierarchy automatically
        ///Renders reusable sidebar navigation component
        ///Improves separation of concerns and layout maintainability
        /// </summary>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<UserPermissionViewModel> models = new List<UserPermissionViewModel>();
            // Load user context for permissions
            int currentUserId = 0;
            var user = HttpContext.User;
            if (User.Identity?.IsAuthenticated == true)
            {
                // Prefer standard NameIdentifier, fallback to custom claim used elsewhere
                var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)
                           ?? user.FindFirst("UserId");

                if (idClaim != null)
                {
                    int.TryParse(idClaim.Value, out currentUserId);
                }
            }

            // If not authenticated, load no permissions (menu becomes empty except static items)
            if (currentUserId > 0) 
            {
                var responses =await _apiClient.LoadPermissions(currentUserId, null, "View");
                models = responses.Data;
            }
            var response = await _apiClient.GetMenusAsync();

            var activeMenus = response.Data?
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ToList() ?? new List<MenuViewModel>();


            var menuDict = activeMenus.ToDictionary(m => m.MenuID);


            var showMenuIds = new HashSet<int>();

            foreach (var item in models)
            {
                showMenuIds.Add(item.MenuID);

                var current = menuDict.ContainsKey(item.MenuID)
                    ? menuDict[item.MenuID]
                    : null;

                while (current != null && current.ParentID.HasValue)
                {
                    if (showMenuIds.Add(current.ParentID.Value))
                    {
                        current = menuDict.ContainsKey(current.ParentID.Value)
                            ? menuDict[current.ParentID.Value]
                            : null;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            var filteredMenus = activeMenus
                .Where(m => showMenuIds.Contains(m.MenuID))
                .ToList();

            return View(filteredMenus);
        }
    }
}
