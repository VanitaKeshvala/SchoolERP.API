using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Models;
using SchoolERP.Net.Services;
using SchoolERP.Net.Services.Clients;

namespace SchoolERP.Net.Controllers
{
    /// <summary>
    /// This controller manages the school's front office activities, such as visitor logs, complaints, phone calls, and mail (postal) services.
    /// </summary>
    public class FrontOfficeController : Controller
    {
        private readonly IFrontOfficeClientService _client;
        private readonly IUserMenuPermissionClientService _menuPerm;
        private readonly IClassClientService _classClient;
        private readonly ISectionClientService _sectionClient;
        private readonly IStudentInformationClientService _studentClient;
        private readonly IHumanResourceClientService _hrClient;
        private const string MenuPath = "/FrontOffice/Setup";
        private const string ComplaintMenuPath = "/FrontOffice/Complaint";

        public FrontOfficeController(
            IFrontOfficeClientService client,
            IUserMenuPermissionClientService menuPerm,
            IClassClientService classClient,
            ISectionClientService sectionClient,
            IStudentInformationClientService studentClient,
            IHumanResourceClientService hrClient)
        {
            _client = client;
            _menuPerm = menuPerm;
            _classClient = classClient;
            _sectionClient = sectionClient;
            _studentClient = studentClient;
            _hrClient = hrClient;
        }

        /// <summary>
        /// Shows the 'Setup' page where you can manage categories for visitors and complaints (like Purposes, Sources, and References).
        /// </summary>
        public async Task<IActionResult> Setup()
        {
            // Step 1: Gather all categories like 'Visitor Purposes', 'Complaint Types', 'Sources', and 'References'.
            var purposes        = await _client.GetAllPurposesAsync();
            var complaintTypes  = await _client.GetAllComplaintTypesAsync();
            var sources         = await _client.GetAllSourcesAsync();
            var references      = await _client.GetAllReferencesAsync();

            // Step 2: Organize these lists so they can be shown on the setup management page.
            var model = new FrontOfficeSetupPageViewModel
            {
                Purposes       = purposes.Success       ? purposes.Data       : new List<MstFOPurposeViewModel>(),
                ComplaintTypes = complaintTypes.Success  ? complaintTypes.Data  : new List<MstFOComplaintTypeViewModel>(),
                Sources        = sources.Success         ? sources.Data         : new List<MstFOSourceViewModel>(),
                References     = references.Success      ? references.Data      : new List<MstFOReferenceViewModel>()
            };

            // Step 3: Open the 'Setup' page for the user.
            return View(model);
        }

        // ─── PURPOSE ────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetPurpose(int id)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "Permission denied." });
            var r = await _client.GetPurposeByIDAsync(id);
            if (!r.Success) return Json(new { success = false, message = r.Message });
            return Json(new { success = true, data = r.Data });
        }

        [HttpPost]
        public async Task<IActionResult> SavePurpose([FromBody] MstFOPurposeUpsertRequest req)
        {
            bool isCreate = req.PurposeID <= 0;
            if (isCreate && !(await _menuPerm.Has(MenuPath, "Add")).Data)
                return Json(new { success = false, message = "Permission denied." });
            if (!isCreate && !(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "Permission denied." });
            var r = await _client.UpsertPurposeAsync(req);
            return Json(new { success = r.Success, message = r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeletePurpose([FromBody] List<int> id)
        {
            if (!(await _menuPerm.Has(MenuPath, "Delete")).Data)
                return Json(new { success = false, message = "Permission denied." });
            var r = await _client.DeletePurposeAsync(id);
            return Json(new { success = r.Success, message = r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> TogglePurposeStatus(int id, bool isActive)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "Permission denied." });
            var r = await _client.TogglePurposeStatusAsync(id, isActive);
            return Json(new { success = r.Success, message = r.Message });
        }

        // ─── COMPLAINT TYPE ─────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetComplaintType(int id)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "Permission denied." });
            var r = await _client.GetComplaintTypeByIDAsync(id);
            if (!r.Success) return Json(new { success = false, message = r.Message });
            return Json(new { success = true, data = r.Data });
        }

        [HttpPost]
        public async Task<IActionResult> SaveComplaintType([FromBody] MstFOComplaintTypeUpsertRequest req)
        {
            bool isCreate = req.ComplaintTypeID <= 0;
            if (isCreate && !(await _menuPerm.Has(MenuPath, "Add")).Data)
                return Json(new { success = false, message = "Permission denied." });
            if (!isCreate && !(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "Permission denied." });
            var r = await _client.UpsertComplaintTypeAsync(req);
            return Json(new { success = r.Success, message = r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComplaintType([FromBody] List<int> id)
        {
            if (!(await _menuPerm.Has(MenuPath, "Delete")).Data)
                return Json(new { success = false, message = "Permission denied." });
            var r = await _client.DeleteComplaintTypeAsync(id);
            return Json(new { success = r.Success, message = r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleComplaintTypeStatus(int id, bool isActive)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "Permission denied." });
            var r = await _client.ToggleComplaintTypeStatusAsync(id, isActive);
            return Json(new { success = r.Success, message = r.Message });
        }

        // ─── SOURCE ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetSource(int id)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "Permission denied." });
            var r = await _client.GetSourceByIDAsync(id);
            if (!r.Success) return Json(new { success = false, message = r.Message });
            return Json(new { success = true, data = r.Data });
        }

        [HttpPost]
        public async Task<IActionResult> SaveSource([FromBody] MstFOSourceUpsertRequest req)
        {
            bool isCreate = req.SourceID <= 0;
            if (isCreate && !(await _menuPerm.Has(MenuPath, "Add")).Data)
                return Json(new { success = false, message = "Permission denied." });
            if (!isCreate && !(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "Permission denied." });
            var r = await _client.UpsertSourceAsync(req);
            return Json(new { success = r.Success, message = r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSource([FromBody] List<int> id)
        {
            if (!(await _menuPerm.Has(MenuPath, "Delete")).Data)
                return Json(new { success = false, message = "Permission denied." });
            var r = await _client.DeleteSourceAsync(id);
            return Json(new { success = r.Success, message = r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleSourceStatus(int id, bool isActive)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "Permission denied." });
            var r = await _client.ToggleSourceStatusAsync(id, isActive);
            return Json(new { success = r.Success, message = r.Message });
        }

        // ─── REFERENCE ──────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetReference(int id)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "Permission denied." });
            var r = await _client.GetReferenceByIDAsync(id);
            if (!r.Success) return Json(new { success = false, message = r.Message });
            return Json(new { success = true, data = r.Data });
        }

        [HttpPost]
        public async Task<IActionResult> SaveReference([FromBody] MstFOReferenceUpsertRequest req)
        {
            bool isCreate = req.ReferenceID <= 0;
            if (isCreate && !(await _menuPerm.Has(MenuPath, "Add")).Data)
                return Json(new { success = false, message = "Permission denied." });
            if (!isCreate && !(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "Permission denied." });
            var r = await _client.UpsertReferenceAsync(req);
            return Json(new { success = r.Success, message = r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReference([FromBody] List<int> id)
        {
            if (!(await _menuPerm.Has(MenuPath, "Delete")).Data)
                return Json(new { success = false, message = "Permission denied." });
            var r = await _client.DeleteReferenceAsync(id);
            return Json(new { success = r.Success, message = r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleReferenceStatus(int id, bool isActive)
        {
            if (!(await _menuPerm.Has(MenuPath, "Edit")).Data)
                return Json(new { success = false, message = "Permission denied." });
            var r = await _client.ToggleReferenceStatusAsync(id, isActive);
            return Json(new { success = r.Success, message = r.Message });
        }

        /// <summary>
        /// Shows the main 'Complaint' management page where you can see all student or parent complaints and their status.
        /// </summary>
        public async Task<IActionResult> Complaint()
        {
            // Step 1: Fetch all recorded complaints, types of complaints, and where they came from (sources).
            var complaints      = await _client.GetAllComplaintsAsync();
            var complaintTypes  = await _client.GetAllComplaintTypesAsync();
            var sources         = await _client.GetAllSourcesAsync();

            // Step 2: Combine this information to be shown on the complaint management screen.
            var model = new FOComplaintPageViewModel
            {
                Complaints     = complaints.Success      ? complaints.Data      : new List<FOComplaintViewModel>(),
                ComplaintTypes = complaintTypes.Success  ? complaintTypes.Data  : new List<MstFOComplaintTypeViewModel>(),
                Sources        = sources.Success         ? sources.Data         : new List<MstFOSourceViewModel>()
            };

            // Step 3: Open the 'Complaint' management page.
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetComplaint(int id)
        {
            if (!(await _menuPerm.Has(ComplaintMenuPath, "Edit")).Data)
                return Json(new { success = false, message = "Permission denied." });
            var r = await _client.GetComplaintByIDAsync(id);
            return Json(new { success = r.Success, message = r.Message, data = r.Data });
        }

        [HttpPost]
        public async Task<IActionResult> SaveComplaint([FromBody] FOComplaintUpsertRequest req)
        {
            bool isCreate = req.ComplaintID <= 0;
            if (isCreate && !(await _menuPerm.Has(ComplaintMenuPath, "Add")).Data)
                return Json(new { success = false, message = "Permission denied." });
            if (!isCreate && !(await _menuPerm.Has(ComplaintMenuPath, "Edit")).Data)
                return Json(new { success = false, message = "Permission denied." });
            var r = await _client.UpsertComplaintAsync(req);
            return Json(new { success = r.Success, message = r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComplaint([FromBody] List<int> id)
        {
            if (!(await _menuPerm.Has(ComplaintMenuPath, "Delete")).Data)
                return Json(new { success = false, message = "Permission denied." });
            var r = await _client.DeleteComplaintAsync(id);
            return Json(new { success = r.Success, message = r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleComplaintStatus(int id, bool isActive)
        {
            if (!(await _menuPerm.Has(ComplaintMenuPath, "Edit")).Data)
                return Json(new { success = false, message = "Permission denied." });
            var r = await _client.ToggleComplaintStatusAsync(id, isActive);
            return Json(new { success = r.Success, message = r.Message });
        }

        /// <summary>
        /// Shows the 'Postal Receive' page, which tracks all the physical mail or packages the school has received.
        /// </summary>
        public async Task<IActionResult> PostalReceive()
        {
            var res = await _client.GetAllPostalReceivesAsync();
            var model = new FOPostalReceivePageViewModel
            {
                Items = res.Success ? res.Data : new List<FOPostalReceiveViewModel>()
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetPostalReceive(int id)
        {
            var r = await _client.GetPostalReceiveByIDAsync(id);
            return Json(new { success = r.Success, message = r.Message, data = r.Data });
        }

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
                    req.FileName = attachmentFile.FileName;
                    req.FileType = attachmentFile.ContentType;
                    using (var ms = new MemoryStream())
                    {
                        await attachmentFile.CopyToAsync(ms);
                        req.Attachment = ms.ToArray();
                    }
                }
                var r = await _client.UpsertPostalReceiveAsync(req);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeletePostalReceive([FromBody] List<int> id)
        {
            var r = await _client.DeletePostalReceiveAsync(id);
            return Json(new { success = r.Success, message = r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> TogglePostalReceiveStatus(int id, bool isActive)
        {
            var r = await _client.TogglePostalReceiveStatusAsync(id, isActive);
            return Json(new { success = r.Success, message = r.Message });
        }

        [HttpGet]
        public async Task<IActionResult> DownloadAttachment(int id)
        {
            var r = await _client.GetPostalReceiveByIDAsync(id);
            if (!r.Success || r.Data?.Attachment == null) return NotFound();
            
            string contentType = r.Data.FileType ?? "application/octet-stream";
            string fileName = r.Data.FileName ?? $"attachment_{id}";
            
            return File(r.Data.Attachment, contentType, fileName);
        }

        /// <summary>
        /// Shows the 'Postal Dispatch' page, which keeps a record of all the mail or packages the school has sent out.
        /// </summary>
        public async Task<IActionResult> PostalDispatch()
        {
            var res = await _client.GetAllPostalDispatchesAsync();
            var model = new FOPostalDispatchPageViewModel
            {
                Items = res.Success ? res.Data : new List<FOPostalDispatchViewModel>()
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetPostalDispatch(int id)
        {
            var r = await _client.GetPostalDispatchByIDAsync(id);
            return Json(new { success = r.Success, message = r.Message, data = r.Data });
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
                    req.FileName = attachmentFile.FileName;
                    req.FileType = attachmentFile.ContentType;
                    using (var ms = new MemoryStream())
                    {
                        await attachmentFile.CopyToAsync(ms);
                        req.Attachment = ms.ToArray();
                    }
                }
                var r = await _client.UpsertPostalDispatchAsync(req);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeletePostalDispatch([FromBody] List<int> id)
        {
            var r = await _client.DeletePostalDispatchAsync(id);
            return Json(new { success = r.Success, message = r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> TogglePostalDispatchStatus(int id, bool isActive)
        {
            var r = await _client.TogglePostalDispatchStatusAsync(id, isActive);
            return Json(new { success = r.Success, message = r.Message });
        }

        [HttpGet]
        public async Task<IActionResult> DownloadDispatchAttachment(int id)
        {
            var r = await _client.GetPostalDispatchByIDAsync(id);
            if (!r.Success || r.Data?.Attachment == null) return NotFound();

            string contentType = r.Data.FileType ?? "application/octet-stream";
            string fileName = r.Data.FileName ?? $"dispatch_attachment_{id}";

            return File(r.Data.Attachment, contentType, fileName);
        }

        /// <summary>
        /// Shows the 'Phone Call Log' page, where staff can record details of incoming and outgoing calls.
        /// </summary>
        public async Task<IActionResult> PhoneCallLog()
        {
            var res = await _client.GetAllPhoneCallLogsAsync();
            var model = new FOPhoneCallLogPageViewModel
            {
                Items = res.Success ? res.Data : new List<FOPhoneCallLogViewModel>()
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetPhoneCallLog(int id)
        {
            var r = await _client.GetPhoneCallLogByIDAsync(id);
            return Json(new { success = r.Success, message = r.Message, data = r.Data });
        }

        [HttpPost]
        public async Task<IActionResult> SavePhoneCallLog([FromBody] FOPhoneCallLogUpsertRequest req)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { success = false, message = "Validation Error: " + errors });
            }
            var r = await _client.UpsertPhoneCallLogAsync(req);
            return Json(new { success = r.Success, message = r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeletePhoneCallLog([FromBody] List<int> id)
        {
            var r = await _client.DeletePhoneCallLogAsync(id);
            return Json(new { success = r.Success, message = r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> TogglePhoneCallLogStatus(int id, bool isActive)
        {
            var r = await _client.TogglePhoneCallLogStatusAsync(id, isActive);
            return Json(new { success = r.Success, message = r.Message });
        }

        /// <summary>
        /// Shows the 'Visitor Book' page, where the school tracks who visited, why they came, and who they met.
        /// </summary>
        public async Task<IActionResult> VisitorBook()
        {
            var visitors = await _client.GetAllVisitorsAsync();
            var purposes = await _client.GetAllPurposesAsync();
            var classes  = await _classClient.GetAllAsync();
            var staff    = await _hrClient.GetAllStaffAsync();

            var model = new FOVisitorBookPageViewModel
            {
                Visitors = visitors.Success ? visitors.Data : new List<FOVisitorBookViewModel>(),
                Purposes = purposes.Success ? purposes.Data : new List<MstFOPurposeViewModel>(),
                Classes = classes.Success ? classes.Data : new List<MstClassViewModel>(),
                Staff = staff.Success ? staff.Data : new List<HRStaffViewModel>()
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetSections(int classId)
        {
            var r = await _sectionClient.GetByClassAsync(classId);
            return Json(new { success = r.Success, data = r.Data });
        }

        [HttpGet]
        public async Task<IActionResult> GetStudents(int classId, int sectionId)
        {
            var r = await _studentClient.GetStudentListAsync(classId, sectionId);
            return Json(new { success = r.Success, data = r.Data });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStaff()
        {
            var r = await _hrClient.GetAllStaffAsync();
            return Json(new { success = r.Success, data = r.Data });
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentByID(int id)
        {
            var r = await _studentClient.GetStudentByIDAsync(id);
            return Json(new { success = r.Success, data = r.Data });
        }

        [HttpGet]
        public async Task<IActionResult> GetVisitor(int id)
        {
            var r = await _client.GetVisitorByIDAsync(id);
            return Json(new { success = r.Success, message = r.Message, data = r.Data });
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
                    req.FileName = attachmentFile.FileName;
                    req.FileType = attachmentFile.ContentType;
                    using (var ms = new MemoryStream())
                    {
                        await attachmentFile.CopyToAsync(ms);
                        req.Attachment = ms.ToArray();
                    }
                }
                var r = await _client.UpsertVisitorAsync(req);
                return Json(new { success = r.Success, message = r.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVisitor([FromBody] List<int> id)
        {
            var r = await _client.DeleteVisitorAsync(id);
            return Json(new { success = r.Success, message = r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleVisitorStatus(int id, bool isActive)
        {
            var r = await _client.ToggleVisitorStatusAsync(id, isActive);
            return Json(new { success = r.Success, message = r.Message });
        }

        /// <summary>
        /// Shows the 'Admission Inquiry' page, where the school records potential students who have asked about joining.
        /// Users can filter inquiries by date, source (like 'Website'), or class.
        /// </summary>
        public async Task<IActionResult> AdmissionInquiry(DateTime? fromDate = null, DateTime? toDate = null, int sourceId = 0, int classId = 0, string? status = null)
        {
            var inquiries = await _client.GetAllAdmissionInquiriesAsync(fromDate, toDate, sourceId, classId, status);
            var classes   = await _classClient.GetAllAsync();
            var sources   = await _client.GetAllSourcesAsync();
            var references = await _client.GetAllReferencesAsync();
            var staff     = await _hrClient.GetAllStaffAsync();

            var model = new FOAdmissionInquiryPageViewModel
            {
                Inquiries  = inquiries.Success ? inquiries.Data : new List<FOAdmissionInquiryViewModel>(),
                Classes    = classes.Success   ? classes.Data   : new List<MstClassViewModel>(),
                Sources    = sources.Success   ? sources.Data   : new List<MstFOSourceViewModel>(),
                References = references.Success ? references.Data : new List<MstFOReferenceViewModel>(),
                Staff      = staff.Success     ? staff.Data     : new List<HRStaffViewModel>()
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetInquiry(int id)
        {
            var r = await _client.GetAdmissionInquiryByIDAsync(id);
            return Json(new { success = r.Success, message = r.Message, data = r.Data });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllInquiriesJson(DateTime? fromDate = null, DateTime? toDate = null, int sourceId = 0, int classId = 0, string? status = null)
        {
            var r = await _client.GetAllAdmissionInquiriesAsync(fromDate, toDate, sourceId, classId, status);
            return Json(new { success = r.Success, message = r.Message, data = r.Data });
        }

        [HttpPost]
        public async Task<IActionResult> SaveInquiry([FromBody] FOAdmissionInquiryUpsertRequest req)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { success = false, message = "Validation Error: " + errors });
            }
            var r = await _client.UpsertAdmissionInquiryAsync(req);
            return Json(new { success = r.Success, message = r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteInquiry([FromBody] List<int> id)
        {
            var r = await _client.DeleteAdmissionInquiryAsync(id);
            return Json(new { success = r.Success, message = r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> SaveFollowUp([FromBody] FOInquiryFollowUpSaveRequest req)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { success = false, message = "Validation Error: " + errors });
            }
            var r = await _client.SaveInquiryFollowUpAsync(req);
            return Json(new { success = r.Success, message = r.Message });
        }

        [HttpGet]
        public async Task<IActionResult> DownloadVisitorAttachment(int id)
        {
            var r = await _client.GetVisitorByIDAsync(id);
            if (!r.Success || r.Data?.Attachment == null) return NotFound();

            string contentType = r.Data.FileType ?? "application/octet-stream";
            string fileName = r.Data.FileName ?? $"visitor_doc_{id}";

            return File(r.Data.Attachment, contentType, fileName);
        }
    }
}
