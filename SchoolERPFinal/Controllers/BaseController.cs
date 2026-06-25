using Microsoft.AspNetCore.Mvc;
using static SchoolERP.Net.Helpers.PermissionHelper;
using System.Security.Claims;
using SchoolERP.Net.Helpers;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Controllers
{
    public class BaseController : Controller
    {
        protected readonly PermissionHelper _permHelper;

        public BaseController(PermissionHelper permHelper)
        {
            _permHelper = permHelper;
        }

        // Call this in any action — one line!
        protected async Task<PagePermissions> GetPermissions(string pageUrl, string importUrl = null)
        {
            int userId = GetCurrentUserId();
            return await PagePermissionHelper.Load(_permHelper, userId, pageUrl, importUrl);
        }

        protected int GetCurrentUserId()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
                return 0;

            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                     ?? User.FindFirst("UserId");

            return claim != null && int.TryParse(claim.Value, out int id) ? id : 0;
        }

        protected int CurrentSessionId
        {
            get
            {
                if (Request.Cookies.TryGetValue("CurrentSessionId", out string val)
                    && int.TryParse(val, out int id))
                    return id;
                return 0;
            }
        }
    }
}
