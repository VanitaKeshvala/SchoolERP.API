using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolERP.Shared.Models;
using SchoolERP.Net.Services;
using SchoolERP.Net.Services.Clients;

namespace SchoolERP.Net.Helpers
{
    public class PermissionHelper
    {
        private readonly IUserManagementClientService _userMgmtService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private List<UserPermissionViewModel>? _userPermissions = null;
        private bool _isSuperAdmin = false;

        public PermissionHelper(IUserManagementClientService userMgmtService, IHttpContextAccessor httpContextAccessor)
        {
            _userMgmtService = userMgmtService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LoadPermissions(int userId)
        {
            if (_userPermissions == null)
            {
                _isSuperAdmin = IsSuperAdmin(_httpContextAccessor.HttpContext?.User);
                if (_isSuperAdmin)
                {
                    _userPermissions = new List<UserPermissionViewModel>();
                    return;
                }
                _userPermissions = (await _userMgmtService.GetUserPermissions(userId)).Data;
            }
        }

        public bool HasPermission(int menuId, string permissionName)
        {
            if (_isSuperAdmin) return true;
            if (_userPermissions == null) return false;
            return _userPermissions.Any(p => p.MenuID == menuId && p.PermissionName.Equals(permissionName, System.StringComparison.OrdinalIgnoreCase));
        }

        public bool HasPermissionByKey(string menuKey, string permissionName)
        {
            if (_isSuperAdmin) return true;
            if (_userPermissions == null) return false;
            return _userPermissions.Any(p => p.MenuKey.Equals(menuKey, System.StringComparison.OrdinalIgnoreCase) && p.PermissionName.Equals(permissionName, System.StringComparison.OrdinalIgnoreCase));
        }

        public bool HasPermissionByUrl(string menuUrl, string permissionName)
        {
            if (_isSuperAdmin) return true;
            if (_userPermissions == null || string.IsNullOrWhiteSpace(menuUrl)) return false;
            var key = menuUrl.Trim().TrimEnd('/');
            return _userPermissions.Any(p =>
                !string.IsNullOrEmpty(p.MenuURL) &&
                p.PermissionName.Equals(permissionName, System.StringComparison.OrdinalIgnoreCase) &&
                (p.MenuURL.Equals(key, System.StringComparison.OrdinalIgnoreCase) ||
                 p.MenuURL.StartsWith(key + "/", System.StringComparison.OrdinalIgnoreCase) ||
                 p.MenuURL.Contains(key, System.StringComparison.OrdinalIgnoreCase)));
        }

        public List<UserPermissionViewModel> GetPermissions() => _userPermissions ?? new List<UserPermissionViewModel>();

        private static bool IsSuperAdmin(ClaimsPrincipal? user)
        {
            if (user?.Identity?.IsAuthenticated != true) return false;

            return user.Claims.Any(c =>
                (string.Equals(c.Type, ClaimTypes.Role, System.StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(c.Type, "Role", System.StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(c.Type, "UserTypeName", System.StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(c.Type, "UserType", System.StringComparison.OrdinalIgnoreCase)) &&
                IsSuperAdminValue(c.Value));
        }

        private static bool IsSuperAdminValue(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            var normalized = value.Replace(" ", string.Empty)
                .Replace("-", string.Empty)
                .Replace("_", string.Empty)
                .Trim();
            return string.Equals(normalized, "SuperAdmin", System.StringComparison.OrdinalIgnoreCase);
        }


        

        public  static class PagePermissionHelper
        {
            public static async Task<PagePermissions> Load(
                PermissionHelper permHelper,
                int userId,
                string pageUrl,
                string importUrl = null)
            {
                // If not logged in → return all false
                if (userId <= 0)
                    return PagePermissions.Denied;

                // Load once
                 await permHelper.LoadPermissions(userId);

                return new PagePermissions
                {
                    CanView = permHelper.HasPermissionByUrl(pageUrl, "View"),
                    CanAdd = permHelper.HasPermissionByUrl(pageUrl, "Add"),
                    CanEdit = permHelper.HasPermissionByUrl(pageUrl, "Edit"),
                    CanDelete = permHelper.HasPermissionByUrl(pageUrl, "Delete"),
                    CanPrint = permHelper.HasPermissionByUrl(pageUrl, "Print"),
                    CanExport = permHelper.HasPermissionByUrl(pageUrl, "Export"),
                    CanImport = importUrl != null && permHelper.HasPermissionByUrl(importUrl, "Import"),
                };
            }
        }
    }
}
