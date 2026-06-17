using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Services;
using SchoolERP.Net.Models;
using System.Security.Claims;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;
using SchoolERP.Net.Services.Clients;
using SchoolERP.Net.Helpers;

namespace SchoolERP.Net.Controllers
{
    /// <summary>
    /// This controller manages all the web pages related to student information.
    /// It handles pages for student admission, viewing student lists, student categories, 
    /// houses, and other student-specific settings.
    /// Think of this as the manager who decides which page to show when you click on student-related menus.
    /// </summary>
    public class StudentInformationController : Controller
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
        //private readonly PermissionHelper _permHelper;

        public StudentInformationController(IStudentInformationClientService studentService,
            ICompanyClientService companyService, ISessionClientService sessionService, 
            IFieldClientService fieldService, IRouteClientService routeService, 
            IRoutePickupPointClientService routePickupPointService,
            IHostelClientService hostelService, IClassClientService classService,
            ISectionClientService sectionService, IVehicleAssignClientService vehicleAssignService, 
            IAttendanceClientService attendanceService)
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
        }


        private int GetUserId() => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("UserId")?.Value, out var id) ? id : 0;
        private async Task<int> GetCompanyId()
        {
            var response = await _companyService.GetUserCurrentCompanyAsync();
            return response?.Data ?? 0;
        }
        private async Task<int> GetSessionId()
        {
            var response = await _sessionService.GetUserCurrentSessionAsync();
            return response?.Data ?? 0;
        }

        /// <summary>
        /// Shows the page where the school can manage reasons for disabling students (e.g., 'Transfer Certificate issued').
        /// </summary>
        public async Task<IActionResult> DisableReason()
        {
            var response = await _studentService.GetAllDisableReasons();

            var model = new StudentDisableReasonPageViewModel
            {
                Items = response.Data ?? new List<StudentDisableReasonViewModel>()
            };
            return View(model);
        }

        /// <summary>
        /// Shows the page for managing student houses (e.g., Red House, Blue House).
        /// </summary>
        public async Task<IActionResult> StudentHouse()
        {
            var response = await _studentService.GetAllStudentHouses();
            StudentHousePageViewModel model = new StudentHousePageViewModel();
            model.Items = response.Data;
            return View(model);
        }

        /// <summary>
        /// Shows the page for managing student categories (e.g., General, OBC).
        /// </summary>
        public async Task<IActionResult> StudentCategory()
        {
            var res = (await _studentService.GetAllStudentCategories()).Data;
            StudentCategoryPageViewModel model= new StudentCategoryPageViewModel();
            model.Items = res;
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
            ViewBag.Categories = (await _studentService.GetAllStudentCategories()).Data;
            ViewBag.Houses = (await _studentService.GetAllStudentHouses()).Data;
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
                    ViewBag.NextRollNo = (await _studentService.GetNewStudentRollNo()).Data;
                }
            }

            return View(fields.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentList(int? classId, int? sectionId, string? searchTerm, int? excludeStudentId)
        {
            var companyId = GetCompanyId();
            var sessionId = GetSessionId();
            var response = await _studentService.GetStudentListAsync(classId, sectionId, searchTerm);
            var students = response.Data ?? new List<StudentListViewModel>();

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
        [HttpPost]
        /// <summary>
        /// Shows the list of all students. 
        /// Users can filter the list by class, section, or search for a specific name/roll number.
        /// </summary>
        public async Task<IActionResult> Students(int? classId, int? sectionId, string? search, string? viewType)
        {
            var companyId =await GetCompanyId();
            var sessionId = await GetSessionId();
            
            var allFieldsresponse =await _fieldService.GetAllFieldsAsync(companyId, sessionId, belongsTo: "students");
            List<FieldModel> allFields = new List<FieldModel>();
            if (allFieldsresponse.Success) 
            {
                allFields = allFieldsresponse.Data;
            }
            var response = await _studentService.GetStudentListAsync(classId, sectionId, search);            
            var model = new StudentListPageViewModel
            {
                Students = response.Data,
                SelectedClassId = classId,
                SelectedSectionId = sectionId,
                SearchTerm = search,
                ViewType = viewType ?? "list",
                SystemFields = allFields.Where(f => f.IsSystemField).ToList(),
                CustomFields = allFields.Where(f => !f.IsSystemField).ToList()
            };

            ViewBag.Classes =(await _classService.GetAllAsync()).Data;
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
            var response = await _studentService.GetNewStudentRollNo();
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
            var res =await _studentService.UpsertStudentAdmission(req);
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
            var companyId =await GetCompanyId();
            var sessionId = await GetSessionId();

            var model = new StudentListPageViewModel
            {
                //Students =await _studentService.GetDisabledStudentList(classId, sectionId, search),
                SelectedClassId = classId,
                SelectedSectionId = sectionId,
                SearchTerm = search
            };
            var response = await _studentService.GetDisabledStudentList(classId,sectionId,search);

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

            return View(model);
        }

        public async Task<IActionResult> BulkDelete()
        {
            var companyId = await GetCompanyId();
            var sessionId = await GetSessionId();
            ViewBag.Classes = (await _classService.GetAllAsync()).Data;
            return View();
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
            ViewBag.Classes = (await _classService.GetAllAsync()).Data;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ImportStudent(int classId, int sectionId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Please select a CSV file." });

            try
            {
                var companyId = GetCompanyId();
                var sessionId = GetSessionId();
                var userId = GetUserId();

                var categories =await _studentService.GetAllStudentCategories();
                var houses =await _studentService.GetAllStudentHouses();

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
            var data =await _studentService.GetAllDisableReasons();
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
            var data =await _studentService.GetAllStudentHouses();
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
            var data = await _studentService.GetAllStudentCategories();
            return Json(new { success = true, data });
        }

        [HttpPost]
        public async Task<IActionResult> UpsertStudentCategory([FromBody] StudentCategoryUpsertRequest req)
        {
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
    }
}
