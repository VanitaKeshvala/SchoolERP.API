using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Shared.Models;
using SchoolERP.Net.Services;
using SchoolERP.Net.Services.Clients;
using SchoolERP.Net.Helpers;
using SchoolERP.Shared.Models.Common;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using static System.Collections.Specialized.BitVector32;

namespace SchoolERP.Net.Controllers
{
    /// <summary>
    /// This controller manages the school's front office activities, such as visitor logs, complaints, phone calls, and mail (postal) services.
    /// </summary>
    public class FrontOfficeController : BaseController
    {
        private readonly IFrontOfficeClientService _client;
        private readonly IUserMenuPermissionClientService _menuPerm;
        private readonly IClassClientService _classClient;
        private readonly ISectionClientService _sectionClient;
        private readonly IStudentInformationClientService _studentClient;
        private readonly IHumanResourceClientService _hrClient;
        private readonly ICompanyClientService _companyService;
        private readonly IConfiguration _configuration;
        private readonly IPhotoUploadService _photoService;
        private readonly IWebHostEnvironment _environment;
        private const string MenuPath = "/FrontOffice/Setup";
        private const string ComplaintMenuPath = "/FrontOffice/Complaint";
        private readonly ISessionClientService _sessionService;
        public FrontOfficeController(
            IFrontOfficeClientService client,
            IUserMenuPermissionClientService menuPerm,
            IClassClientService classClient,
            ISectionClientService sectionClient,
            IStudentInformationClientService studentClient,
            IConfiguration configuration,
            IHumanResourceClientService hrClient, ISessionClientService sessionService, PermissionHelper permHelper, 
            ICompanyClientService companyService, IPhotoUploadService photoService, IWebHostEnvironment environment) : base(permHelper)
        {
            _client = client;
            _menuPerm = menuPerm;
            _classClient = classClient;
            _sectionClient = sectionClient;
            _studentClient = studentClient;
            _hrClient = hrClient;
            _sessionService = sessionService;
            _companyService = companyService;
            _configuration = configuration;
            _photoService = photoService;
            _environment = environment;
        }

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
        private int? GetStaffID()
        {
            var staffClaim = User.FindFirst("StaffID")?.Value;
            return int.TryParse(staffClaim, out var staffId) ? staffId : null;
        }
        /// <summary>
        /// Loads the master-data management page — fetches all Purposes,
        /// Complaint Types, Sources, References in one shot so an admin can manage all four from a single screen.
        /// </summary>
        public async Task<IActionResult> Setup()
        {
            var model = new FrontOfficeSetupPageViewModel();
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/FrontOffice/Setup"
               );
                // Step 1: Gather all categories like 'Visitor Purposes', 'Complaint Types', 'Sources', and 'References'.
                var purposes = await _client.GetAllPurposesAsync();
                var complaintTypes = await _client.GetAllComplaintTypesAsync();
                var sources = await _client.GetAllSourcesAsync();
                var references = await _client.GetAllReferencesAsync();

                // Step 2: Organize these lists so they can be shown on the setup management page.
                model = new FrontOfficeSetupPageViewModel
                {
                    Purposes = purposes.Success ? purposes.Data : new List<MstFOPurposeViewModel>(),
                    ComplaintTypes = complaintTypes.Success ? complaintTypes.Data : new List<MstFOComplaintTypeViewModel>(),
                    Sources = sources.Success ? sources.Data : new List<MstFOSourceViewModel>(),
                    References = references.Success ? references.Data : new List<MstFOReferenceViewModel>()
                };
                model.Permissions = perms;
                // Step 3: Open the 'Setup' page for the user.
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
           
        }

        /// <summary>
        ///  GetReference(id)Fetches one record by ID to pre-fill the "Edit" modal. Each checks Edit permission before returning data.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPurpose(int id)
        {
            try
            {
                if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                var r = await _client.GetPurposeByIDAsync(id);
                if (!r.Success) return Json(new { success = false, message = r.Message });
                return Json(new { success = true, data = r.Data });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = true, message = ex.Message });
            }
            
        }

        /// <summary>
        /// One upsert endpoint per entity. PurposeID <= 0 (etc.) decides Create vs Edit, 
        /// and checks the matching Add or Edit permission accordingly before calling the service layer.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SavePurpose([FromBody] MstFOPurposeUpsertRequest req)
        {
            try
            {
                bool isCreate = req.PurposeID <= 0;
                if (isCreate && !(await _menuPerm.Has(MenuPath, "Add")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                if (!isCreate && !(await _menuPerm.Has(MenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                var r = await _client.UpsertPurposeAsync(req);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = ex.Message });
            }
           
        }

        /// <summary>
        /// Bulk delete — accepts List<int> so multiple rows can be removed in one call. Checks Delete permission.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeletePurpose([FromBody] List<int> id)
        {
            try
            {
                if (!(await _menuPerm.Has(MenuPath, "Delete")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                var r = await _client.DeletePurposeAsync(id);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = ex.Message });
            }
            
        }

        /// <summary>
        /// Flips a record's active/inactive flag (e.g. an Active/Inactive switch in a table row) without a full edit round-trip. Checks Edit permission.
        /// </summary>
        //[HttpPost]
        //public async Task<IActionResult> TogglePurposeStatus(int id, bool isActive)
        //{
        //    try
        //    {
        //        if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
        //            return Json(new { success = false, message = "Permission denied." });
        //        var r = await _client.TogglePurposeStatusAsync(id, isActive);
        //        return Json(new { success = r.Success, message = r.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Error"] = ex.Message;
        //        return Json(new { success = false, message = ex.Message });
        //    }
            
        //}

        /// <summary>
        ///  GetReference(id)Fetches one record by ID to pre-fill the "Edit" modal. Each checks Edit permission before returning data.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetComplaintType(int id)
        {
            try
            {
                if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                var r = await _client.GetComplaintTypeByIDAsync(id);
                if (!r.Success) return Json(new { success = false, message = r.Message });
                return Json(new { success = true, data = r.Data });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = ex.Message });
            }
            
        }

        /// <summary>
        /// One upsert endpoint per entity. PurposeID <= 0 (etc.) decides Create vs Edit, 
        /// and checks the matching Add or Edit permission accordingly before calling the service layer.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveComplaintType([FromBody] MstFOComplaintTypeUpsertRequest req)
        {
            try
            {
                bool isCreate = req.ComplaintTypeID <= 0;
                if (isCreate && !(await _menuPerm.Has(MenuPath, "Add")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                if (!isCreate && !(await _menuPerm.Has(MenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                var r = await _client.UpsertComplaintTypeAsync(req);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = ex.Message });
            }
            
        }

        /// <summary>
        /// Bulk delete — accepts List<int> so multiple rows can be removed in one call. Checks Delete permission.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DeleteComplaintType([FromBody] List<int> id)
        {
            try
            {
                if (!(await _menuPerm.Has(MenuPath, "Delete")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                var r = await _client.DeleteComplaintTypeAsync(id);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = ex.Message });
            }
            
        }

        /// <summary>
        /// ToggleComplaintTypeStatus / ToggleSourceStatus / ToggleReferenceStatus(id, isActive)Flips a record's active/inactive flag (e.g. an Active/Inactive switch in a table row) without a full edit round-trip. Checks Edit permission.
        /// </summary>
        //[HttpPost]
        //public async Task<IActionResult> ToggleComplaintTypeStatus(int id, bool isActive)
        //{
        //    try
        //    {
        //        if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
        //            return Json(new { success = false, message = "Permission denied." });
        //        var r = await _client.ToggleComplaintTypeStatusAsync(id, isActive);
        //        return Json(new { success = r.Success, message = r.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Error"] = ex.Message;
        //        return Json(new { success = false, message = ex.Message });
        //    }
            
        //}

        // ─── SOURCE ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetSource(int id)
        {
            try
            {
                if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                var r = await _client.GetSourceByIDAsync(id);
                if (!r.Success) return Json(new { success = false, message = r.Message });
                return Json(new { success = true, data = r.Data });
            }
            catch (Exception ex) 
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = ex.Message });
            }
            
        }

        /// <summary>
        /// One upsert endpoint per entity. PurposeID <= 0 (etc.) decides Create vs Edit, 
        /// and checks the matching Add or Edit permission accordingly before calling the service layer.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveSource([FromBody] MstFOSourceUpsertRequest req)
        {
            try
            {
                bool isCreate = req.SourceID <= 0;
                if (isCreate && !(await _menuPerm.Has(MenuPath, "Add")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                if (!isCreate && !(await _menuPerm.Has(MenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                var r = await _client.UpsertSourceAsync(req);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = ex.Message });
            }
           
        }

        /// <summary>
        /// Bulk delete — accepts List<int> so multiple rows can be removed in one call. Checks Delete permission.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteSource([FromBody] List<int> id)
        {
            try
            {
                if (!(await _menuPerm.Has(MenuPath, "Delete")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                var r = await _client.DeleteSourceAsync(id);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = ex.Message });
            }
            
        }

        /// <summary>
        /// Flips a record's active/inactive flag (e.g. an Active/Inactive switch in a table row) without a full edit round-trip. Checks Edit permission.
        /// </summary>
        //[HttpPost]
        //public async Task<IActionResult> ToggleSourceStatus(int id, bool isActive)
        //{
        //    try
        //    {
        //        if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
        //            return Json(new { success = false, message = "Permission denied." });
        //        var r = await _client.ToggleSourceStatusAsync(id, isActive);
        //        return Json(new { success = r.Success, message = r.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Error"] = ex.Message;
        //        return Json(new { success = false, message = ex.Message });
        //    }
            
        //}

        // ─── REFERENCE ──────────────────────────────────────────
        /// <summary>
        /// Fetches one record by ID to pre-fill the "Edit" modal. Each checks Edit permission before returning data.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetReference(int id)
        {
            try
            {
                if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                var r = await _client.GetReferenceByIDAsync(id);
                if (!r.Success) return Json(new { success = false, message = r.Message });
                return Json(new { success = true, data = r.Data });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = ex.Message });
            }
            
        }
        /// <summary>
        /// One upsert endpoint per entity. PurposeID <= 0 (etc.) decides Create vs Edit,
        /// and checks the matching Add or Edit permission accordingly before calling the service layer.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveReference([FromBody] MstFOReferenceUpsertRequest req)
        {
            try
            {
                bool isCreate = req.ReferenceID <= 0;
                if (isCreate && !(await _menuPerm.Has(MenuPath, "Add")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                if (!isCreate && !(await _menuPerm.Has(MenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                var r = await _client.UpsertReferenceAsync(req);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = ex.Message });
            }
            
        }

        /// <summary>
        /// Bulk delete — accepts List<int> so multiple rows can be removed in one call. Checks Delete permission.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteReference([FromBody] List<int> id)
        {
            try
            {
                if (!(await _menuPerm.Has(MenuPath, "Delete")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                var r = await _client.DeleteReferenceAsync(id);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = ex.Message });
            }
            
        }

        /// <summary>
        /// Flips a record's active/inactive flag (e.g. an Active/Inactive switch in a table row) without a full edit round-trip. Checks Edit permission.
        /// </summary>
        //[HttpPost]
        //public async Task<IActionResult> ToggleReferenceStatus(int id, bool isActive)
        //{
        //    try
        //    {
        //        if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
        //            return Json(new { success = false, message = "Permission denied." });
        //        var r = await _client.ToggleReferenceStatusAsync(id, isActive);
        //        return Json(new { success = r.Success, message = r.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Error"] = ex.Message;
        //        return Json(new { success = false, message = ex.Message });
        //    }
            
        //}

        /// <summary>
        /// Shows the main 'Complaint' management page where you can see all student or parent complaints and their status.
        /// </summary>
        public async Task<IActionResult> Complaint(int? pageIndex,
        int? pageSize,
        string? search,
        int? companyId,
        int? sessionID,
        int? complaintTypeID,
        int? sourceID)
        {
            var model = new FOComplaintPageViewModel();
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/FrontOffice/Complaint"
               );

                var request = new ComplaintSearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionID = sessionID ?? await GetSessionId(),
                    ComplaintTypeID = complaintTypeID ?? null,
                    SourceID=sourceID??null
                };

                // Step 1: Fetch all recorded complaints, types of complaints, and where they came from (sources).
                //var complaints = await _client.GetAllComplaintsAsync();
                var complaints = _client.GetAllComplaintsWithPageAsync(request);
                var complaintTypes = await _client.GetAllComplaintTypesAsync();
                var sources = await _client.GetAllSourcesAsync();


                var sessionTask = _sessionService.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();




                await Task.WhenAll(complaints, sessionTask, companiesTask);

                var pagedResult = await complaints;


                // Step 2: Combine this information to be shown on the complaint management screen.
                model = new FOComplaintPageViewModel
                {
                    Complaints = pagedResult.Success ? pagedResult.Data.Data : new List<FOComplaintViewModel>(),
                    ComplaintTypes = complaintTypes.Success ? complaintTypes.Data : new List<MstFOComplaintTypeViewModel>(),
                    Sources = sources.Success ? sources.Data : new List<MstFOSourceViewModel>(),
                                        
                    Companies = (await companiesTask).Data ?? new(),
                    Sessions = (await sessionTask).Data ?? new(),
                    TotalRecords = pagedResult.Data.TotalRecords,
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,
                    SearchTerm = search,
                    CompanyId = companyId,
                    ComplaintTypeID = complaintTypeID,
                    SourceID=sourceID
                };
                model.Permissions = perms;
                // Step 3: Open the 'Complaint' management page.
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
            
        }


        public async Task<IActionResult> AddComplaint(int? id)
        {
            var model = new FOComplaintAddPageViewModel();
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/FrontOffice/PostalReceive"
               );

                var complaintTypes = await _client.GetAllComplaintTypesAsync();
                var sources = await _client.GetAllSourcesAsync();
                if (id.HasValue && id.Value > 0)
                {
                    var response = await _client.GetComplaintByIDAsync(id.Value);
                    if (response.Success)
                    {
                        model.Complaints = response.Data;
                        model.EditComplaints = response.Data;
                    }
                }
                else
                {
                    model.EditComplaints = null;
                }
                model.ComplaintTypes = complaintTypes.Data;
                model.Sources = sources.Data;
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
        }

        /// <summary>
        /// Fetches one record by ID to pre-fill the "Edit" modal. Each checks Edit permission before returning data.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetComplaint(int id)
        {
            try
            {
                if (!(await _menuPerm.Has(ComplaintMenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                var r = await _client.GetComplaintByIDAsync(id);
                return Json(new { success = r.Success, message = r.Message, data = r.Data });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = ex.Message });
            }
           
        }
        /// <summary>
        /// One upsert endpoint per entity. PurposeID <= 0 (etc.) decides Create vs Edit, 
        /// and checks the matching Add or Edit permission accordingly before calling the service layer.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveComplaint([FromBody] FOComplaintUpsertRequest req)
        {
            try
            {
                bool isCreate = req.ComplaintID <= 0;
                if (isCreate && !(await _menuPerm.Has(ComplaintMenuPath, "Add")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                if (!isCreate && !(await _menuPerm.Has(ComplaintMenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                var r = await _client.UpsertComplaintAsync(req);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = ex.Message });
            }
            
        }
        /// <summary>
        /// Bulk delete — accepts List<int> so multiple rows can be removed in one call. Checks Delete permission.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteComplaint([FromBody] List<int> id)
        {
            try
            {
                if (!(await _menuPerm.Has(ComplaintMenuPath, "Delete")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                var r = await _client.DeleteComplaintAsync(id);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = ex.Message });
            }
            
        }
        /// <summary>
        /// Flips a record's active/inactive flag (e.g. an Active/Inactive switch in a table row) without a full edit round-trip. Checks Edit permission.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ToggleComplaintStatus([FromBody] StatusUpdateRequest request)
        {
            try
            {
                if (!(await _menuPerm.Has(ComplaintMenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "Permission denied." });
                var r = await _client.ToggleComplaintStatusAsync(request);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = ex.Message });
            }
           
        }

        /// <summary>
        /// Shows the 'Postal Receive' page, which tracks all the physical mail or packages the school has received.
        /// </summary>
        public async Task<IActionResult> PostalReceive(int? pageIndex,
        int? pageSize,
        string? search,
        int? companyId,
        int? sessionID)
        {
            var model = new FOPostalReceivePageViewModel();
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/FrontOffice/PostalReceive"
               );

                var request = new ClassSearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionID = sessionID ?? await GetSessionId()
                };

                var postalReceiveResponse = _client.GetAllPostalReceiveWithPageAsync(request);

                var sessionTask = _sessionService.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();

                await Task.WhenAll(postalReceiveResponse, sessionTask, companiesTask);

                var pagedResult = await postalReceiveResponse;
                var res = await _client.GetAllPostalReceivesAsync();
                model = new FOPostalReceivePageViewModel
                {
                    Items = pagedResult.Success ? pagedResult.Data.Data : new List<FOPostalReceiveViewModel>(),
                    Companies = (await companiesTask).Data ?? new(),
                    Sessions = (await sessionTask).Data ?? new(),
                    TotalRecords = pagedResult.Data.TotalRecords,
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,
                    SearchTerm = search,
                    CompanyId = companyId,

                };
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
            
        }


        public async Task<IActionResult> AddPostalReceive(int? id)
        {
            var model = new FOPostalReceiveAddPageViewModel();
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/FrontOffice/PostalReceive"
               );


                if (id.HasValue && id.Value > 0)
                {
                    var response = await _client.GetPostalReceiveByIDAsync(id.Value);
                    if (response.Success)
                    {
                        model.Items = response.Data;
                        model.EditItems = response.Data;
                    }
                }
                else
                {
                    model.EditItems = null;
                }
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
        }

        /// <summary>
        /// Fetches one record by ID to pre-fill the "Edit" modal. Each checks Edit permission before returning data.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPostalReceive(int id)
        {
            try
            {
                var r = await _client.GetPostalReceiveByIDAsync(id);
                return Json(new { success = r.Success, message = r.Message, data = r.Data });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = ex.Message });
            }
            
        }
        /// <summary>
        /// One upsert endpoint per entity. PurposeID <= 0 (etc.) decides Create vs Edit, 
        /// and checks the matching Add or Edit permission accordingly before calling the service layer.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SavePostalReceive([FromForm] FOPostalReceiveUpsertRequest req, IFormFile? attachmentFile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return Json(new { success = false, message = "Validation Error: " + errors });
                }

                if (attachmentFile != null && attachmentFile.Length > 0)
                {
                    long MaxFileSizeMB = _configuration.GetValue<long>("FileUploadSettings:MaxFileSizeMB");
                    long maxBytes = MaxFileSizeMB * 1024L * 1024L;

                    if (attachmentFile.Length > maxBytes)
                    {
                        return Json(new { success = false, message = $"File size exceeds the {MaxFileSizeMB} MB limit." });
                    }

                    req.FileName = attachmentFile.FileName;
                    req.FileType = attachmentFile.ContentType;

                }
                req.CompanyID = await GetCompanyId();
                req.SessionID = await GetSessionId();
                var r = await _client.UpsertPostalReceiveAsync(req);
                if (r.Data?.Result?.Result == 1)
                {
                    int? postalReceiveID = r.Data.Result.PostalReceiveID;
                    if (attachmentFile != null && attachmentFile.Length > 0)
                    {
                        using var memoryStream = new MemoryStream();
                        await attachmentFile.CopyToAsync(memoryStream);

                        byte[] fileBytes = memoryStream.ToArray();
                        PhotoUploadResult photoResult = new PhotoUploadResult();
                        if (req.FileName != null)
                        {
                            photoResult = await _photoService.SaveBase64PhotoAsync(
                                Convert.ToBase64String(fileBytes),
                                req.FileName ?? "photo.jpg",
                                PhotoModule.PostalReceive,
                                FolderNameModule.Documents,
                                postalReceiveID.Value
                            );
                            var attchment = new FOPostalReceiveAttachmentUpsertRequest
                            {
                                PostalReceiveID = postalReceiveID.Value,
                                Attachment = photoResult.PhotoUrl,
                                FileName = attachmentFile.FileName,
                                FileType = attachmentFile.ContentType

                            };
                            var attachment = await _client.UpsertPostalReceiveAttachmentFileAsync(attchment);
                        }
                    }
                }

                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        /// <summary>
        /// Bulk delete — accepts List<int> so multiple rows can be removed in one call. Checks Delete permission.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeletePostalReceive([FromBody] List<int> id)
        {
            try
            {
                var r = await _client.DeletePostalReceiveAsync(id);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
            
        }
        /// <summary>
        /// Flips a record's active/inactive flag (e.g. an Active/Inactive switch in a table row) without a full edit round-trip. Checks Edit permission.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> TogglePostalReceiveStatus([FromBody] StatusUpdateRequest request)
        {
            try
            {
                var r = await _client.TogglePostalReceiveStatusAsync(request);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
            
        }

        /// <summary>
        /// Streams the stored byte array back as a downloadable file (File(bytes, contentType, fileName)), 
        /// reconstructing the original filename/content-type saved at upload time. Returns 404 if there's no attachment.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DownloadAttachment(int id)
        {
            try
            {
                var r = await _client.GetPostalReceiveByIDAsync(id);
                if (!r.Success || r.Data?.Attachment == null) return NotFound();

                string contentType = r.Data.FileType ?? "application/octet-stream";
                string fileName = r.Data.FileName ?? $"attachment_{id}";

                return File(r.Data.Attachment, contentType, fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = "Error: " + ex.Message }); 
            }
            
        }

        /// <summary>
        /// Shows the 'Postal Dispatch' page, which keeps a record of all the mail or packages the school has sent out.
        /// </summary>
        public async Task<IActionResult> PostalDispatch(int? pageIndex,
        int? pageSize,
        string? search,
        int? companyId,
        int? sessionID)
        {
            var model = new FOPostalDispatchPageViewModel();
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/FrontOffice/PostalDispatch"
               );

                var request = new FOPostalDispatchSearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionID = sessionID ?? await GetSessionId()
                };

                var res = _client.GetAllPostalDispatchesWithPageIndexAsync(request);
                var sessionTask = _sessionService.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();
                await Task.WhenAll(res, sessionTask, companiesTask);

                var pagedResult = await res;

                
                model = new FOPostalDispatchPageViewModel
                {
                    Items = pagedResult.Success ? pagedResult.Data.Data : new List<FOPostalDispatchViewModel>(),
                    Companies = (await companiesTask).Data ?? new(),
                    Sessions = (await sessionTask).Data ?? new(),
                    TotalRecords = pagedResult.Data.TotalRecords,
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,
                    SearchTerm = search,
                    CompanyId = companyId,
                    SessionId = sessionID
                };
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
            
        }


        public async Task<IActionResult> AddPostalDispatch(int? id)
        {
            var model = new FOPostalDispatchAddPageViewModel();
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/FrontOffice/VisitorBook"
               );


                if (id.HasValue && id.Value > 0)
                {
                    var response = await _client.GetPostalDispatchByIDAsync(id.Value);
                    if (response.Success)
                    {
                        model.Items = response.Data;
                        model.EditItems = response.Data;
                    }
                }
                else
                {
                    model.EditItems = null;
                }
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
        }

        /// <summary>
        /// Fetches one record for editing. Notably these two don't check permissions
        /// before returning data — worth a look if that's intentional, since the sibling actions elsewhere in the file do check.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPostalDispatch(int id)
        {
            try
            {
                var r = await _client.GetPostalDispatchByIDAsync(id);
                return Json(new { success = r.Success, message = r.Message, data = r.Data });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> SavePostalDispatch([FromForm] FOPostalDispatchUpsertRequest req, IFormFile? attachmentFile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return Json(new { success = false, message = "Validation Error: " + errors });
                }

                if (attachmentFile != null && attachmentFile.Length > 0)
                {
                    long MaxFileSizeMB = _configuration.GetValue<long>("FileUploadSettings:MaxFileSizeMB");
                    long maxBytes = MaxFileSizeMB * 1024L * 1024L;

                    if (attachmentFile.Length > maxBytes)
                    {
                        return Json(new { success = false, message = $"File size exceeds the {MaxFileSizeMB} MB limit." });
                    }

                    req.FileName = attachmentFile.FileName;
                    req.FileType = attachmentFile.ContentType;

                }
                var r = await _client.UpsertPostalDispatchAsync(req);
                if (r.Data?.Result?.Result == 1)
                {
                    int? postalDispatchId = r.Data.Result.POSTALDISPATCHID;
                    if (attachmentFile != null && attachmentFile.Length > 0)
                    {
                        using var memoryStream = new MemoryStream();
                        await attachmentFile.CopyToAsync(memoryStream);

                        byte[] fileBytes = memoryStream.ToArray();
                        PhotoUploadResult photoResult = new PhotoUploadResult();
                        if (req.FileName != null)
                        {
                            photoResult = await _photoService.SaveBase64PhotoAsync(
                                Convert.ToBase64String(fileBytes),
                                req.FileName ?? "photo.jpg",
                                PhotoModule.PostalDispatch,
                                FolderNameModule.Documents,
                                postalDispatchId.Value
                            );
                            var attchment = new FOPostalDispatchAttachmentUpsertRequest
                            {
                                PostalDispatchID = postalDispatchId.Value,
                                Attachment = photoResult.PhotoUrl,
                                FileName = attachmentFile.FileName,
                                FileType = attachmentFile.ContentType

                            };
                            var attachment = await _client.UpsertPostalDispatchAttachmentFileAsync(attchment);
                        }
                    }
                }
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeletePostalDispatch([FromBody] List<int> id)
        {
            try
            {
                var r = await _client.DeletePostalDispatchAsync(id);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> TogglePostalDispatchStatus([FromBody] StatusUpdateRequest request)
        {
            try
            {
                var r = await _client.TogglePostalDispatchStatusAsync(request);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
           
        }

        [HttpGet]
        public async Task<IActionResult> DownloadDispatchAttachment(int id)
        {
            try
            {
                //var r = await _client.GetPostalDispatchByIDAsync(id);
                //if (!r.Success || r.Data?.Attachment == null) return NotFound();

                //string contentType = r.Data.FileType ?? "application/octet-stream";
                //string fileName = r.Data.FileName ?? $"dispatch_attachment_{id}";

                //return File(r.Data.Attachment, contentType, fileName);


                var r = await _client.GetPostalDispatchByIDAsync(id);

                if (!r.Success || string.IsNullOrWhiteSpace(r.Data?.Attachment))
                    return NotFound();

                // Full physical path
                var filePath = Path.Combine(
                    _environment.WebRootPath,
                    r.Data.Attachment.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
                );

                if (!System.IO.File.Exists(filePath))
                    return NotFound("File not found.");

                var fileName = Path.GetFileName(filePath);

                return PhysicalFile(
                    filePath,
                    "application/octet-stream",
                    fileName);

            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = "Error: " + ex.Message }); 
            }
            
        }

        /// <summary>
        /// Shows the 'Phone Call Log' page, where staff can record details of incoming and outgoing calls.
        /// </summary>
        public async Task<IActionResult> PhoneCallLog(int? pageIndex,
        int? pageSize,
        string? search,
        int? companyId,
        int? sessionID,
        string? callType)
        {
            var model = new FOPhoneCallLogPageViewModel();
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/FrontOffice/PhoneCallLog"
               );

                var request = new FOPhoneCallLogSearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionID = sessionID ?? await GetSessionId(),
                    CallType = callType ?? null
                };

                var res = await _client.GetAllPhoneCallLogsAsync();

                var phoneCallLogsResponse = _client.GetAllPhoneCallLogsWithPageAsync(request);

                var sessionTask = _sessionService.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();

                await Task.WhenAll(phoneCallLogsResponse, sessionTask, companiesTask);

                var pagedResult = await phoneCallLogsResponse;

               
                model = new FOPhoneCallLogPageViewModel
                {
                    Items = pagedResult.Success ? pagedResult.Data.Data : new List<FOPhoneCallLogViewModel>(),
                    Companies = (await companiesTask).Data ?? new(),
                    Sessions = (await sessionTask).Data ?? new(),
                    TotalRecords = pagedResult.Data.TotalRecords,
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,
                    SearchTerm = search,
                    CompanyId = companyId,
                    CallType = callType
                };
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
            
        }

        public async Task<IActionResult> AddPhoneCallLog(int? id) 
        {
            var model = new FOPhoneCallLogAddPageViewModel();
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/FrontOffice/VisitorBook"
               );
               
               
                if (id.HasValue && id.Value > 0)
                {
                    var response = await _client.GetPhoneCallLogByIDAsync(id.Value);
                    if (response.Success)
                    {
                        model.Items = response.Data;
                        model.EditItems = response.Data;
                    }
                }
                else
                {
                    model.EditItems = null;
                }
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetPhoneCallLog(int id)
        {
            try
            {
                var r = await _client.GetPhoneCallLogByIDAsync(id);
                return Json(new { success = r.Success, message = r.Message, data = r.Data });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
           
        }

        [HttpPost]
        public async Task<IActionResult> SavePhoneCallLog([FromBody] FOPhoneCallLogUpsertRequest req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return Json(new { success = false, message = "Validation Error: " + errors });
                }
                req.CompanyID = await GetCompanyId();
                req.SessionID = await GetSessionId();
                var r = await _client.UpsertPhoneCallLogAsync(req);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
           
        }

        [HttpPost]
        public async Task<IActionResult> DeletePhoneCallLog([FromBody] List<int> id)
        {
            try
            {
                var r = await _client.DeletePhoneCallLogAsync(id);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
           
        }

        [HttpPost]
        public async Task<IActionResult> TogglePhoneCallLogStatus([FromBody] StatusUpdateRequest request)
        {
            try
            {
                var r = await _client.TogglePhoneCallLogStatusAsync(request);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
            
        }

        /// <summary>
        /// Loads the visitor list page. Also pulls in Purposes, Classes, and Staff, because the "who did they visit" / "why" 
        /// fields on the visitor form need those as dropdown sources.
        /// </summary>
        public async Task<IActionResult> VisitorBook(int? pageIndex,
        int? pageSize,
        string? search,
        int? companyId,
        int? sessionID,
        int? staffId,
        int? studentId,
        int? purpose)
        {
            var model = new FOVisitorBookPageViewModel();
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/FrontOffice/VisitorBook"
               );

                var request = new FOVisitorBookSerchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionID = sessionID ?? await GetSessionId(),
                    StaffID = staffId ?? GetStaffID(),
                    StudentID = studentId ?? null,
                    Purposes=purpose
                };

                var sessionId = await GetSessionId();
                var visitors = _client.GetAllVisitorsWithPageIndexAsync(request);
                var purposes = await _client.GetAllPurposesAsync();
                var classes = await _classClient.GetAllAsync();
                var staff = await _hrClient.GetAllStaffAsync(request.CompanyID,request.SessionID,request.StaffID);
                var sessionTask = _sessionService.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();

                await Task.WhenAll(visitors, sessionTask, companiesTask);
                var pagedResult = await visitors;
                model = new FOVisitorBookPageViewModel
                {
                    Visitors = pagedResult.Success ? pagedResult.Data.Data : new List<FOVisitorBookViewModel>(),
                    Purposes = purposes.Success ? purposes.Data : new List<MstFOPurposeViewModel>(),
                    Classes = classes.Success ? classes.Data : new List<MstClassViewModel>(),
                    Staff = staff.Success ? staff.Data : new List<HRStaffViewModel>(),

                    Companies = (await companiesTask).Data ?? new(),
                    Sessions = (await sessionTask).Data ?? new(),
                    TotalRecords = pagedResult.Data.TotalRecords,
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,
                    SearchTerm = search,
                    CompanyId = companyId,
                    SessionId = sessionId,
                    StaffId=staffId,
                    StudentId=studentId
                };
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
           
        }

        [HttpGet]
        public async Task<IActionResult> GetSections(int classId)
        {
            try
            {
                var r = await _sectionClient.GetByClassAsync(classId);
                return Json(new { success = r.Success, data = r.Data });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
            
        }

        [HttpGet]
        public async Task<IActionResult> GetStudents(int classId, int sectionId)
        {
            try
            {
                var r = await _studentClient.GetStudentListAsync(classId, sectionId);
                return Json(new { success = r.Success, data = r.Data });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
            
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStaff()
        {
            try
            {
                var sessionId = await GetSessionId();
                var companyId = await GetCompanyId();
                var staffId = GetStaffID();
                var r = await _hrClient.GetAllStaffAsync(companyId,sessionId, staffId);
                return Json(new { success = r.Success, data = r.Data });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
            
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentByID(int id)
        {
            try
            {
                var r = await _studentClient.GetStudentByIDAsync(id);
                return Json(new { success = r.Success, data = r.Data });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
           
        }

        [HttpGet]
        public async Task<IActionResult> GetVisitor(int id)
        {
            try
            {
                var r = await _client.GetVisitorByIDAsync(id);
                return Json(new { success = r.Success, message = r.Message, data = r.Data });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
           
        }

        [HttpPost]
        public async Task<IActionResult> SaveVisitor([FromForm] FOVisitorBookUpsertRequest req, IFormFile? attachmentFile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return Json(new { success = false, message = "Validation Error: " + errors });
                }

                if (attachmentFile != null && attachmentFile.Length > 0)
                {
                    long MaxFileSizeMB = _configuration.GetValue<long>("FileUploadSettings:MaxFileSizeMB");
                    long maxBytes = MaxFileSizeMB * 1024L * 1024L;

                    if (attachmentFile.Length > maxBytes)
                    {
                        return Json(new { success = false, message = $"File size exceeds the {MaxFileSizeMB} MB limit." });
                    }

                    req.FileName = attachmentFile.FileName;
                    req.FileType = attachmentFile.ContentType;
                                      
                }
                var r = await _client.UpsertVisitorAsync(req);
                if (r.Data?.Result?.Result == 1) 
                {
                    int? visitorBookId = r.Data.Result.VISITORBOOKID;
                    if (attachmentFile != null && attachmentFile.Length > 0)
                    {
                        using var memoryStream = new MemoryStream();
                        await attachmentFile.CopyToAsync(memoryStream);

                        byte[] fileBytes = memoryStream.ToArray();
                        PhotoUploadResult photoResult = new PhotoUploadResult();
                        if (req.FileName != null)
                        {
                            photoResult = await _photoService.SaveBase64PhotoAsync(
                                Convert.ToBase64String(fileBytes),
                                req.FileName ?? "photo.jpg",
                                PhotoModule.VisitorBook,
                                FolderNameModule.Documents,
                                visitorBookId.Value
                            );
                            var attchment = new FOVisitorBookAttachmentUpsertRequest
                            {
                                VisitorBookID = visitorBookId.Value,
                                Attachment = photoResult.PhotoUrl,
                                FileName = attachmentFile.FileName,
                                FileType = attachmentFile.ContentType

                            };
                            var attachment = await _client.UpsertVisitorAttachmentFileAsync(attchment);
                        }
                    }
                }
                
                    return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVisitor([FromBody] List<int> id)
        {
            try
            {
                var r = await _client.DeleteVisitorAsync(id);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
         
        }

        [HttpPost]
        public async Task<IActionResult> ToggleVisitorStatus([FromBody] StatusUpdateRequest request)
        {
            try
            {
                var r = await _client.ToggleVisitorStatusAsync(request);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
           
        }

        /// <summary>
        /// Shows the 'Admission Inquiry' page, where the school records potential students who have asked about joining.
        /// Users can filter inquiries by date, source (like 'Website'), or class.
        /// </summary>
        public async Task<IActionResult> AdmissionInquiry(
            DateTime? fromDate = null,
            DateTime? toDate = null, 
            int? sourceId = 0,
            int? classId = 0, 
            string? status = null,
            int? pageIndex=null,
        int? pageSize = null,
        string? search = null,
        int? companyId = null,
        int? sessionID = null)
        {
            var model = new FOAdmissionInquiryPageViewModel();
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/FrontOffice/AdmissionInquiry"
               );
                var req = new EnquirySearchRequest 
                {
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionID = sessionID ?? await GetSessionId(),
                    FromDate=fromDate??null,
                    ToDate=toDate??null,
                    SourceID=sourceId??null,
                    ClassID=classId??null,
                    Status=status ?? null,
                    SearchKeyword=search??null,
                    PageNumber=pageIndex,
                    PageSize=pageSize

                };
                var sessionId = await GetSessionId();
                var staffId = GetStaffID();
                var inquiries = _client.GetAllAdmissionInquiriesWithPageIndexAsync(req);
                var classes = await _classClient.GetAllAsync(false,sessionId);
                var sources = await _client.GetAllSourcesAsync();
                var references = await _client.GetAllReferencesAsync();
                var staff = await _hrClient.GetAllStaffAsync(req.CompanyID.Value,req.SessionID.Value,staffId);

                var sessionTask = _sessionService.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();

                await Task.WhenAll(inquiries, sessionTask, companiesTask);

                var pagedResult = await inquiries;

                model = new FOAdmissionInquiryPageViewModel
                {
                    Inquiries = pagedResult.Success ? pagedResult.Data.Data : new List<FOAdmissionInquiryViewModel>(),
                    Classes = classes.Success ? classes.Data : new List<MstClassViewModel>(),
                    Sources = sources.Success ? sources.Data : new List<MstFOSourceViewModel>(),
                    References = references.Success ? references.Data : new List<MstFOReferenceViewModel>(),
                    Staff = staff.Success ? staff.Data : new List<HRStaffViewModel>(),
                    Companies = (await companiesTask).Data ?? new(),
                    Sessions = (await sessionTask).Data ?? new(),
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,
                    SearchTerm = search,
                    CompanyId = companyId,
                    SessionId = sessionId
                };
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
            
        }

        [HttpGet]
        public async Task<IActionResult> GetInquiry(int id)
        {
            try
            {
                var r = await _client.GetAdmissionInquiryByIDAsync(id);
                return Json(new { success = r.Success, message = r.Message, data = r.Data });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
           
        }

        [HttpGet]
        public async Task<IActionResult> GetAllInquiriesJson(DateTime? fromDate = null, DateTime? toDate = null, int sourceId = 0, int classId = 0, string? status = null)
        {
            try
            {
                var r = await _client.GetAllAdmissionInquiriesAsync(fromDate, toDate, sourceId, classId, status);
                return Json(new { success = r.Success, message = r.Message, data = r.Data });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> SaveInquiry([FromBody] FOAdmissionInquiryUpsertRequest req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return Json(new { success = false, message = "Validation Error: " + errors });
                }
                var r = await _client.UpsertAdmissionInquiryAsync(req);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> DeleteInquiry([FromBody] List<int> id)
        {
            try
            {
                var r = await _client.DeleteAdmissionInquiryAsync(id);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
           
        }

        [HttpPost]
        public async Task<IActionResult> SaveFollowUp([FromBody] FOInquiryFollowUpSaveRequest req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return Json(new { success = false, message = "Validation Error: " + errors });
                }
                var r = await _client.SaveInquiryFollowUpAsync(req);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
            
        }

        [HttpGet]
        public async Task<IActionResult> DownloadVisitorAttachment(int id)
        {
            try
            {
                var r = await _client.GetVisitorByIDAsync(id);

                if (!r.Success || string.IsNullOrWhiteSpace(r.Data?.Attachment))
                    return NotFound();

                // Full physical path
                var filePath = Path.Combine(
                    _environment.WebRootPath,
                    r.Data.Attachment.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
                );

                if (!System.IO.File.Exists(filePath))
                    return NotFound("File not found.");

                var fileName = Path.GetFileName(filePath);

                return PhysicalFile(
                    filePath,
                    "application/octet-stream",
                    fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Json(new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
        public async Task<IActionResult> InquiryFollowUp(int id) 
        {
            var model = new FOAdmissionInquiryViewModel();
            try
            {
                
                model = (await _client.GetAdmissionInquiryByIDAsync(id)).Data;

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
        }

        public async Task<IActionResult> AddAdmissionEnquiry(int? id) 
        {
            var model = new AddAdmissionInquiryPageViewModel();
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/FrontOffice/AddAdmissionEnquiry"
               );
                var sessionId = await GetSessionId();
                var companyId = await GetCompanyId();
                var staffId = GetStaffID();
                //var inquiries = await _client.GetAdmissionInquiryByIDAsync(id);
                var classes = await _classClient.GetAllAsync(false, sessionId);
                var sources = await _client.GetAllSourcesAsync();
                var references = await _client.GetAllReferencesAsync();
                var staff = await _hrClient.GetAllStaffAsync(companyId,sessionId, staffId);

                model = new AddAdmissionInquiryPageViewModel
                {
                    //Inquiries = inquiries.Success ? inquiries.Data : new FOAdmissionInquiryViewModel(),
                    Classes = classes.Success ? classes.Data : new List<MstClassViewModel>(),
                    Sources = sources.Success ? sources.Data : new List<MstFOSourceViewModel>(),
                    References = references.Success ? references.Data : new List<MstFOReferenceViewModel>(),
                    Staff = staff.Success ? staff.Data : new List<HRStaffViewModel>()
                };
                if (id.HasValue && id.Value > 0)
                {
                    var response = await _client.GetAdmissionInquiryByIDAsync(id.Value);
                    if (response.Success)
                    {
                        model.Inquiries = response.Data;
                        model.EditInquiries = response.Data;
                    }
                }
                else
                {
                    model.EditInquiries = null;
                }
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
        }

        public async Task<IActionResult> AddVisitorBook(int? id)
        {
            var model = new AddVisitorBookPageViewModel();
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/FrontOffice/VisitorBook"
               );
                var sessionId = await GetSessionId();
                var companyId = await GetCompanyId();
                var staffId = GetStaffID();
                var visitors = await _client.GetAllVisitorsAsync();
                var purposes = await _client.GetAllPurposesAsync();
                var classes = await _classClient.GetAllAsync();
                var staff = await _hrClient.GetAllStaffAsync(companyId,sessionId, staffId);

                model = new AddVisitorBookPageViewModel
                {
                    //Visitors = visitors.Success ? visitors.Data : new List<FOVisitorBookViewModel>(),
                    Purposes = purposes.Success ? purposes.Data : new List<MstFOPurposeViewModel>(),
                    Classes = classes.Success ? classes.Data : new List<MstClassViewModel>(),
                    Staff = staff.Success ? staff.Data : new List<HRStaffViewModel>()
                };
                if (id.HasValue && id.Value > 0)
                {
                    var response = await _client.GetVisitorByIDAsync(id.Value);
                    if (response.Success)
                    {
                        model.Visitors = response.Data;
                        model.EditVisitors = response.Data;
                    }
                }
                else
                {
                    model.EditVisitors = null;
                }
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }

        }
    }
}
