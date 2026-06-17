using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Models;
using SchoolERP.Net.Services;
using SchoolERP.Net.Services.Clients;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace SchoolERP.Net.Controllers
{
    /// <summary>
    /// This controller is the central hub for all system settings, including company details, roles, email, SMS, and more.
    /// </summary>
    public class SettingsController : Controller
    {
        private readonly ISettingsClientService _settingsClient;
        private readonly IRoleClientService _roleClient;
        private readonly IUserClientService _userClient;
        private readonly IEmailConfigClientService _emailClient;
        private readonly ISmsConfigClientService _smsClient;
        private readonly IPaymentMethodClientService _paymentClient;
        private readonly ICompanyClientService _companyClient;
        private readonly ISessionClientService _sessionClient;
        private readonly ICurrencyClientService _currencyClient;
        private readonly IUserMenuPermissionClientService _menuPerm;
        private const string SettingsMenuPath = "/Settings";

        public SettingsController(
            ISettingsClientService settingsClient, 
            IRoleClientService roleClient, 
            IUserClientService userClient,
            IEmailConfigClientService emailClient,
            ISmsConfigClientService smsClient,
            IPaymentMethodClientService paymentClient,
            ICompanyClientService companyClient,
            ISessionClientService sessionClient,
            ICurrencyClientService currencyClient,
            IUserMenuPermissionClientService menuPerm)
        {
            _settingsClient = settingsClient;
            _roleClient = roleClient;
            _userClient = userClient;
            _emailClient = emailClient;
            _smsClient = smsClient;
            _paymentClient = paymentClient;
            _companyClient = companyClient;
            _sessionClient = sessionClient;
            _currencyClient = currencyClient;
            _menuPerm = menuPerm;
        }

        /// <summary>
        /// Shows the general settings page for the school's main information.
        /// </summary>
        public IActionResult CompanyMaster()
        {
            ViewData["Title"] = "Company Master";
            return View();
        }

        /// <summary>
        /// Shows the page where you can manage user roles and what each role is allowed to do.
        /// </summary>
        public async Task<IActionResult> Roles()
        {
            ViewData["Title"] = "Roles & Permission";
            
            // Invoke the backend Role Service to fetch all available roles
            var rolesResponse = await _roleClient.GetAllRolesAsync();
            
            // Map the service response to the ViewModel, providing an empty list fallback if the database call fails
            var model = new RolesPageViewModel
            {
                Roles = rolesResponse.Success
                    ? rolesResponse.Data.Select(r => new RoleViewModel
                    {
                        RoleID = r.RoleID,
                        RoleName = r.RoleName,
                        Description = r.RoleDesc ?? "",
                        IsActive = r.IsActive
                    }).ToList()
                    : new List<RoleViewModel>()
            };
            return View(model);
        }

        /// <summary>
        /// Gets the list of allowed actions for a specific role to show them on the screen.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPermissions(int roleId)
        {
            if (!(await _menuPerm.Has(SettingsMenuPath, "View")).Data)
                return Json(new { success = false, message = "You do not have permission to view role permissions.", permissions = (object?)null });

            var response = await _roleClient.GetPermissionsAsync(roleId);
            return Json(new { success = response.Success, permissions = response.Data, message = response.Message });
        }

        /// <summary>
        /// Saves a new user role or updates an existing one.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveRole([FromBody] RoleUpsertRequest request)
        {
            var isCreate = request.RoleID <= 0;
            if (isCreate && !(await _menuPerm.Has(SettingsMenuPath, "Add")).Data)
                return Json(new { success = false, message = "You do not have permission to add roles." });
            if (!isCreate && !(await _menuPerm.Has(SettingsMenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit roles." });

            // Transform the simplified incoming request into the strict Upsert pipeline request
            var serviceRequest = new MstRoleUpsertRequest
            {
                RoleID = request.RoleID,
                RoleName = request.RoleName,
                RoleDesc = request.Description,
                IsActive = true // Force active status on new creates from Settings Module wrapper
            };

            var response = await _roleClient.SaveRoleAsync(serviceRequest);
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Saves the chosen list of allowed actions for a specific role.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveRolePermissions([FromBody] MstRolePermissionSaveRequest request)
        {
            if (!(await _menuPerm.Has(SettingsMenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to update role permissions." });

            var response = await _roleClient.SavePermissionsAsync(request);
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Shows the page for setting up academic sessions (school years).
        /// </summary>
        public IActionResult Sessions()
        {
            ViewData["Title"] = "Sessions Setting";
            return View();
        }

        /// <summary>
        /// Shows the page for managing the different types of money used in the system.
        /// </summary>
        public IActionResult Currencies()
        {
            ViewData["Title"] = "Currencies";
            return View();
        }

        /// <summary>
        /// Shows the page where you configure the email server for sending notifications.
        /// </summary>
        public async Task<IActionResult> EmailSettings()
        {
            ViewData["Title"] = "Email Settings";
            var response = await _emailClient.GetEmailConfigAsync();
            var model = (response.Success && response.Data != null) ? response.Data : new MstEmailConfigViewModel();
            return View(model);
        }

        /// <summary>
        /// Saves the email server settings you provided.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveEmailSettings([FromBody] MstEmailConfigUpsertRequest request)
        {
            if (!(await _menuPerm.Has(SettingsMenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to change email settings." });

            var response = await _emailClient.UpsertEmailConfigAsync(request);
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Shows the page where you configure the SMS gateway for sending text messages.
        /// </summary>
        public async Task<IActionResult> SmsSettings()
        {
            ViewData["Title"] = "SMS Settings";
            var response = await _smsClient.GetSmsConfigAsync();
            var model = (response.Success && response.Data != null) ? response.Data : new MstSmsConfigViewModel();
            return View(model);
        }

        /// <summary>
        /// Saves the SMS gateway settings you provided.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveSmsSettings([FromBody] MstSmsConfigUpsertRequest request)
        {
            if (!(await _menuPerm.Has(SettingsMenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to change SMS settings." });

            var response = await _smsClient.UpsertSmsConfigAsync(request);
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Shows the page for managing the different languages the system supports.
        /// </summary>
        public IActionResult Languages()
        {
            ViewData["Title"] = "Languages";
            return View();
        }

        /// <summary>
        /// Shows the page where you can change specific words or phrases for a chosen language.
        /// </summary>
        public async Task<IActionResult> Translations(string id)
        {
            // Fail safe routing to root Languages board
            if (string.IsNullOrEmpty(id)) return RedirectToAction("Languages");
            
            ViewData["Title"] = "Translate " + System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(id.Replace("_", " "));
            ViewData["SelectedLanguage"] = id;
            
            // Extracts all JSON localized string translations from db/file cache
            var response = await _settingsClient.GetTranslationsAsync(id);
            return View(response.Success ? response.Data : new Dictionary<string, string>());
        }

        /// <summary>
        /// Saves the updated translation for a specific word or phrase.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateTranslation([FromBody] TranslationUpdateModel model)
        {
            if (!(await _menuPerm.Has(SettingsMenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to update translations." });

            var response = await _settingsClient.UpdateTranslationAsync(model);
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Redirects you to the main menu setup page.
        /// </summary>
        public IActionResult SidebarMenu()
        {
            return RedirectToAction("Index", "MasterMenu");
        }

        /// <summary>
        /// Shows the notification settings page.
        /// </summary>
        public IActionResult Notifications() => View();
        
        /// <summary>
        /// Shows the page for managing different ways payments can be made (like Razorpay).
        /// </summary>
        public async Task<IActionResult> PaymentMethods()
        {
            ViewData["Title"] = "Payment Methods";
            var companyResponse = await _companyClient.GetUserCurrentCompanyAsync();
            int companyId = companyResponse.Success ? (companyResponse.Data ?? 0) : 0;
            
            var response = await _paymentClient.GetPaymentMethodByCompanyAsync(companyId);
            var model = new PaymentMethodsPageViewModel
            {
                PaymentMethod = response.Success ? response.Data : null
            };
            return View(model);
        }

        /// <summary>
        /// Saves or updates a payment method's details.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SavePaymentMethod([FromBody] MstPaymentMethodUpsertRequest request)
        {
            var isCreate = request.PaymentId <= 0;
            if (isCreate && !(await _menuPerm.Has(SettingsMenuPath, "Add")).Data)
                return Json(new { success = false, message = "You do not have permission to add payment methods." });
            if (!isCreate && !(await _menuPerm.Has(SettingsMenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit payment methods." });

            var companyResponse = await _companyClient.GetUserCurrentCompanyAsync();
            request.CompanyId = companyResponse.Success ? (companyResponse.Data ?? 0) : 0;

            if (request.CompanyId <= 0)
                return Json(new { success = false, message = "Unable to determine current company." });

            var response = await _paymentClient.UpsertPaymentMethodAsync(request);
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Permanently removes a payment method from the system.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeletePaymentMethod(int id)
        {
            if (!(await _menuPerm.Has(SettingsMenuPath, "Delete")).Data)
                return Json(new { success = false, message = "You do not have permission to delete payment methods." });

            var response = await _paymentClient.DeletePaymentMethodAsync(id);
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Turns a payment method on or off.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> TogglePaymentStatus(int id, bool isActive)
        {
            if (!(await _menuPerm.Has(SettingsMenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to change payment method status." });

            var response = await _paymentClient.ToggleStatusAsync(id, isActive);
            return Json(new { success = response.Success, message = response.Message });
        }

        // Region: Legacy / Empty Layout Hooks
        /// <summary>
        /// Shows the history of system backups.
        /// </summary>
        public IActionResult BackupHistory() => View();

        /// <summary>
        /// Shows the page for setting up what appears at the top and bottom of printed documents.
        /// </summary>
        public IActionResult PrintHeaderFooter() => View();

        /// <summary>
        /// Shows the 'System Fields' page where you can choose which built-in fields are visible or required.
        /// </summary>
        public async Task<IActionResult> SystemFields()
        {
            ViewData["Title"] = "System Fields";
            var response = await _settingsClient.GetAllFieldsAsync(isSystemField: true);
            var model = response.Success ? response.Data : new List<FieldModel>();
            return View(model);
        }

        /// <summary>
        /// Shows the settings for student profile information.
        /// </summary>
        public IActionResult StudentProfile() => View();

        /// <summary>
        /// Shows the page for managing different parts (modules) of the application.
        /// </summary>
        public IActionResult Modules() => View();

        /// <summary>
        /// Shows the settings for allowed file types.
        /// </summary>
        public IActionResult FileType() => View();

        /// <summary>
        /// Shows the 'Custom Fields' page where you can add your own data fields to forms like Student Admission.
        /// </summary>
        public async Task<IActionResult> CustomFields()
        {
            ViewData["Title"] = "Custom Fields";
            var response = await _settingsClient.GetAllFieldsAsync(isSystemField: false);
            var model = response.Success ? response.Data : new List<FieldModel>();
            return View(model);
        }

        /// <summary>
        /// Shows the settings for security 'captcha' tests.
        /// </summary>
        public IActionResult Captcha() => View();

        /// <summary>
        /// Shows the page for uploading the school's logo.
        /// </summary>
        public IActionResult Logo() => View();

        /// <summary>
        /// Shows the page for changing the login screen background.
        /// </summary>
        public IActionResult LoginPageBG() => View();

        /// <summary>
        /// Shows the settings for student and guardian information.
        /// </summary>
        public IActionResult StudentGuardian() => View();

        /// <summary>
        /// Shows the page for configuring how IDs (like Admission Numbers) are automatically generated.
        /// </summary>
        public IActionResult IDAutoGen() => View();

        /// <summary>
        /// Shows the settings for different types of attendance.
        /// </summary>
        public IActionResult AttendanceType() => View();

        /// <summary>
        /// Shows the system maintenance page.
        /// </summary>
        public IActionResult Maintenance() => View();

        /// <summary>
        /// Shows various other small settings.
        /// </summary>
        public IActionResult Miscellaneous() => View();
        /// <summary>
        /// Shows the main page for managing all registered companies and branches.
        /// </summary>
        public async Task<IActionResult> Companies()
        {
            ViewData["Title"] = "Company Master";

            var companiesResponse = await _companyClient.GetAllAsync();
            var sessionsResponse = await _sessionClient.GetAllAsync();
            var currenciesResponse = await _currencyClient.GetAllAsync();

            var rawCompanies = companiesResponse.Success ? companiesResponse.Data : new List<MstCompanyViewModel>();
            var hierarchicalCompanies = new List<MstCompanyViewModel>();
            BuildCompanyHierarchy(rawCompanies, hierarchicalCompanies, null, 0);

            var model = new MstCompanyPageViewModel
            {
                Companies = hierarchicalCompanies,
                ParentCompanies = rawCompanies,
                Sessions = sessionsResponse.Success ? sessionsResponse.Data : new List<MstSessionViewModel>(),
                Currencies = currenciesResponse.Success ? currenciesResponse.Data : new List<MstCurrencyViewModel>()
            };

            return View(model);
        }

        private void BuildCompanyHierarchy(List<MstCompanyViewModel> source, List<MstCompanyViewModel> target, int? parentId, int level)
        {
            var children = source.Where(c => c.ParentCompanyId == parentId).OrderBy(c => c.SchoolName).ToList();
            foreach (var child in children)
            {
                child.Level = level;
                child.HasChildren = source.Any(c => c.ParentCompanyId == child.CompanyId);
                target.Add(child);
                BuildCompanyHierarchy(source, target, child.CompanyId, level + 1);
            }

            // Also add any orphaned nodes (where parent is not in source) at level 0 if this is the top-level call
            if (parentId == null)
            {
                var orphans = source.Where(c => c.ParentCompanyId != null && !source.Any(p => p.CompanyId == c.ParentCompanyId) && !target.Any(t => t.CompanyId == c.CompanyId)).OrderBy(c => c.SchoolName).ToList();
                foreach (var orphan in orphans)
                {
                    orphan.Level = 0;
                    orphan.HasChildren = source.Any(c => c.ParentCompanyId == orphan.CompanyId);
                    target.Add(orphan);
                    BuildCompanyHierarchy(source, target, orphan.CompanyId, 1);
                }
            }
        }

        /// <summary>
        /// Saves a new company or updates an existing one.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveCompany([FromBody] MstCompanyUpsertRequest request)
        {
            var isCreate = request.CompanyId <= 0;
            if (isCreate && !(await _menuPerm.Has(SettingsMenuPath, "Add")).Data)
                return Json(new { success = false, message = "You do not have permission to add companies." });
            if (!isCreate && !(await _menuPerm.Has(SettingsMenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit companies." });

            var response = await _companyClient.UpsertAsync(request);
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Permanently removes a company from the records.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteCompany([FromBody] List<int> id)
        {
            if (!(await _menuPerm.Has(SettingsMenuPath, "Delete")).Data)
                return Json(new { success = false, message = "You do not have permission to delete companies." });

            var response = await _companyClient.DeleteAsync(id);
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Gets the details of a specific company for viewing or editing.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCompany(int id)
        {
            if (!(await _menuPerm.Has(SettingsMenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit companies.", data = (object?)null });

            var response = await _companyClient.GetByIDAsync(id);
            return Json(new { success = response.Success, data = response.Data, message = response.Message });
        }

        /// <summary>
        /// Turns a company's active status on or off.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ToggleCompanyStatus(int id, bool isActive)
        {
            if (!(await _menuPerm.Has(SettingsMenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to change company status." });

            var response = await _companyClient.ToggleStatusAsync(id, isActive);
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Shows the main page for managing all user accounts in the system.
        /// </summary>
        public async Task<IActionResult> Users()
        {
            ViewData["Title"] = "Users";

            var usersResponse = await _userClient.GetAllUsersAsync();
            var rolesResponse = await _userClient.GetRolesDropdownAsync();
            var typesResponse = await _userClient.GetUserTypesDropdownAsync();

            var model = new UsersPageViewModel
            {
                Users = usersResponse.Success ? usersResponse.Data : new List<UserViewModel>(),
                Roles = rolesResponse.Success ? rolesResponse.Data : new List<RoleViewModel>(),
                UserTypes = typesResponse.Success ? typesResponse.Data : new List<MstUserTypeViewModel>()
            };

            return View(model);
        }

        /// <summary>
        /// Saves a new user account or updates an existing one.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveUser([FromBody] UserUpsertRequest request)
        {
            var isCreate = request.UserID <= 0;
            if (isCreate && !(await _menuPerm.Has(SettingsMenuPath, "Add")).Data)
                return Json(new { success = false, message = "You do not have permission to add users." });
            if (!isCreate && !(await _menuPerm.Has(SettingsMenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit users." });

            var response = await _userClient.SaveUserAsync(request);
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Turns a user account on or off.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int userId, bool isActive)
        {
            if (!(await _menuPerm.Has(SettingsMenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to change user status." });

            var response = await _userClient.ToggleStatusAsync(userId, isActive);
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Permanently removes a user account.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!(await _menuPerm.Has(SettingsMenuPath, "Delete")).Data)
                return Json(new { success = false, message = "You do not have permission to delete users." });

            var response = await _userClient.DeleteUserAsync(id);
            return Json(new { success = response.Success, message = response.Message });
        }

        #region Settings Fields & IDAutoGen API Proxy Endpoints

        [HttpGet]
        public async Task<IActionResult> GetAllFields(bool? isSystemField = null, string belongsTo = null)
        {
            var res = await _settingsClient.GetAllFieldsAsync(isSystemField, belongsTo);
            return Json(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetFieldByID(int id)
        {
            var res = await _settingsClient.GetFieldByIDAsync(id);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> UpsertField([FromBody] FieldViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid model state" });

            var isCreate = model.FieldId <= 0;
            if (isCreate && !(await _menuPerm.Has(SettingsMenuPath, "Add")).Data)
                return Json(new { success = false, message = "You do not have permission to add fields." });
            if (!isCreate && !(await _menuPerm.Has(SettingsMenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit fields." });

            var res = await _settingsClient.UpsertFieldAsync(model);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteField([FromBody]  List<int> id)
        {
            if (!(await _menuPerm.Has(SettingsMenuPath, "Delete")).Data)
                return Json(new { success = false, message = "You do not have permission to delete fields." });

            var res = await _settingsClient.DeleteFieldAsync(id);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFieldStatus([FromBody] FieldToggleStatusRequest request)
        {
            if (!(await _menuPerm.Has(SettingsMenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to change field status." });

            var res = await _settingsClient.ToggleFieldStatusAsync(request.Id, request.IsActive);
            return Json(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetIDAutoGenSettings()
        {
            var res = await _settingsClient.GetIDAutoGenSettingsAsync();
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> SaveIDAutoGenSettings([FromBody] IDAutoGenRequest request)
        {
            if (!(await _menuPerm.Has(SettingsMenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to change ID auto-generation settings." });

            var res = await _settingsClient.SaveIDAutoGenSettingsAsync(request);
            return Json(res);
        }

        #endregion
    }
}
