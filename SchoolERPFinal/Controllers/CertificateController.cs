using Microsoft.AspNetCore.Mvc;
using SchoolERP.Shared.Models;
using SchoolERP.Net.Services.Clients;
using System.Threading.Tasks;

using SchoolERP.Net.Services;
using System.Collections.Generic;
using SchoolERP.Net.Helpers;
using System.Security.Claims;

namespace SchoolERP.Net.Controllers
{
    public class CertificateController : BaseController
    {
        private readonly IStudentCertificateClientService _certificateClient;
        private readonly IStudentIDCardClientService _idCardClient;
        private readonly IStaffIDCardClientService _staffIdCardClient;
        private readonly IClassClientService _classClient;
        private readonly ISectionClientService _sectionClient;
        private readonly IStudentInformationClientService _studentInfoClient;
        private readonly IHumanResourceClientService _hrClient;
        private readonly IUserMenuPermissionClientService _menuPerm;
        private readonly ICompanyClientService _companyService;
        private readonly ISessionClientService _sessionService;
        public CertificateController(
            IStudentCertificateClientService certificateClient, 
            IStudentIDCardClientService idCardClient,
            IStaffIDCardClientService staffIdCardClient,
            IClassClientService classClient,
            ISectionClientService sectionClient,
            IStudentInformationClientService studentInfoClient,
            IHumanResourceClientService hrClient,
            IUserMenuPermissionClientService menuPerm,
            ICompanyClientService companyService,
            ISessionClientService sessionService,
            PermissionHelper permHelper) : base(permHelper)
        {
            _certificateClient = certificateClient;
            _idCardClient = idCardClient;
            _staffIdCardClient = staffIdCardClient;
            _classClient = classClient;
            _sectionClient = sectionClient;
            _studentInfoClient = studentInfoClient;
            _hrClient = hrClient;
            _menuPerm = menuPerm;
            _companyService = companyService;
            _sessionService = sessionService;
        }
        private int GetUserId() => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("UserId")?.Value, out var id) ? id : 0;
        private async Task<int> GetCompanyId()
        {
            if (CurrentCompanyId == null) 
            {
                var response = await _companyService.GetUserCurrentCompanyAsync();
                return response?.Data ?? 0;
            }
            return CurrentCompanyId;
        } 
        private async Task<int> GetSessionId()
        {
            if (CurrentSessionId == null)
            {
                var response = await _sessionService.GetUserCurrentSessionAsync();
                return response?.Data ?? 0;
            }
            return CurrentSessionId;
        }
        private int? GetStaffIDForLogin()
        {
            var staffClaim = User.FindFirst("StaffID")?.Value;
            return int.TryParse(staffClaim, out var staffId) ? staffId : null;
        }
        public async Task<IActionResult> StudentCertificate()
        {
            // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
            var perms = await GetPermissions(
               "/Certificate/StudentCertificate"
           );
            var model = new StudentCertificatePageViewModel();
            var resp = await _certificateClient.GetAll();
            if (resp.Success)
            {
                model.Certificates = resp.Data;
            }
            model.Permissions = perms;
            return View(model);
        }

        public async Task<IActionResult> GenerateCertificate()
        {
            // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
            var perms = await GetPermissions(
               "/HumanResource/Staffs"
           );

            return View(perms);
        }

        public async Task<IActionResult> StudentIdCard()
        {
            // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
            var perms = await GetPermissions(
               "/Certificate/StudentIdCard"
           );
            var model = new StudentIDCardPageViewModel();
            var resp = await _idCardClient.GetAll();
            if (resp.Success)
            {
                model.IDCards = resp.Data;
            }
            model.Permissions = perms;
            return View(model);
        }

        public async Task<IActionResult> GenerateStudentIdCard()
        {
            // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
            var perms = await GetPermissions(
               "/Certificate/GenerateStudentIdCard"
           );
            return View(perms);
        }

        public async Task<IActionResult> StaffIdCard()
        {
            // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
            var perms = await GetPermissions(
               "/Certificate/StaffIdCard"
           );
            var model = new StaffIDCardPageViewModel();
            var resp = await _staffIdCardClient.GetAll();
            if (resp.Success)
            {
                model.IDCards = resp.Data;
            }
            model.Permissions = perms;
            return View(model);
        }

        public async Task<IActionResult> GenerateStaffIdCard()
        {
            // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
            var perms = await GetPermissions(
               "/Certificate/GenerateStaffIdCard"
           );
            return View(perms);
        }

        #region Certificate Module API Proxy Endpoints

        // --- Classes & Sections ---
        [HttpGet]
        public async Task<IActionResult> GetAllClasses()
        {
            var res = await _classClient.GetAllAsync();
            return Json(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetSectionsByClass(int classId)
        {
            var res = await _sectionClient.GetByClassAsync(classId);
            return Json(res);
        }

        // --- Student Information ---
        [HttpGet]
        public async Task<IActionResult> GetStudentList(int? classId = null, int? sectionId = null, string? searchTerm = null)
        {
            var companyId = await GetCompanyId();
            var sessionId = await GetSessionId();
            var res = await _studentInfoClient.GetStudentListAsync(companyId,sessionId, classId, sectionId, searchTerm);
            return Json(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentByID(int id)
        {
            var res = await _studentInfoClient.GetStudentByIDAsync(id);
            return Json(res);
        }

        // --- Human Resources ---
        [HttpGet]
        public async Task<IActionResult> GetAllDepartments()
        {
            var res = await _hrClient.GetAllDepartmentsAsync();
            return Json(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDesignations()
        {
            var res = await _hrClient.GetAllDesignationsAsync();
            return Json(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStaff()
        {
            var sessionId = await GetSessionId();
            var companyId = await GetCompanyId();
            var staffId = GetStaffIDForLogin();
            var res = await _hrClient.GetAllStaffAsync(companyId,sessionId, staffId);
            return Json(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetStaffByID(int id)
        {
            var res = await _hrClient.GetStaffByIDAsync(id);
            return Json(res);
        }

        // --- Student ID Cards ---
        [HttpGet]
        public async Task<IActionResult> GetAllStudentIDCards()
        {
            var res = await _idCardClient.GetAll();
            return Json(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentIDCardByID(int id)
        {
            var res = await _idCardClient.GetByID(id);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> UpsertStudentIDCard([FromBody] StudentIDCardUpsertRequest request)
        {
            var isCreate = request.IDCardID <= 0;
            if (isCreate && !(await _menuPerm.Has("/Certificate/StudentIdCard", "Add")).Data)
                return Json(new { success = false, message = "You do not have permission to add student ID cards." });
            if (!isCreate && !(await _menuPerm.Has("/Certificate/StudentIdCard", "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit student ID cards." });

            var res = await _idCardClient.Upsert(request);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudentIDCard([FromBody] List<int> id)
        {
            if (!(await _menuPerm.Has("/Certificate/StudentIdCard", "Delete")).Data)
                return Json(new { success = false, message = "You do not have permission to delete student ID cards." });

            var res = await _idCardClient.Delete(id);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStudentIDCardStatus(int id, bool isActive)
        {
            if (!(await _menuPerm.Has("/Certificate/StudentIdCard", "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit student ID cards." });

            var res = await _idCardClient.ToggleStatus(id, isActive);
            return Json(res);
        }

        // --- Staff ID Cards ---
        [HttpGet]
        public async Task<IActionResult> GetAllStaffIDCards()
        {
            var res = await _staffIdCardClient.GetAll();
            return Json(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetStaffIDCardByID(int id)
        {
            var res = await _staffIdCardClient.GetByID(id);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> UpsertStaffIDCard([FromBody] StaffIDCardUpsertRequest request)
        {
            var isCreate = request.IDCardID <= 0;
            if (isCreate && !(await _menuPerm.Has("/Certificate/StaffIdCard", "Add")).Data)
                return Json(new { success = false, message = "You do not have permission to add staff ID cards." });
            if (!isCreate && !(await _menuPerm.Has("/Certificate/StaffIdCard", "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit staff ID cards." });

            var res = await _staffIdCardClient.Upsert(request);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStaffIDCard([FromBody] List<int> id)
        {
            if (!(await _menuPerm.Has("/Certificate/StaffIdCard", "Delete")).Data)
                return Json(new { success = false, message = "You do not have permission to delete staff ID cards." });

            var res = await _staffIdCardClient.Delete(id);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStaffIDCardStatus(int id, bool isActive)
        {
            if (!(await _menuPerm.Has("/Certificate/StaffIdCard", "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit staff ID cards." });

            var res = await _staffIdCardClient.ToggleStatus(id, isActive);
            return Json(res);
        }

        // --- Student Certificates ---
        [HttpGet]
        public async Task<IActionResult> GetAllStudentCertificates()
        {
            var res = await _certificateClient.GetAll();
            return Json(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentCertificateByID(int id)
        {
            var res = await _certificateClient.GetByID(id);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> UpsertStudentCertificate([FromBody] StudentCertificateUpsertRequest request)
        {
            var isCreate = request.CertificateID <= 0;
            if (isCreate && !(await _menuPerm.Has("/Certificate/StudentCertificate", "Add")).Data)
                return Json(new { success = false, message = "You do not have permission to add student certificates." });
            if (!isCreate && !(await _menuPerm.Has("/Certificate/StudentCertificate", "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit student certificates." });

            var res = await _certificateClient.Upsert(request);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudentCertificate([FromBody] List<int> id)
        {
            if (!(await _menuPerm.Has("/Certificate/StudentCertificate", "Delete")).Data)
                return Json(new { success = false, message = "You do not have permission to delete student certificates." });

            var res = await _certificateClient.Delete(id);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStudentCertificateStatus(int id, bool isActive)
        {
            if (!(await _menuPerm.Has("/Certificate/StudentCertificate", "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit student certificates." });

            var res = await _certificateClient.ToggleStatus(id, isActive);
            return Json(res);
        }

        [HttpGet]
        public async Task<IActionResult> GenerateStudentCertificate(int studentId, int certificateId)
        {
            var res = await _certificateClient.Generate(studentId, certificateId);
            return Json(res);
        }

        #endregion
    }
}
