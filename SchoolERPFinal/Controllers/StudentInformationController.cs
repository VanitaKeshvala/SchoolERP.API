using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Services;
using SchoolERP.Shared.Models;
using System.Security.Claims;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;
using SchoolERP.Net.Services.Clients;
using SchoolERP.Net.Helpers;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Controllers
{
    /// <summary>
    /// This controller manages all the web pages related to student information.
    /// It handles pages for student admission, viewing student lists, student categories, 
    /// houses, and other student-specific settings.
    /// Think of this as the manager who decides which page to show when you click on student-related menus.
    /// </summary>
    public class StudentInformationController :   BaseController
    {
        private readonly IStudentInformationClientService _studentService;
        private readonly ICompanyClientService _companyService;
        private readonly IFieldClientService _fieldService;
        private readonly ISessionClientService _sessionService;
        private readonly IRouteClientService _routeService;
        private readonly IRoutePickupPointClientService _routePickupPointService;
        private readonly IHostelClientService _hostelService;
        private readonly IClassClientService _classService;
        private readonly ISectionClientService _sectionService;
        private readonly IVehicleAssignClientService _vehicleAssignService;
        private readonly IAttendanceClientService _attendanceService;
        private readonly IPhotoUploadService _photoService;
        //private readonly PermissionHelper _permHelper;

        public StudentInformationController(IStudentInformationClientService studentService,
            ICompanyClientService companyService, ISessionClientService sessionService, 
            IFieldClientService fieldService, IRouteClientService routeService, 
            IRoutePickupPointClientService routePickupPointService,
            IHostelClientService hostelService, IClassClientService classService,
            ISectionClientService sectionService, IVehicleAssignClientService vehicleAssignService, 
            IAttendanceClientService attendanceService, IPhotoUploadService photoService, PermissionHelper permHelper) : base(permHelper)
        {
            _studentService = studentService;
            _companyService = companyService;
            _sessionService = sessionService;
            _fieldService = fieldService;
            _routeService = routeService;
            _routePickupPointService = routePickupPointService;
            _hostelService = hostelService;
            _classService = classService;
            _sectionService = sectionService;
            _vehicleAssignService = vehicleAssignService;
            _attendanceService = attendanceService;
            _photoService = photoService;
        }


        private int GetUserId() => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("UserId")?.Value, out var id) ? id : 0;
        private async Task<int> GetCompanyId()
        {
            var response = await _companyService.GetUserCurrentCompanyAsync();
            return response?.Data ?? 0;
        }
        private async Task<int> GetSessionId()
        {
            if(CurrentSessionId == null) 
            {
                var response = await _sessionService.GetUserCurrentSessionAsync();
                return response?.Data ?? 0;
            }
            return CurrentSessionId;
        }

        /// <summary>
        /// Shows the page where the school can manage reasons for disabling students (e.g., 'Transfer Certificate issued').
        /// </summary>
        public async Task<IActionResult> DisableReason()
        {
            var model = new StudentDisableReasonPageViewModel();
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/StudentInformation/DisableReason"
               );

                var sessionId = await GetSessionId();
                var response = await _studentService.GetAllDisableReasons(sessionId);
                                
                model.Items = response.Data ?? new List<StudentDisableReasonViewModel>();
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
        /// Shows the page for managing student houses (e.g., Red House, Blue House).
        /// </summary>
        
        public async Task<IActionResult> StudentHouse(int? pageIndex,
        int? pageSize,
        string? search,
        int? companyId,
        int? sessionID)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                    "/StudentInformation/StudentHouse"
                );

                var request = new SubjectSearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionID = sessionID ?? await GetSessionId()
                };
                var studentHouseTask = _studentService.GetStudentHouseListAsync(request);
                var sessionTask = _sessionService.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();

                await Task.WhenAll(studentHouseTask, sessionTask, companiesTask);
                var pagedResult = await studentHouseTask;
                var model = new StudentHousePageViewModel
                {
                    Items = pagedResult.Success ? pagedResult.Data.Data : new List<StudentHouseViewModel>(),
                    TotalRecords = pagedResult.Data.TotalRecords,
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,
                    SearchTerm = search,
                    CompanyId = companyId,
                    SessionId = sessionID,
                    Companies = (await companiesTask).Data ?? new(),
                    Sessions = (await sessionTask).Data ?? new(),
                };
               
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception ex)
            {
                throw;
            }
            
        }

        /// <summary>
        /// Shows the page for managing student categories (e.g., General, OBC).
        /// </summary>
        public async Task<IActionResult> StudentCategory()
        {
            // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
            var perms = await GetPermissions(
                "/StudentInformation/StudentCategory"
            );
            var sessionId = await GetSessionId();
            var res = (await _studentService.GetAllStudentCategories(sessionId)).Data;
            StudentCategoryPageViewModel model= new StudentCategoryPageViewModel();
            model.Items = res;
            model.Permissions = perms;
            return View(model);
        }

        /// <summary>
        /// Shows the main Student Admission form. 
        /// If an 'id' is provided, it opens the form in 'Edit' mode for that specific student.
        /// It also prepares all the dropdown lists needed for the form, like classes, houses, and hostels.
        /// </summary>
        public async Task<IActionResult> StudentAdmission(int? id)
        {
            var companyId =await GetCompanyId();
            var sessionId =await GetSessionId();
            
            // Fetch all fields to ensure Transport, Hostel, and Guardian categories are included
            var fields = await _fieldService.GetAllFieldsAsync(companyId, sessionId, belongsTo: "students");
            
            if (id.HasValue && id > 0)
            {
                var studentDetails =await _studentService.GetStudentByIDAsync(id.Value);
                if (studentDetails.Data.BasicInfo.StudentID > 0)
                {
                    ViewBag.StudentDetails = studentDetails.Data;
                    ViewBag.IsEdit = true;
                }
            }



            // Fetch lookup data for dropdowns
            ViewBag.Categories = (await _studentService.GetAllStudentCategories(sessionId)).Data;
            ViewBag.Houses = (await _studentService.GetAllStudentHouses(sessionId)).Data;
            ViewBag.Routes =(await _routeService.GetAllRoutesAsync()).Data;
            ViewBag.PickupPoints =(await _routePickupPointService.GetAllRoutePickupPointsAsync()).Data;
            ViewBag.Hostels =(await _hostelService.GetAllHostelsAsync()).Data;
            var res= await _vehicleAssignService.GetAllAssignmentsAsync();
            ViewBag.VehicleAssignments = res.Data;
            ViewBag.Classes =(await _classService.GetAllAsync()).Data;

            // Check if Roll No is Auto Generated
            var autoGenSettings =await _fieldService.GetIDAutoGenSettingsAsync(companyId, sessionId);
            var studentSettings = autoGenSettings.Data.FirstOrDefault(s => s.EntityType == "Student" && s.IsEnabled);
            bool isRollAutoGenerated = studentSettings != null;
            ViewBag.IsRollAutoGenerated = isRollAutoGenerated;
            if (isRollAutoGenerated)
            {
                ViewBag.RollNoDependencies = studentSettings.FieldsToInclude;
                if (!(ViewBag.IsEdit ?? false))
                {
                    var roleModel = new StudentRollNoRequest
                    {
                        SessionId = sessionId
                    };
                    ViewBag.NextRollNo = (await _studentService.GetNewStudentRollNo(roleModel)).Data;
                }
            }

            return View(fields.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentList(int? classId, int? sectionId, string? searchTerm, int? excludeStudentId)
        {
            var companyId = await GetCompanyId();
            var sessionId =await GetSessionId();
            var response = await _studentService.GetStudentListAsync(sessionId, classId, sectionId, searchTerm);
            var students =  new List<StudentListViewModel>();
            if(response.Success != false && response.Data !=null) 
            {
                students = response.Data.Data ?? new List<StudentListViewModel>();
            }
            if (excludeStudentId.HasValue)
            {
                students = students.Where(s => s.StudentID != excludeStudentId.Value).ToList();
            }

            var result = students.Select(s => new { 
                studentID = s.StudentID, 
                fullName = s.FullName, 
                admissionNo = s.AdmissionNo,
                rollNo = s.RollNo
            }).ToList();
            return Json(result);
        }

        [HttpGet]
        /// <summary>
        /// Shows the list of all students. 
        /// Users can filter the list by class, section, or search for a specific name/roll number.
        /// </summary>
        public async Task<IActionResult> Students(int? classId, int? sectionId, string? search, string? viewType, int page ,
    int pageSize )
        {
            var model = new StudentListPageViewModel();
            try
            {                
                // ── Permissions ──────────────────────────────────────────────
                var perms = await GetPermissions(
                    "/StudentInformation/Students",
                    "/StudentInformation/ImportStudent"
                );

                // ── Redirect if no view permission ───────────────────────────
                if (!perms.CanView)
                {
                    TempData["Error"] = "You do not have permission to view students.";
                    return RedirectToAction("Index", "Home");
                }


                var companyId = await GetCompanyId();
                var sessionId = await GetSessionId();

                var allFieldsresponse = await _fieldService.GetAllFieldsAsync(companyId, sessionId, belongsTo: "students");
                List<FieldModel> allFields = new List<FieldModel>();
                if (allFieldsresponse.Success)
                {
                    allFields = allFieldsresponse.Data;
                }
                var response = await _studentService.GetStudentListAsync(sessionId, classId, sectionId, search, page, pageSize);

                model.Students = response.Data.Data;
                model.TotalRecords = response.Data.TotalRecords;
                model.PageNumber = response.Data.PageNumber;
                model.PageSize = response.Data.PageSize;
                model.SelectedClassId = classId;
                model.SelectedSectionId = sectionId;
                model.SearchTerm = search;
                model.ViewType = viewType ?? "list";
                model.SystemFields = allFields.Where(f => f.IsSystemField).ToList();
                model.CustomFields = allFields.Where(f => !f.IsSystemField).ToList();
                model.Permissions = perms;
                

                ViewBag.Classes = (await _classService.GetAllAsync()).Data;
                if (classId.HasValue)
                {
                    ViewBag.Sections = (await _sectionService.GetByClassAsync(classId.Value)).Data;
                }
                else
                {
                    ViewBag.Sections = new List<MstSectionViewModel>();
                }

                int currentUserId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }            
        }
        
        
        [Route("StudentInformation/Details/{id}")]
        /// <summary>
        /// Shows the detailed profile page of a specific student.
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var model =await _studentService.GetStudentByIDAsync(id);
            if (model.Data.BasicInfo.StudentID == 0) 
            {
                return Content($"Student record with ID {id} was not found for the current Company/Session.");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult GetStudentDetails(int id)
        {
            //var model = _studentService.GetStudentDetails(id);
            var model = _studentService.GetStudentByIDAsync(id);
            return Json(model);
        }
        [HttpGet]
        public async Task<IActionResult> GetNextRollNo()
        {
            var query = Request.Query;
            var values = new Dictionary<string, string>();
            foreach (var key in query.Keys)
            {
                values[key] = query[key]!;
            }
            var roleModel = new StudentRollNoRequest
            {
                SessionId = await GetSessionId()
            };
            var response = await _studentService.GetNewStudentRollNo(roleModel);
            string rollNo =string.Empty;
            if (response.Success)
            {
                rollNo = response.Data;
            }
            return Json(new { rollNo });
        }

        [HttpGet]
        public async Task<IActionResult> GetNextAdmissionNo()
        {
            string admissionNo = string.Empty;
            var response = await _studentService.GetNextAdmissionNo();

            if (response.Success)
            {
                admissionNo = response.Data;
            }
            return Json(new { admissionNo });
        }

        [HttpPost]
        /// <summary>
        /// This action is called when the 'Save' button is clicked on the Admission form.
        /// It takes all the student data and sends it to the service to be saved in the database.
        /// </summary>
        public async Task<IActionResult> SaveStudent([FromBody] StudentAdmissionUpsertRequest req)
        {
            req.CompanyID = await GetCompanyId();
            req.SessionId =await GetSessionId();
            var res =await _studentService.UpsertStudentAdmission(req);
            req.FieldValues.TryGetValue("Student Photo", out var photoBase64);
            req.FieldValues.TryGetValue("Student Photo Name", out var photoName);

            req.FieldValues.TryGetValue("Mother Photo", out var photoMotherPhotoBase64);
            req.FieldValues.TryGetValue("Student Photo Name", out var motherPhotoType);

            req.FieldValues.TryGetValue("Father Photo", out var photoFatherPhotoBase64);
            req.FieldValues.TryGetValue("Father Photo Name", out var fatherPhotoType);

            req.FieldValues.TryGetValue("Guardian Photo", out var photoGuardianPhotoBase64);
            req.FieldValues.TryGetValue("Guardian Photo Name", out var guardianPhotoType);
            if (res.Success)
            {
                int studentid = res.Data;
                PhotoUploadResult photoResult = new PhotoUploadResult();
                if (photoBase64 != null && photoBase64 !="")
                {
                    photoResult = await _photoService.SaveBase64PhotoAsync(
                        photoBase64,
                        photoName ?? "photo.jpg",
                        PhotoModule.Student,
                        studentid
                    );
                }

                PhotoUploadResult photoFatherResult = new PhotoUploadResult();
                if(photoFatherPhotoBase64 != null && photoFatherPhotoBase64 != "") 
                {
                    photoFatherResult = await _photoService.SaveBase64PhotoAsync(
                        photoFatherPhotoBase64,
                        fatherPhotoType ?? "photo.jpg",
                        PhotoModule.Parent,
                        studentid
                    );
                }
                PhotoUploadResult photoMotherResult = new PhotoUploadResult();
                if (photoMotherPhotoBase64 != null && photoMotherPhotoBase64 != "") 
                {
                        photoMotherResult = await _photoService.SaveBase64PhotoAsync(
                            photoMotherPhotoBase64,
                            motherPhotoType ?? "photo.jpg",
                            PhotoModule.Parent,
                            studentid
                    );
                }

                PhotoUploadResult photoGuardianResult = new PhotoUploadResult();
                if (photoMotherPhotoBase64 != null && photoMotherPhotoBase64 != "") 
                {
                   photoGuardianResult = await _photoService.SaveBase64PhotoAsync(
                       photoGuardianPhotoBase64,
                       guardianPhotoType ?? "photo.jpg",
                       PhotoModule.Parent,
                       studentid
                   );
                }
                   

                if (photoResult.Success)
                {
                    var model = new ProfileRequest();
                    model.Id =studentid;
                    model.PhotoDoc = photoResult.PhotoUrl;
                    model.MotherPhoto= photoMotherResult.PhotoUrl;
                    model.FatherPhoto= photoFatherResult.PhotoUrl;
                    model.GuardianPhoto = photoGuardianResult.PhotoUrl;
                    await _studentService.UpdateStudentProfileAsync(model);
                }
            }
            return Json(new { success = res.Success, message = res.Message });
        }

        [HttpGet]
        public async Task<IActionResult> GetPickupPoints(int routeId)
        {
            var companyId =await GetCompanyId();
            var sessionId = await GetSessionId();
            var allLinks =await _routePickupPointService.GetAllRoutePickupPointsAsync();
            var points = allLinks.Data.Where(l => l.RouteID == routeId && l.IsActive)
                                 .Select(l => new { id = l.PickupPointID, name = l.PickupPointName })
                                 .ToList();
            return Json(points);
        }

        [HttpGet]
        public async Task<IActionResult> GetHostelRooms(int hostelId)
        {
            var companyId =await GetCompanyId();
            var sessionId = await GetSessionId();
            var allRooms =await _hostelService.GetAllHostelRoomsAsync();
            var rooms = allRooms.Data.Where(r => r.HostelID == hostelId && r.IsActive)
                                .Select(r => new { id = r.RoomId, name = r.RoomTitle })
                                .ToList();
            return Json(rooms);
        }

        [HttpGet]
        public async Task<IActionResult> GetSections(int classId)
        {
            var sections =await _sectionService.GetByClassAsync(classId);
            var result = sections.Data.Where(s => s.IsActive)
                                 .Select(s => new { id = s.SectionID, name = s.SectionName })
                                 .ToList();
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudent([FromBody] List<int> ids)
        {
            var res =await _studentService.BulkDeleteStudents(ids);
            return Json(new { success =true, message = res.Message });
        }

        public IActionResult MultiClassStudents()
        {
            return View();
        }

        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> DisabledStudents(int? classId, int? sectionId, string? search)
        {
            // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
            var perms = await GetPermissions(
               "/StudentInformation/DisabledStudents"
           );
            var companyId =await GetCompanyId();
            var sessionId = await GetSessionId();

            var model = new StudentListPageViewModel
            {
                //Students =await _studentService.GetDisabledStudentList(classId, sectionId, search),
                SelectedClassId = classId,
                SelectedSectionId = sectionId,
                SearchTerm = search
            };
            var response = await _studentService.GetDisabledStudentList(classId,sectionId,search, sessionId);

            if (response.Success)
            {
                model.Students = response.Data ?? new List<StudentListViewModel>();
            }

            ViewBag.Classes = (await _classService.GetAllAsync()).Data;
            if (classId.HasValue)
            {
                ViewBag.Sections =(await _sectionService.GetByClassAsync(classId.Value)).Data;
            }
            else
            {
                ViewBag.Sections = new List<MstSectionViewModel>();
            }
            model.Permissions = perms;
            return View(model);
        }

        public async Task<IActionResult> BulkDelete()
        {
            // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
            var perms = await GetPermissions(
               "/StudentInformation/BulkDelete"
            );
            var companyId = await GetCompanyId();
            var sessionId = await GetSessionId();
            ViewBag.Classes = (await _classService.GetAllAsync(false, sessionId)).Data;
            return View(perms);
        }

        [HttpPost]
        public async Task<IActionResult> BulkDeleteStudents([FromBody] List<int> ids)
        {
            var res =await _studentService.BulkDeleteStudents(ids);
            return Json(new { success = res.Success, message = res.Message });
        }

        public async Task<IActionResult> ImportStudent()
        {
            var companyId =await GetCompanyId();
            var sessionId = await GetSessionId();
            ViewBag.Classes = (await _classService.GetAllAsync(false,sessionId)).Data;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ImportStudent(int classId, int sectionId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Please select a CSV file." });

            try
            {
                var companyId = await GetCompanyId();
                var sessionId =await GetSessionId();
                var userId =  GetUserId();

                var categories =await _studentService.GetAllStudentCategories(sessionId);
                var houses =await _studentService.GetAllStudentHouses(sessionId);

                using var reader = new StreamReader(file.OpenReadStream());
                string? headerLine = await reader.ReadLineAsync();
                if (headerLine == null) return Json(new { success = false, message = "File is empty." });

                var headers = headerLine.Split(',').Select(h => h.Trim().ToLower()).ToList();
                var results = new List<object>();

                while (!reader.EndOfStream)
                {
                    string? line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = line.Split(',').Select(v => v.Trim()).ToList();
                    var fieldValues = new Dictionary<string, string>();

                    fieldValues["Class"] = classId.ToString();
                    fieldValues["Section"] = sectionId.ToString();

                    for (int i = 0; i < headers.Count && i < values.Count; i++)
                    {
                        string header = headers[i];
                        string value = values[i];
                        string mappedKey = MapCsvHeaderToKey(header);

                        if (!string.IsNullOrEmpty(mappedKey))
                        {
                            if (mappedKey == "Category")
                            {
                                var cat = categories.Data.FirstOrDefault(c => string.Equals(c.StudentCategoryName, value, StringComparison.OrdinalIgnoreCase));
                                value = cat?.StudentCategoryID.ToString() ?? "";
                            }
                            else if (mappedKey == "House")
                            {
                                var house = houses.Data.FirstOrDefault(h => string.Equals(h.StudentHouseName, value, StringComparison.OrdinalIgnoreCase));
                                value = house?.StudentHouseID.ToString() ?? "";
                            }
                            fieldValues[mappedKey] = value;
                        }
                    }

                    var req = new StudentAdmissionUpsertRequest { StudentID = 0, FieldValues = fieldValues };
                    var res = await _studentService.UpsertStudentAdmission(req);
                    results.Add(new
                    {
                        admissionNo = fieldValues.GetValueOrDefault("Admission No"),
                        name = (fieldValues.GetValueOrDefault("First Name") + " " + fieldValues.GetValueOrDefault("Last Name")).Trim(),
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

        private string MapCsvHeaderToKey(string header)
        {
            return header switch
            {
                "admission_no" => "Admission No",
                "roll_no" => "Roll No",
                "first_name" => "First Name",
                "middlename" => "Middle Name",
                "last_name" => "Last Name",
                "gender" => "Gender",
                "date_of_birth" => "Date of Birth",
                "category" => "Category",
                "religion" => "Religion",
                "caste" => "Caste",
                "mobile_no" => "Mobile Number",
                "email" => "Email",
                "admission_date" => "Admission Date",
                "blood_group" => "Blood Group",
                "student_house" => "House",
                "height" => "Height",
                "weight" => "Weight",
                "father_name" => "Father Name",
                "father_phone" => "Father Phone",
                "father_occupation" => "Father Occupation",
                "mother_name" => "Mother Name",
                "mother_phone" => "Mother Phone",
                "mother_occupation" => "Mother Occupation",
                "guardian_is" => "If Guardian Is",
                "guardian_name" => "Guardian Name",
                "guardian_relation" => "Guardian Relation",
                "guardian_email" => "Guardian Email",
                "guardian_phone" => "Guardian Phone",
                "guardian_occupation" => "Guardian Occupation",
                "current_address" => "CurrentAddress",
                "permanent_address" => "PermanentAddress",
                _ => ""
            };
        }

        #region Extracted API Endpoints

        [HttpGet]
        public async Task<IActionResult> GetStudentAttendanceHistory(int id, int year)
        {
            try
            {
                var data =await _attendanceService.GetStudentAttendanceHistoryAsync(id, year, await GetCompanyId());
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDisableReasons()
        {
            var sessionId = await GetSessionId();
            var data =await _studentService.GetAllDisableReasons(sessionId);
            return Json(new { success = true, data });
        }

        [HttpPost]
        public async Task<IActionResult> UpsertDisableReason([FromBody] StudentDisableReasonUpsertRequest req)
        {
            var res = await _studentService.UpsertDisableReason(req);
            return Json(new { success = res.Success, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDisableReason([FromBody] List<int> ids)
        {
            var res =await _studentService.DeleteDisableReason(ids);
            return Json(new { success = true, message = res.Message });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStudentHouses()
        {
            var sessionId = await GetSessionId();
            var data =await _studentService.GetAllStudentHouses(sessionId);
            return Json(new { success = true, data });
        }

        [HttpPost]
        public async Task<IActionResult> UpsertStudentHouse([FromBody] StudentHouseUpsertRequest req)
        {
            var res =await _studentService.UpsertStudentHouse(req);
            return Json(new { success = res.Success, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudentHouse([FromBody] List<int> ids)
        {
            var res =await _studentService.DeleteStudentHouse(ids);
            return Json(new { success = res.Success, message = res.Message });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStudentCategories()
        {
            var sessionId = await GetSessionId();
            var data = await _studentService.GetAllStudentCategories(sessionId);
            return Json(new { success = true, data });
        }

        [HttpPost]
        public async Task<IActionResult> UpsertStudentCategory([FromBody] StudentCategoryUpsertRequest req)
        {
            req.SessionID = await GetSessionId();
            var res =await _studentService.UpsertStudentCategory(req);
            return Json(new { success = res.Success, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudentCategory([FromBody] List<int> ids)
        {
            var res = await _studentService.DeleteStudentCategory(ids);
            return Json(new { success = res.Success, message = res.Message });
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentTimeline(int id)
        {
            try
            {
                var data =await _studentService.GetStudentTimeline(id);
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Database Error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpsertTimeline([FromBody] StudentTimelineUpsertRequest req)
        {
            try
            {
                var result =await _studentService.UpsertTimeline(req);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Database Error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTimeline(int id)
        {
            try
            {
                var result =await _studentService.DeleteTimeline(id);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Database Error: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadTimelineDoc(int id)
        {
            var response = await _studentService.DownloadTimelineDoc(id);

            if (!response.Success || response.Data == null)
                return NotFound();

            return File(
                response.Data.FileBytes,
                response.Data.ContentType,
                response.Data.FileName);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus([FromBody] StudentStatusToggleRequest req)
        {
            var res = await _studentService.ToggleStatus(req);
            return Json(new { success = res.Success, message = res.Message });
        }

        [HttpGet]
        public async Task<IActionResult> GetByID(int id)
        {
            try
            {
                var data = await _studentService.GetStudentByIDAsync(id);
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMultiClassStudents(int? classId, int? sectionId, string? searchTerm)
        {
            var data = await _studentService.GetMultiClassStudents(classId, sectionId, searchTerm);
            return Json(new { success = true, data });
        }

        [HttpPost]
        public async Task<IActionResult> UpsertMultiClass([FromBody] StudentMultiClassUpsertRequest req)
        {
            var res = await _studentService.UpsertMultiClass(req);
            return Json(new { success = res.Success, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMultiClass(int id)
        {
            var res =await _studentService.DeleteMultiClass(id);
            return Json(new { success = res.Success, message = res.Message });
        }

        [HttpGet]
        public async Task<IActionResult> GetClasses()
        {
            var data =await _classService.GetAllAsync();
            return Json(new { success = true, data });
        }

        [HttpGet]
        public async Task<IActionResult> GetSectionsByClass(int id)
        {
            var response = await _sectionService.GetByClassAsync(id);
            return Json(new { response.Success, response.Data });
        }

        #endregion

        public async Task<IActionResult> AddStudentCategory(int? id) 
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                    "/StudentInformation/StudentHouse"
                );
                // Step 1: Initialize a new blank page model.
                var model = new StudentCategoryAddViewModel();
                if (id.HasValue && id.Value > 0)
                {
                    var response = await _studentService.GetStudentCategoryByIdAsync(id.Value);
                    if (response.Success)
                    {
                        model.Items = response.Data;
                        model.EditStudentCategory = response.Data;
                    }
                }
                else
                {
                    model.EditStudentCategory = null;
                }

                
                
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IActionResult> AddStudentHouse(int? id)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                    "/StudentInformation/StudentHouse"
                );
                // Step 1: Initialize a new blank page model.
                var model = new StudentHouseAddViewModel(); //StudentHouseViewModel
                if (id.HasValue && id.Value > 0)
                {
                    var response = await _studentService.GetStudentHouseByIdAsync(id.Value);
                    if (response.Success)
                    {
                        model.Items = response.Data;
                        model.EditStudentHouse = response.Data;
                    }
                }
                else
                {
                    model.EditStudentHouse = null;
                }

                model.Permissions = perms;
                return View(model);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IActionResult> AddDisableReason(int? id)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                    "/StudentInformation/StudentHouse"
                );
                // Step 1: Initialize a new blank page model.
                var model = new StudentDisableReasonAddViewModel(); //StudentHouseViewModel
                if (id.HasValue && id.Value > 0)
                {
                    var response = await _studentService.GetDisableReasonsByID(id.Value);
                    if (response.Success)
                    {
                        model.Items = response.Data;
                        model.EditStudentDisableReason = response.Data;
                    }
                }
                else
                {
                    model.EditStudentDisableReason = null;
                }

                model.Permissions = perms;
                return View(model);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
