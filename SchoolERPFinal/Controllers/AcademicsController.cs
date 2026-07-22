using Microsoft.AspNetCore.Mvc;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using SchoolERP.Net.Services;
using SchoolERP.Net.Services.Clients;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Net.Helpers;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.Design;

namespace SchoolERP.Net.Controllers
{
    /// <summary>
    /// This controller manages the school's academic structure, including Classes, Subjects, and how they are grouped together.
    /// </summary>
    public class AcademicsController : BaseController
    {
        private readonly IClassClientService _classClient;
        private readonly ISectionClientService _sectionClient;
        private readonly ISubjectClientService _subjectClient;
        private readonly ISubjectGroupClientService _subjectGroupClient;
        private readonly IAcademicsClientService _academicsClient;
        private readonly IHumanResourceClientService _hrClient;
        private readonly ISessionClientService _sessionClient;
        private readonly IUserMenuPermissionClientService _menuPerm;
        private readonly ICompanyClientService _companyService;
        private const string ClassMenuPath = "/Academics/Class";
        private const string SubjectMenuPath = "/Academics/Subject";
        private const string SubjectGroupMenuPath = "/Academics/SubjectGroup";
        private const string TimeTableMenuPath = "/Academics/AddTimeTable";

        public AcademicsController(
            IClassClientService classClient, 
            ISectionClientService sectionClient, 
            ISubjectClientService subjectClient, 
            ISubjectGroupClientService subjectGroupClient,
            IAcademicsClientService academicsClient,
            IHumanResourceClientService hrClient,
            ISessionClientService sessionClient,
            IUserMenuPermissionClientService menuPerm, ICompanyClientService companyService, PermissionHelper permHelper) : base(permHelper)
        {
            _classClient = classClient;
            _sectionClient = sectionClient;
            _subjectClient = subjectClient;
            _subjectGroupClient = subjectGroupClient;
            _academicsClient = academicsClient;
            _hrClient = hrClient;
            _sessionClient = sessionClient;
            _menuPerm = menuPerm;
            _companyService = companyService;
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
                var response = await _sessionClient.GetUserCurrentSessionAsync();
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
        /// Shows the 'Class' management page where you can define the different grades or classes in the school.
        /// </summary>
        public async Task<IActionResult> Class(int? pageIndex,
        int? pageSize,
        string? search,
        int? companyId,
        int? sessionID,
        int? sectionId,
        bool? isActive,
        int? userTypeId)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Academics/Class"
               );

                var request = new ClassSearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionID = sessionID ?? await GetSessionId(),
                    SectionID=sectionId??null
                };

                var sessionId = await GetSessionId();
                var classesResponse = _classClient.GetAllClassWithPageAsync(request);
                var sectionsResponse = await _sectionClient.GetAllAsync(false, request.SessionID, request.CompanyID);

                var sessionTask = _sessionClient.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();




                await Task.WhenAll(classesResponse, sessionTask, companiesTask);

                var pagedResult = await classesResponse;

                var model = new MstClassPageViewModel
                {
                    Classes = pagedResult.Success ? pagedResult.Data.Data : new List<MstClassViewModel>(),
                    AvailableSections = sectionsResponse.Success ? sectionsResponse.Data : new List<MstSectionViewModel>(),
                    Companies = (await companiesTask).Data ?? new(),
                    Sessions = (await sessionTask).Data ?? new(),
                    TotalRecords = pagedResult.Data.TotalRecords,
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,
                    SearchTerm = search,
                    CompanyId = companyId,
                    SessionId = sessionId
                };
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }           
        }

        [HttpGet]
        public async Task<IActionResult> GetClass(int id)
        {
            if (!(await _menuPerm.Has(ClassMenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit classes." });
            var response = await _classClient.GetByIDAsync(id);
            if (!response.Success) return Json(new { success = false, message = response.Message });
            return Json(new { success = true, data = response.Data });
        }

        [HttpPost]
        public async Task<IActionResult> SaveClass([FromBody] MstClassUpsertRequest request)
        {
            try
            {
                var isCreate = request.ClassID <= 0;
                request.CompanyId = await GetCompanyId();
                request.SessionId = await GetSessionId();

                if (isCreate && !(await _menuPerm.Has(ClassMenuPath, "Add")).Data)
                    return Json(new { success = false, message = "You do not have permission to add classes." });
                if (!isCreate && !(await _menuPerm.Has(ClassMenuPath, "Edit")).Data)
                    return Json(new { success = false, message = "You do not have permission to edit classes." });
                var response = await _classClient.UpsertAsync(request);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> ToggleClassStatus([FromBody] StatusUpdateRequest request)
        {
            var response = await _classClient.ToggleStatusAsync(request);
            return Json(new { success = response.Success, message = response.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteClass([FromBody] List<int> ids)
        {
            var response = await _classClient.DeleteAsync(ids);
            return Json(new { success = response.Success, message = response.Message });
        }

        /// <summary>
        /// Shows the 'Subject' management page where you can define all the subjects taught in the school (like Math, Science, or English).
        /// </summary>
      
        public async Task<IActionResult> Subject(int? pageIndex,
        int? pageSize,
        string? search,
        int? companyId,
        int? sessionID,
        bool? isActive,
        int? userTypeId)
        {
            try
            {
                // Retrieves the logged-in Subject's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Academics/Subject"
               );
                var request = new SubjectSearchRequest
                {
                    PageNumber = pageIndex??1,
                    PageSize = pageSize??10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionID = sessionID ?? await GetSessionId()
                };
                var sessionId = await GetSessionId();
               
                // ── Parallel fetch — dropdowns + paged users ─────────────────
                var subjectTask = _subjectClient.GetAllAsync(request);
                var sessionTask = _sessionClient.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();

                await Task.WhenAll(subjectTask, sessionTask, companiesTask);

                var pagedResult = await subjectTask;
                var model = new MstSubjectPageViewModel
                {
                    Subjects = pagedResult.Success ? pagedResult.Data.Data : new List<MstSubjectViewModel>(),
                    TotalRecords = pagedResult.Data.TotalRecords,
                    PageNumber = pagedResult.Data.PageNumber,
                    PageSize = pagedResult.Data.PageSize,
                    SearchTerm = search,
                    CompanyId = companyId,
                    SessionId=sessionId,
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

        [HttpGet]
        public async Task<IActionResult> GetSubject(int id)
        {
            var response = await _subjectClient.GetByIDAsync(id);
            if (!response.Success) return Json(new { success = false, message = response.Message });
            return Json(new { success = true, data = response.Data });
        }

        [HttpPost]
        public async Task<IActionResult> SaveSubject([FromBody] MstSubjectUpsertRequest request)
        {
            var response = await _subjectClient.UpsertAsync(request);
            return Json(new { success = response.Success, message = response.Message });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleSubjectStatus([FromBody] StatusUpdateRequest request)
        {
            try
            {
                var response = await _subjectClient.ToggleStatusAsync(request);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message});
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSubject([FromBody] List<int> ids)
        {
            var response = await _subjectClient.DeleteAsync(ids);
            return Json(new { success = response.Success, message = response.Message });
        }

        [HttpGet]
        public async Task<IActionResult> GetSectionsByClass(int id)
        {
            try
            {
                var response = await _sectionClient.GetByClassAsync(id);

                var role = User.FindFirst("UserTypeName")?.Value;
                if (role.Trim() == "Student")
                {
                    
                    var sectionId = int.Parse(User.FindFirst("SectionID")?.Value);

                    var sectionResponse = await _sectionClient.GetByIDAsync(sectionId);

                    if (sectionResponse.Success && sectionResponse.Data != null)
                    {
                        // Replace the list inside response.Data with just this one section
                        response.Data = new List<MstSectionViewModel> { sectionResponse.Data };
                    }
                    else
                    {
                        // Student has no matching section — return empty list rather than the full unfiltered list
                        response.Data = new List<MstSectionViewModel>();
                    }
                }

                if (!response.Success) return Json(new { success = false, message = response.Message });
                return Json(new { success = true, data = response.Data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message});
            }
            
        }

        [HttpGet]
        public async Task<IActionResult> GetSubjectGroups(int classId, int sectionId)
        {
            var response = await _subjectGroupClient.GetAllAsync();
            if (!response.Success) return Json(new { success = false, message = response.Message });

            var groups = response.Data.Where(g => g.ClassID == classId && g.SectionIds.Contains(sectionId.ToString())).ToList();
            return Json(new { success = true, data = groups });
        }

        /// <summary>
        /// Shows the 'Subject Group' page where you can group subjects together for specific classes.
        /// </summary>
        public async Task<IActionResult> SubjectGroup()
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Academics/SubjectGroup"
               );
                var sessionId = await GetSessionId();
                var request = new DropdowRequest
                {
                    CompanyID = await GetCompanyId(),
                    SessionID = sessionId
                };



                var groupsResponse = await _subjectGroupClient.GetAllAsync(false, sessionId);
                var classesResponse = await _classClient.GetAllAsync(false, sessionId);
                var sectionsResponse = await _sectionClient.GetAllAsync(false, sessionId);
                var subjectsResponse = await _subjectClient.SubjectsDropdowBindAsync(request);
                var model = new MstSubjectGroupPageViewModel
                {
                    SubjectGroups = groupsResponse.Success ? groupsResponse.Data : new List<MstSubjectGroupViewModel>(),
                    Classes = classesResponse.Success ? classesResponse.Data : new List<MstClassViewModel>(),
                    Sections = sectionsResponse.Success ? sectionsResponse.Data : new List<MstSectionViewModel>(),
                    Subjects = subjectsResponse.Success ? subjectsResponse.Data : new List<Dropdowbinding>()
                };
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception ex)
            {
                throw;
            }
           
        }

        [HttpGet]
        public async Task<IActionResult> GetSubjectGroup(int id)
        {
            var response = await _subjectGroupClient.GetByIDAsync(id);
            if (!response.Success) return Json(new { success = false, message = response.Message });
            return Json(new { success = true, data = response.Data });
        }

        [HttpPost]
        public async Task<IActionResult> SaveSubjectGroup([FromBody] MstSubjectGroupUpsertRequest request)
        {
            var response = await _subjectGroupClient.UpsertAsync(request);
            return Json(new { success = true, message = response.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSubjectGroup([FromBody] List<int> ids)
        {
            var response = await _subjectGroupClient.DeleteAsync(ids);
            return Json(new { success = response.Success, message = response.Message });
        }

        [HttpGet]
        /// <summary>
        /// Shows the page for creating or updating the school timetable.
        /// </summary>
        public async Task<IActionResult> AddTimeTable()
        {
            try
            {
                var sessionId = await GetSessionId();
                var request = new DropdowRequest
                {
                    CompanyID = await GetCompanyId(),
                    SessionID = sessionId
                };
                var staffId = GetStaffID();
                if (staffId == null || staffId == 0)
                {
                    staffId = GetStaffID();
                    staffId = (staffId ?? 0) <= 0 ? null : staffId;
                }
                var classes = await _classClient.GetAllAsync();
                var subjectsResponse = await _subjectClient.SubjectsDropdowBindAsync(request);
                var staff = await _hrClient.GetAllStaffAsync(request.CompanyID,request.SessionID, staffId);
                var subjectGroups = await _subjectGroupClient.GetAllAsync();

                var model = new AddTimeTablePageViewModel
                {
                    Classes = classes.Success ? classes.Data : new List<MstClassViewModel>(),
                    Subjects = subjectsResponse.Success ? subjectsResponse.Data : new List<Dropdowbinding>(),
                    Staff = staff.Success ? staff.Data : new List<HRStaffViewModel>(),
                    SubjectGroups = subjectGroups.Success ? subjectGroups.Data : new List<MstSubjectGroupViewModel>()
                };

                return View(model);
            }
            catch (Exception ex)
            {

                throw;
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> AddTimeTable(int classId, int sectionId, int subjectGroupId = 0)
        {
            try
            {
                var sessionId = await GetSessionId();
                var request = new DropdowRequest
                {
                    CompanyID = await GetCompanyId(),
                    SessionID = sessionId
                };

                var staffId = GetStaffID();
                if (staffId == null || staffId == 0)
                {
                    staffId = (staffId ?? 0) <= 0 ? null : staffId;
                }
                var classes = await _classClient.GetAllAsync();
                var subjectsResponse = await _subjectClient.SubjectsDropdowBindAsync(request);
                var staff = await _hrClient.GetAllStaffAsync(request.CompanyID,request.SessionID, staffId);
                var subjectGroups = await _subjectGroupClient.GetAllAsync();

                var allSubjects = subjectsResponse.Success ? subjectsResponse.Data : new List<Dropdowbinding>();
                var allSubjectGroups = subjectGroups.Success ? subjectGroups.Data : new List<MstSubjectGroupViewModel>();

                if (subjectGroupId > 0)
                {
                    var groupResponse = await _subjectGroupClient.GetByIDAsync(subjectGroupId);
                    if (groupResponse.Success && groupResponse.Data != null && groupResponse.Data.SubjectIds != null && groupResponse.Data.SubjectIds.Any())
                    {
                        allSubjects = allSubjects.Where(s => groupResponse.Data.SubjectIds.Contains(s.SubjectID.ToString())).ToList();
                    }
                }

                var model = new AddTimeTablePageViewModel
                {
                    Classes = classes.Success ? classes.Data : new List<MstClassViewModel>(),
                    Subjects = allSubjects,
                    Staff = staff.Success ? staff.Data : new List<HRStaffViewModel>(),
                    SubjectGroups = allSubjectGroups,
                    SelectedClassId = classId,
                    SelectedSectionId = sectionId,
                    SelectedSubjectGroupId = subjectGroupId
                };

                if (classId > 0 && sectionId > 0)
                {
                    var slots = await _academicsClient.GetTimeTableByClassAsync(classId, sectionId);
                    model.TimeTableSlots = slots.Success ? slots.Data : new List<TimeTableViewModel>();
                }

                return View(model);
            }
            catch (Exception ex)
            {

                throw;
            }
           
        }

        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> ClassTimeTable(int classId = 0, int sectionId = 0)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Academics/ClassTimeTable"
               );
                var sessionId = await GetSessionId();
                var classes = await _classClient.GetAllAsync(false, sessionId);
                var model = new AddTimeTablePageViewModel
                {
                    Classes = classes.Success ? classes.Data : new List<MstClassViewModel>(),
                    SelectedClassId = classId,
                    SelectedSectionId = sectionId
                };
                var role = User.FindFirst("UserTypeName")?.Value;
                if(role.Trim() == "Student") 
                {
                    classId=int.Parse(User.FindFirst("ClassID")?.Value);
                    sectionId = int.Parse(User.FindFirst("SectionID")?.Value);
                    model.SelectedClassId = classId;
                    model.SelectedSectionId = sectionId;

                    var classResponse = await _classClient.GetByIDAsync(classId);

                    if (classResponse.Success && classResponse.Data != null)
                    {
                        // Wrap the single record into a list
                        model.Classes = new List<MstClassViewModel> { classResponse.Data };
                    }
                }

                if (classId > 0 && sectionId > 0)
                {
                    var slots = await _academicsClient.GetTimeTableByClassAsync(classId, sectionId);
                    model.TimeTableSlots = slots.Success ? slots.Data : new List<TimeTableViewModel>();
                }
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception)
            {

                throw;
            }
           
        }

        [HttpGet]
        public async Task<IActionResult> GetTimeTable(int classId, int sectionId)
        {
            var response = await _academicsClient.GetTimeTableByClassAsync(classId, sectionId);
            return Json(new { success = response.Success, data = response.Data, message = response.Message });
        }

        [HttpPost]
        public async Task<IActionResult> SaveTimeTable([FromBody] TimeTableUpsertRequest request)
        {
            var response = await _academicsClient.UpsertTimeTableAsync(request);
            return Json(new { success = response.Success, message = response.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTimeTable(int id)
        {
            var response = await _academicsClient.DeleteTimeTableSlotAsync(id);
            return Json(new { success = response.Success, message = response.Message });
        }

       
        [HttpGet]
        public async Task<IActionResult> TeacherTimeTable()
        {
            var sessionId = await GetSessionId();
            // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
            var perms = await GetPermissions(
               "/Academics/TeacherTimeTable"
           );
            var companyId = await GetCompanyId();
            var staffId = GetStaffID();
            if (staffId == null || staffId == 0)
            {
                staffId = GetStaffID();
                staffId = (staffId ?? 0) <= 0 ? null : staffId;
            }
            var staff = await _hrClient.GetAllStaffAsync(companyId,sessionId, staffId);

            var model = new TeacherTimeTablePageViewModel
            {
                Staff = staff.Success ? staff.Data : new List<HRStaffViewModel>(),
                SelectedStaffId = staffId ?? 0
            };
            model.Permissions = perms;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> TeacherTimeTable(int staffId)
        {

            var sessionId = await GetSessionId();
            var companyId = await GetCompanyId();
            var staff = await _hrClient.GetAllStaffAsync(companyId,sessionId, staffId);

            var model = new TeacherTimeTablePageViewModel
            {
                Staff = staff.Success ? staff.Data : new List<HRStaffViewModel>(),
                SelectedStaffId = staffId
            };

            if (staffId > 0)
            {
                var req = new TimeTableSearchRequest
                {
                    CompanyID=companyId,
                    SessionID=sessionId,
                    StaffID=staffId
                };
                var slots = await _academicsClient.GetTimeTableByStaffAsync(req);
                model.TimeTableSlots = slots.Success ? slots.Data : new List<TimeTableViewModel>();
            }

            return View(model);
        }

        public async Task<IActionResult> AssignClassTeacher(int? pageIndex,
        int? pageSize,
        string? search,
        int? companyId,
        int? sessionID,
        string? sectionId,
        string? classId)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Academics/AssignClassTeacher"
               );


                var request = new AcademicsSearchRequest
                {
                    PageNumber = pageIndex ?? 1,
                    PageSize = pageSize ?? 10,
                    SearchKeyword = search,
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionID = sessionID ?? await GetSessionId(),
                    SectionID = sectionId ?? null,
                    ClassIDs = classId ?? null
                };

                var sessionId = await GetSessionId();
                var staffId = GetStaffID();
                var classes = await _classClient.GetAllAsync(false, sessionId);
                var staff = await _hrClient.GetAllStaffAsync(request.CompanyID,request.SessionID, staffId);
                var assignments =  _academicsClient.GetAllClassWithPageAsync(request);


                var sessionTask = _sessionClient.GetAllAsync();
                var companiesTask = _companyService.GetAllAsync();

                await Task.WhenAll(assignments, sessionTask, companiesTask);

                var pagedResult = await assignments;

                var model = new AssignClassTeacherPageViewModel
                {
                    Classes = classes.Success ? classes.Data : new List<MstClassViewModel>(),
                    Staff = staff.Success ? staff.Data : new List<HRStaffViewModel>(),
                    Assignments = pagedResult.Success ? pagedResult.Data.Data : new List<ClassTeacherViewModel>(),
                    Companies = (await companiesTask).Data ?? new(),
                    Sessions = (await sessionTask).Data ?? new(),
                    TotalRecords = pagedResult.Data.TotalRecords,
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
                throw ex;
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> SaveClassTeacher([FromBody] ClassTeacherUpsertRequest request)
        {
            try
            {
                request.CompanyID = await GetCompanyId();
                request.SessionID = await GetSessionId();

                var response = await _academicsClient.UpsertClassTeacherAsync(request);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> DeleteClassTeacher(List<int> id)
        {
            var response = await _academicsClient.DeleteClassTeacherAsync(id);
            return Json(new { success = response.Success, message = response.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteClassTeacherByClassSection(List<int> classId, List<int> sectionId)
        {
            try
            {
                var assignmentsResponse = await _academicsClient.GetAllClassTeachersAsync(classId, sectionId);
                if (assignmentsResponse.Success)
                {
                    List<int> classTeacherIds = new List<int>();

                    foreach (var item in assignmentsResponse.Data)
                    {
                        classTeacherIds.Add(item.ClassTeacherID);
                    }
                    var resp=await _academicsClient.DeleteClassTeacherAsync(classTeacherIds);
                    return Json(new { success = resp.Success, message = resp.Message ?? "Failed to load assignments." });
                }
                else 
                {
                    return Json(new { success = false, message ="Failed to load assignments." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message});
            }
            
        }

        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> PromoteStudents(int? classId, int? sectionId, int? pageIndex = null,
        int? pageSize = null,
        string? search = null,
        int? companyId = null,
        int? sessionID = null)
        {
            var model = new PromoteStudentsPageViewModel();
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Academics/PromoteStudents"
               );


                var reqModel = new SearchPromotedStudent
                {
                    CompanyID = companyId ?? await GetCompanyId(),
                    SessionID = sessionID ?? await GetSessionId(),
                    ClassID = classId ?? null,
                    SectionID = sectionId ?? null,
                    SearchKeyword = search ?? null,
                    PageNumber = pageIndex,
                    PageSize = pageSize
                };

                var classes = await _classClient.GetAllAsync(false, sessionID);
                var sessions = await _sessionClient.GetAllAsync();
                var companies = await _companyService.GetAllAsync();

                model = new PromoteStudentsPageViewModel
                {
                    Classes = classes.Success ? classes.Data : new List<MstClassViewModel>(),
                    Sessions = sessions.Success ? sessions.Data : new List<MstSessionViewModel>(),
                    Companies = companies.Success ? companies.Data : new List<MstCompanyViewModel>(),
                    SelectedClassId = classId,
                    SelectedSectionId = sectionId,
                    CompanyId = companyId,
                    SessionId = sessionID

                };
                if (classId.HasValue && sectionId.HasValue)
                {
                    var students = await _academicsClient.GetForPromotionPageIndexAsync(reqModel);
                    if (students.Success) model.Students = students.Data.Data;
                    model.PageNumber = students.Data.PageNumber;
                    model.PageSize = students.Data.PageSize;
                    model.SearchTerm = search;

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

        [HttpPost]
        public async Task<IActionResult> SavePromotion([FromBody] PromotionRequest request)
        {
            var response = await _academicsClient.PromoteStudentsAsync(request);
            return Json(new { success = response.Success, message = response.Message });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleSubjectGroupStatus([FromBody] StatusUpdateRequest request)
        {
            try
            {
                var response = await _subjectGroupClient.ToggleStatusAsync(request);
                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }


        public async Task<IActionResult> AddClass(int? id)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Academics/Class"
               );
                var sessionId = await GetSessionId();
                var companyId = await GetCompanyId();
                var sectionsResponse = await _sectionClient.GetAllAsync(false, sessionId, companyId);
                var model = new MstClassAddViewModel
                {
                    AvailableSections = sectionsResponse.Success ? sectionsResponse.Data : new List<MstSectionViewModel>()
                };


                if (id.HasValue && id.Value > 0)
                {
                    var response = await _classClient.GetByIDAsync(id.Value);
                    if (response.Success)
                    {
                        model.Classes = response.Data;
                        model.EditClasses = response.Data;
                    }
                }
                else
                {
                    model.EditClasses = null;
                }

                model.Permissions = perms;
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IActionResult> AddSubject(int? id)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Academics/Class"
               );
                var sessionId = await GetSessionId();
                var sectionsResponse = await _sectionClient.GetAllAsync(false, sessionId);
                var model = new MstSubjectAddViewModel();
                if (id.HasValue && id.Value > 0)
                {
                    var response = await _subjectClient.GetByIDAsync(id.Value);
                    if (response.Success)
                    {
                        model.Subjects = response.Data;
                        model.EditSubjects = response.Data;
                    }
                }
                else
                {
                    model.EditSubjects = null;
                }

                model.Permissions = perms;
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<IActionResult> AddSubjectGroup(int? id)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Academics/SubjectGroup"
               );
                var sessionId = await GetSessionId();

                var request = new DropdowRequest
                {
                    CompanyID = await GetCompanyId(),
                    SessionID = sessionId
                };


                var classesResponse = await _classClient.GetAllAsync(false, sessionId);
                var sectionsResponse = await _sectionClient.GetAllAsync(false, sessionId);
                var subjectsResponse = await _subjectClient.SubjectsDropdowBindAsync(request);
                var model = new MstSubjectGroupAddViewModel
                {                    
                    Classes = classesResponse.Success ? classesResponse.Data : new List<MstClassViewModel>(),
                    Sections = sectionsResponse.Success ? sectionsResponse.Data : new List<MstSectionViewModel>(),
                    Subjects = subjectsResponse.Success ? subjectsResponse.Data : new List<Dropdowbinding>()
                };

                if (id.HasValue && id.Value > 0)
                {
                    var response = await _subjectGroupClient.GetByIDAsync(id.Value);
                    if (response.Success)
                    {
                        model.SubjectGroups = response.Data;
                        model.EditSubjectGroups = response.Data;
                    }
                }
                else
                {
                    model.EditSubjectGroups = null;
                }
                model.Permissions = perms;
                return View(model);
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        public async Task<IActionResult> AddAssignClassTeacher(int? id, int? sectionId, string staffIds = null)
        {
            try
            {
                // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
                var perms = await GetPermissions(
                   "/Academics/Class"
               );
                var sessionId = await GetSessionId();
                var companyId = await GetCompanyId();
                var staffId = GetStaffID();
                if (staffId == null || staffId == 0)
                {
                    staffId = GetStaffID();
                    staffId = (staffId ?? 0) <= 0 ? null : staffId;
                }
                var sectionsResponse = await _sectionClient.GetAllAsync(false, sessionId);
                var classes = await _classClient.GetAllAsync(false, sessionId);
                var staff = await _hrClient.GetAllStaffAsync(companyId,sessionId, staffId);
                var model = new AssignClassTeacherAddViewModel
                {
                    Classes = classes.Success ? classes.Data : new List<MstClassViewModel>(),
                    Staff = staff.Success ? staff.Data : new List<HRStaffViewModel>()
                };


                if (id.HasValue && id.Value > 0)
                {
                    var response = await _sectionClient.GetByClassAsync(id.Value);
                    if (response.Success)
                    {
                        //model.Assignments = response.Data;
                        model.EditAssignments = true;
                    }
                }
                else
                {
                    model.EditAssignments = false;
                }

                model.Permissions = perms;
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
