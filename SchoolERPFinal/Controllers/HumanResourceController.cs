using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Services.Clients;
using SchoolERP.Net.Services;
using SchoolERP.Net.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using System;


namespace SchoolERP.Net.Controllers
{
    /// <summary>
    /// This controller manages the school's human resources, including staff information, job designations, departments, and leave policies.
    /// </summary>
    public class HumanResourceController : Controller
    {
        private readonly IHumanResourceClientService _hrClient;
        private readonly IRoleClientService _roleClient;
        private readonly IUserTypeClientService _userTypeClient;
        private readonly ICompanyClientService _companyClient;
        private readonly ISettingsClientService _settingsClient; 
        private readonly ISessionClientService _sessionService;

        private const string DesignationMenuPath = "/HumanResource/Designation";
        private const string DepartmentMenuPath = "/HumanResource/Department";
        private const string LeaveTypeMenuPath = "/HumanResource/LeaveType";
        private const string StaffMenuPath = "/HumanResource/Staffs";

        public HumanResourceController(
            IHumanResourceClientService hrClient,
            IRoleClientService roleClient,
            IUserTypeClientService userTypeClient,
            ICompanyClientService companyClient,
            ISettingsClientService settingsClient,
            ISessionClientService sessionService)
        {
            _hrClient = hrClient;
            _roleClient = roleClient;
            _userTypeClient = userTypeClient;
            _companyClient = companyClient;
            //_hrService = hrService;
            _settingsClient = settingsClient;
            _sessionService = sessionService;
        }

        private int GetUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("UserId"), out var id) ? id : 0;
        private async Task<int> GetCompanyId()
        {
            var response = await _companyClient.GetUserCurrentCompanyAsync();
            return response?.Data ?? 0;
        }
        private async Task<int> GetSessionId()
        {
            var response = await _sessionService.GetUserCurrentSessionAsync();
            return response?.Data ?? 0;
        }

        /// <summary>
        /// Shows the 'Designation' page where you can manage different job titles (like Teacher, Admin, or Accountant).
        /// </summary>
        public async Task<IActionResult> Designation()
        {
            // Step 1: Ask the system for all the job titles currently set up.
            var res = await _hrClient.GetAllDesignationsAsync();
            
            // Step 2: Prepare the list to be shown on the screen.
            var model = new HRDesignationPageViewModel
            {
                Items = res.Success ? res.Data : new List<HRDesignationViewModel>()
            };
            if (!res.Success) ViewBag.ErrorMessage = res.Message;
            
            // Step 3: Open the 'Designation' page.
            return View(model);
        }

        /// <summary>
        /// Shows the 'Department' page where you can manage different departments in the school (like Academic, Finance, or Sports).
        /// </summary>
        public async Task<IActionResult> Department()
        {
            var res = await _hrClient.GetAllDepartmentsAsync();
            var model = new   HRDepartmentPageViewModel();
            if (res.Success) 
            {
                model.Items = res.Data.Data;
            }
            if (!res.Success) ViewBag.ErrorMessage = res.Message;
            return View(model);
        }

        /// <summary>
        /// Shows the 'Leave Type' page where you can define different types of employee leave (like Medical Leave or Casual Leave).
        /// </summary>
        public async Task<IActionResult> LeaveType()
        {
            var res = await _hrClient.GetAllLeaveTypesAsync();
            var model = new HRLeaveTypePageViewModel
            {
                Items = res.Success ? res.Data : new List<HRLeaveTypeViewModel>()
            };
            if (!res.Success) ViewBag.ErrorMessage = res.Message;
            return View(model);
        }

        /// <summary>
        /// Shows the 'Add Staff' page where you can register a new employee or update an existing employee's details.
        /// </summary>
        public async Task<IActionResult> AddStaff(int? id)
        {
            // Step 1: Initialize a new blank page model.
            var model = new HRStaffPageViewModel();
 
            // Step 2: Fetch all the necessary dropdown lists (Designations, Departments, Roles, User Types, and School Branches).
            var desigRes = await _hrClient.GetAllDesignationsAsync();
            model.Designations = desigRes.Success ? desigRes.Data : new List<HRDesignationViewModel>();
 
            var deptRes = await _hrClient.GetAllDepartmentsAsync();
            model.Departments = deptRes.Success ? deptRes.Data.Data : new List<HRDepartmentViewModel>();

            var leaveRes = await _hrClient.GetAllLeaveTypesAsync();
            model.LeaveTypes = leaveRes.Success ? leaveRes.Data : new List<HRLeaveTypeViewModel>();
 
            var rolesRes = await _roleClient.GetAllRolesAsync();
            model.Roles = rolesRes.Success ? rolesRes.Data : new List<MstRoleViewModel>();
 
            var typesRes = await _userTypeClient.GetAllAsync();
            model.UserTypes = typesRes.Success ? typesRes.Data : new List<MstUserTypeViewModel>();
 
            var compRes = await _companyClient.GetAllAsync();
            model.Companies = compRes.Success ? compRes.Data : new List<MstCompanyViewModel>();

            var fieldRes = await _settingsClient.GetAllFieldsAsync(isSystemField: null, belongsTo: "Staff");
            var allFields = fieldRes.Success ? fieldRes.Data : new List<FieldModel>();
            model.SystemFields = allFields.Where(f => f.IsSystemField).ToList();
            model.CustomFields = allFields.Where(f => !f.IsSystemField).ToList();
 
            // Step 3: If we are editing an existing person (ID is provided), fetch their details.

            if (id.HasValue && id.Value > 0)
            {
                var staffRes = await _hrClient.GetStaffByIDAsync(id.Value);
                if (staffRes.Success)
                {
                    model.EditStaff = staffRes.Data;
                }
            }
            // Step 4: If this is a new person, generate a fresh Staff ID code for them.
            else
            {
                var codeRes = await _hrClient.GetNewStaffCodeAsync();
                model.NewStaffCode = codeRes.Success ? codeRes.Data : "";
            }
 
            // Step 5: Open the 'Add Staff' page with all the gathered information.
            return View(model);
        }

        /// <summary>
        /// Shows the 'Staff Directory' page, displaying a list of all current school employees.
        /// </summary>
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> Staffs(string? search, string? roleName, string? designationName, string? departmentName, string? viewType)
        {
            var companyId =await GetCompanyId();
            var sessionId =await GetSessionId();

            var allStaff = (await _hrClient.GetAllStaffAsync()).Data;

            // Server-side Filtering
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                allStaff = allStaff.Where(s => 
                    (s.FirstName + " " + s.LastName).ToLower().Contains(search) || 
                    s.StaffCode.ToLower().Contains(search)
                ).ToList();
            }

            if (!string.IsNullOrEmpty(roleName))
            {
                allStaff = allStaff.Where(s => s.RoleName == roleName).ToList();
            }

            if (!string.IsNullOrEmpty(designationName))
            {
                allStaff = allStaff.Where(s => s.DesignationName == designationName).ToList();
            }

            if (!string.IsNullOrEmpty(departmentName))
            {
                allStaff = allStaff.Where(s => s.DepartmentName == departmentName).ToList();
            }

            var fieldRes = await _settingsClient.GetAllFieldsAsync(isSystemField: null, belongsTo: "Staff");
            var allFields = fieldRes.Success ? fieldRes.Data : new List<FieldModel>();
            //this code used if any time data null code working not any error get null
            var depmodel = new List<HRDepartmentViewModel>();
            var depart = await _hrClient.GetAllDepartmentsAsync();
            if (depart.Success) 
            {
                depmodel = depart.Data.Data;
            }

            //this code used if any time data null code working not any error get null
            var desimodel = new List<HRDesignationViewModel>();
            var desi = await _hrClient.GetAllDesignationsAsync();
            if (desi.Success)
            {
                desimodel = desi.Data;
            }

            //this code used if any time data null code working not any error get null
            var rolesmodel = new List<MstRoleViewModel>();
            var roles = await _roleClient.GetAllRolesAsync();
            if (roles.Success)
            {
                rolesmodel = roles.Data;
            }
            var model = new HRStaffPageViewModel
            {
                Roles = rolesmodel,
                Departments = depmodel,
                Designations = desimodel,
                StaffList = allStaff,
                ViewType = viewType ?? "list",
                SearchTerm = search,
                SelectedRole = roleName,
                SelectedDesignation = designationName,
                SelectedDepartment = departmentName,
                SystemFields = allFields.Where(f => f.IsSystemField).ToList(),
                CustomFields = allFields.Where(f => !f.IsSystemField).ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> DisableStaffs()
        {
            var model = new HRStaffPageViewModel();
            
            var desigRes = await _hrClient.GetAllDesignationsAsync();
            model.Designations = desigRes.Success ? desigRes.Data : new List<HRDesignationViewModel>();

            var deptRes = await _hrClient.GetAllDepartmentsAsync();
            model.Departments = deptRes.Success ? deptRes.Data.Data : new List<HRDepartmentViewModel>();

            var rolesRes = await _roleClient.GetAllRolesAsync();
            model.Roles = rolesRes.Success ? rolesRes.Data : new List<MstRoleViewModel>();

            var staffRes = await _hrClient.GetAllStaffAsync();
            // Filter DISABLED ONLY
            model.StaffList = staffRes.Success ? staffRes.Data.Where(s => !s.IsActive).ToList() : new List<HRStaffViewModel>();

            return View(model);
        }

        public async Task<IActionResult> StaffDetails(int id)
        {
            var res = await _hrClient.GetStaffByIDAsync(id);
            if (!res.Success || res.Data == null)
            {
                return RedirectToAction("Staffs");
            }

            var fieldRes = await _settingsClient.GetAllFieldsAsync(isSystemField: true, belongsTo: "Staff");
            res.Data.SystemFields = fieldRes.Success ? fieldRes.Data : new List<FieldModel>();

            return View(res.Data);
        }

        public async Task<IActionResult> StaffAttendance()
        {
            var res = await _roleClient.GetAllRolesAsync();
            var model = new HRStaffPageViewModel
            {
                Roles = res.Data ?? new List<MstRoleViewModel>()
            };
            return View(model);
        }

        /// <summary>
        /// Shows the 'Apply Leave' page where staff members can request time off.
        /// They can see their own past leave requests and check the status of pending requests.
        /// </summary>
        public async Task<IActionResult> ApplyLeave()
        {
            int userId = GetUserId();
            int companyId =await GetCompanyId();
            int sessionId =await GetSessionId();

            var allStaff =(await _hrClient.GetAllStaffAsync()).Data;
            var currentStaff = allStaff.FirstOrDefault(s => s.UserID == userId);

            // Fallback: If not found in current company (often 0 on fresh login), search globally
            if (currentStaff == null)
            {
                var globalStaff =(await _hrClient.GetAllStaffAsync()).Data;
                currentStaff = globalStaff.FirstOrDefault(s => s.UserID == userId);
            }

            var model = new HRApplyLeavePageViewModel();
            
            if (currentStaff != null)
            {
                // Use staff's own company/session if current ones are invalid
                if (companyId <= 0) companyId = currentStaff.CompanyID;
                if (sessionId <= 0) sessionId = currentStaff.SessionID;

                // Only show leaves for this staff member
                model.Leaves =(await _hrClient.GetAllApplyLeaveAsync()).Data
                    .Where(l => l.StaffID == currentStaff.StaffID).ToList();
                
                // Set the current staff as the only option
                model.StaffList = new List<HRStaffViewModel> { currentStaff };
            }
            else
            {
                model.Leaves = new List<HRApplyLeaveViewModel>();
                model.StaffList = new List<HRStaffViewModel>();
            }

            // Ensure we fetch leave types for the correct company
            model.LeaveTypes =(await _hrClient.GetAllLeaveTypesAsync()).Data;
            
            return View(model);
        }

        /// <summary>
        /// Shows the 'Approve Leave' page where administrators can see all staff leave requests and decide whether to approve or reject them.
        /// </summary>
        public async Task<IActionResult> ApproveLeave()
        {
            int companyId = await GetCompanyId();
            int sessionId = await GetSessionId();

            var model = new HRApplyLeavePageViewModel
            {
                Leaves =(await _hrClient.GetAllApplyLeaveAsync()).Data,
                StaffList =(await _hrClient.GetAllStaffAsync()).Data,
                LeaveTypes = (await _hrClient.GetAllLeaveTypesAsync()).Data
            };
            return View(model);
        }

        public async Task<IActionResult> GetAllPayroll(int month,int year,int rollId)
        {
            int companyId = await GetCompanyId();
            int sessionId = await GetSessionId();
            var list =(await _hrClient.GetAllPayrollAsync(month, year, rollId)).Data;
            return Ok(new { success = true, data = list, debug_companyId = companyId, debug_sessionId = sessionId });
        }


        public async Task<IActionResult> DownloadStaffDocument(int id, string type)
        {
            
            var response = await _hrClient
        .GetStaffDocumentAsync(id, type);

            if (!response.Success)
                return NotFound();

            var bytes = await response.Data.Content.ReadAsByteArrayAsync();

            var contentType =
                response.Data.Content.Headers.ContentType?.MediaType
                ?? "application/octet-stream";

            var fileName =
                response.Data.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                ?? "Document";

            if (bytes == null || bytes.Length == 0) return NotFound();
            return File(bytes, contentType, fileName);
        }

        public async Task<IActionResult> DownloadApplyLeaveDocument(int id)
        {
            
            var response = await _hrClient
        .GetApplyLeaveDocumentAsync(id);

            if (!response.Success)
                return NotFound();

            var bytes = await response.Data.Content.ReadAsByteArrayAsync();

            var contentType =
                response.Data.Content.Headers.ContentType?.MediaType
                ?? "application/octet-stream";

            var fileName =
                response.Data.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                ?? "Attachment.pdf";
            if (bytes == null || bytes.Length == 0) return NotFound();
            return File(bytes, contentType, fileName);
        }

        /// <summary>
        /// Shows the 'Payroll' page where administrators can manage staff salaries, bonuses, and deductions.
        /// </summary>
        public async Task<IActionResult> Payroll()
        {
            var rolesRes = await _roleClient.GetAllRolesAsync();
            var model = new HRPayrollPageViewModel
            {
                Roles = rolesRes.Success ? rolesRes.Data : new List<MstRoleViewModel>(),
                SelectedMonth = DateTime.Now.Month,
                SelectedYear = DateTime.Now.Year
            };
            return View(model);
        }

        public async Task<IActionResult> GeneratePayroll(int id, int month, int year)
        {
            var model =(await _hrClient.GetPayrollGenerationDataAsync(id, month, year)).Data;
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> ImportStaff()
        {
            var rolesRes = await _roleClient.GetAllRolesAsync();
            ViewBag.Roles = rolesRes.Success ? rolesRes.Data : new List<MstRoleViewModel>();

            var desigRes = await _hrClient.GetAllDesignationsAsync();
            ViewBag.Designations = desigRes.Success ? desigRes.Data : new List<HRDesignationViewModel>();

            var deptRes = await _hrClient.GetAllDepartmentsAsync();
            ViewBag.Departments = deptRes.Success ? deptRes.Data.Data : new List<HRDepartmentViewModel>();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ImportStaff(int roleId, int designationId, int departmentId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Please select a CSV file." });

            try
            {
                var companyId = await GetCompanyId();
                var sessionId = await GetSessionId();
                var userId = GetUserId();

                using var reader = new StreamReader(file.OpenReadStream());
                string? headerLine = await reader.ReadLineAsync();
                if (headerLine == null) return Json(new { success = false, message = "File is empty." });

                var headers = headerLine.Split(',').Select(h => h.Trim().ToLower()).ToList();
                var results = new List<object>();

                while (!reader.EndOfStream)
                {
                    string? line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // Simple CSV split (note: doesn't handle commas inside quotes, but standard for this project's samples)
                    var values = line.Split(',').Select(v => v.Trim()).ToList();
                    var req = new HRStaffUpsertRequest
                    {
                        StaffID = 0,
                        DesignationID = designationId,
                        DepartmentID = departmentId,
                        RoleIDs = new List<int> { roleId },
                        CompanyIDs = new List<int> { companyId },
                        IsActive = true,
                        UserTypeID = 2 // Defaulting to Staff user type, adjust if needed
                    };

                    for (int i = 0; i < headers.Count && i < values.Count; i++)
                    {
                        string header = headers[i];
                        string value = values[i];

                        switch (header)
                        {
                            case "employee_id": req.StaffCode = value; break;
                            case "name": req.FirstName = value; break;
                            case "surname": req.LastName = value; break;
                            case "father_name": req.FatherName = value; break;
                            case "mother_name": req.MotherName = value; break;
                            case "email": 
                                req.Email = value; 
                                req.Username = value; 
                                break;
                            case "dob": 
                                if (DateTime.TryParse(value, out var dob)) req.DOB = dob; 
                                break;
                            case "gender": req.Gender = value; break;
                            case "marital_status": req.MaritalStatus = value; break;
                            case "contact_no": 
                                req.MobileNo = value.Length > 10 ? value.Substring(0, 10) : value; 
                                break;
                            case "emergency_contact_no": 
                                req.EmergencyMobileNo = value.Length > 10 ? value.Substring(0, 10) : value; 
                                break;
                            case "date_of_joining": 
                                if (DateTime.TryParse(value, out var doj)) req.DOJ = doj; 
                                break;
                            case "local_address": req.CurrentAddress = value; break;
                            case "permanent_address": req.PermanentAddress = value; break;
                            case "qualification": req.Qualification = value; break;
                            case "work_exp": req.WorkExperience = value; break;
                            case "note": req.Note = value; break;
                            case "basic_salary": 
                                if (decimal.TryParse(value, out var salary)) req.BasicSalary = salary; 
                                break;
                            case "epf_no": req.EPFNo = value; break;
                            case "contract_type": req.ContractType = value; break;
                            case "shift": req.WorkShift = value; break;
                            case "location": req.WorkLocation = value; break;
                            case "bank_account_no": req.BankAccountNo = value; break;
                            case "bank_name": req.BankName = value; break;
                            case "ifsc_code": req.IFSCCode = value; break;
                            case "bank_branch": req.BankBranchName = value; break;
                            case "account_title": req.AccountTitle = value; break;
                            case "facebook": req.FacebookURL = value; break;
                            case "twitter": req.TwitterURL = value; break;
                            case "linkedin": req.LinkedinURL = value; break;
                            case "instagram": req.InstagramURL = value; break;
                        }
                    }

                    // Validation: Ensure mandatory fields are present
                    if (string.IsNullOrEmpty(req.FirstName) || string.IsNullOrEmpty(req.Username))
                    {
                        results.Add(new { staffCode = req.StaffCode, name = req.FirstName, success = false, message = "Missing mandatory fields (Name/Email)." });
                        continue;
                    }

                    var res = await _hrClient.UpsertStaffAsync(req);
                    results.Add(new
                    {
                        staffCode = req.StaffCode,
                        name = (req.FirstName + " " + req.LastName).Trim(),
                        success = res.Success,
                        message = res.Message
                    });
                }
                return Json(new { success = true, results });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStaff([FromBody] List<int> id)
        {
            var res = await _hrClient.DeleteStaffAsync(id);
            return Json(new { success = true, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLeaveType([FromBody] List<int> id)
        {
            var res = await _hrClient.DeleteLeaveTypeAsync(id);
            return Json(new { success =true, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDepartment([FromBody] List<int> id)
        {
            var res = await _hrClient.DeleteDepartmentAsync(id);
            return Json(new { success = res.Success, message = res.Message });
        }
        [HttpPost]
        public async Task<IActionResult> DeleteDesignation([FromBody] List<int> id)
        {
            var res = await _hrClient.DeleteDesignationAsync(id);
            return Json(new { success = res.Success, message = res.Message });
        }
        
        [HttpPost("UpdateDepartment")]
        public async Task<IActionResult> UpdateDepartment([FromBody] HRDepartmentUpsertRequest req)
        {
            var isCreate = req.DepartmentID <= 0;
            var res =await _hrClient.UpsertDepartmentAsync(req);
            return Ok(new { success = res.Success, message = res.Message });
        }
        [HttpPost("UpsertDesignation")]
        public async Task<IActionResult> UpsertDesignation([FromBody] HRDesignationUpsertRequest req)
        {            
            var res = await _hrClient.UpsertDesignationAsync(req);
            return Ok(new { success = res.Success, message = res.Message });
        }

        [HttpPost("UpsertStaff")]
        public async Task<IActionResult> UpsertStaff([FromBody] HRStaffUpsertRequest req)
        {
            var isCreate = req.StaffID <= 0;
            var res =await _hrClient.UpsertStaffAsync(req);
            return Ok(new { success = res.Success, message = res.Message });
        }
    }
}
