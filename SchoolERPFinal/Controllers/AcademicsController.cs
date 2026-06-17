using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Models;
using SchoolERP.Net.Services;
using SchoolERP.Net.Services.Clients;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchoolERP.Net.Controllers
{
    /// <summary>
    /// This controller manages the school's academic structure, including Classes, Subjects, and how they are grouped together.
    /// </summary>
    public class AcademicsController : Controller
    {
        private readonly IClassClientService _classClient;
        private readonly ISectionClientService _sectionClient;
        private readonly ISubjectClientService _subjectClient;
        private readonly ISubjectGroupClientService _subjectGroupClient;
        private readonly IAcademicsClientService _academicsClient;
        private readonly IHumanResourceClientService _hrClient;
        private readonly ISessionClientService _sessionClient;
        private readonly IUserMenuPermissionClientService _menuPerm;
        
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
            IUserMenuPermissionClientService menuPerm)
        {
            _classClient = classClient;
            _sectionClient = sectionClient;
            _subjectClient = subjectClient;
            _subjectGroupClient = subjectGroupClient;
            _academicsClient = academicsClient;
            _hrClient = hrClient;
            _sessionClient = sessionClient;
            _menuPerm = menuPerm;
        }

        /// <summary>
        /// Shows the 'Class' management page where you can define the different grades or classes in the school.
        /// </summary>
        public async Task<IActionResult> Class()
        {
            var classesResponse = await _classClient.GetAllAsync();
            var sectionsResponse = await _sectionClient.GetAllAsync();
            var model = new MstClassPageViewModel
            {
                Classes = classesResponse.Success ? classesResponse.Data : new List<MstClassViewModel>(),
                AvailableSections = sectionsResponse.Success ? sectionsResponse.Data : new List<MstSectionViewModel>()
            };
            return View(model);
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
            var isCreate = request.ClassID <= 0;
            if (isCreate && !(await _menuPerm.Has( ClassMenuPath, "Add")).Data)
                return Json(new { success = false, message = "You do not have permission to add classes." });
            if (!isCreate && !(await _menuPerm.Has(ClassMenuPath, "Edit")).Data)
                return Json(new { success = false, message = "You do not have permission to edit classes." });
            var response = await _classClient.UpsertAsync(request);
            return Json(new { success = response.Success, message = response.Message });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleClassStatus(int id, bool isActive)
        {
            var response = await _classClient.ToggleStatusAsync(id, isActive);
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
        public async Task<IActionResult> Subject()
        {
            var response = await _subjectClient.GetAllAsync();
            var model = new MstSubjectPageViewModel
            {
                Subjects = response.Success ? response.Data : new List<MstSubjectViewModel>()
            };
            return View(model);
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
        public async Task<IActionResult> ToggleSubjectStatus(int id, bool isActive)
        {
            var response = await _subjectClient.ToggleStatusAsync(id, isActive);
            return Json(new { success = response.Success, message = response.Message });
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
            var response = await _sectionClient.GetByClassAsync(id);
            if (!response.Success) return Json(new { success = false, message = response.Message });
            return Json(new { success = true, data = response.Data });
        }

        [HttpGet]
        public async Task<IActionResult> GetSubjectGroups(int classId, int sectionId)
        {
            var response = await _subjectGroupClient.GetAllAsync();
            if (!response.Success) return Json(new { success = false, message = response.Message });

            var groups = response.Data.Where(g => g.ClassID == classId && g.SectionIds.Contains(sectionId)).ToList();
            return Json(new { success = true, data = groups });
        }

        /// <summary>
        /// Shows the 'Subject Group' page where you can group subjects together for specific classes.
        /// </summary>
        public async Task<IActionResult> SubjectGroup()
        {
            var groupsResponse = await _subjectGroupClient.GetAllAsync();
            var classesResponse = await _classClient.GetAllAsync();
            var sectionsResponse = await _sectionClient.GetAllAsync();
            var subjectsResponse = await _subjectClient.GetAllAsync();
            var model = new MstSubjectGroupPageViewModel
            {
                SubjectGroups = groupsResponse.Success ? groupsResponse.Data : new List<MstSubjectGroupViewModel>(),
                Classes = classesResponse.Success ? classesResponse.Data : new List<MstClassViewModel>(),
                Sections = sectionsResponse.Success ? sectionsResponse.Data : new List<MstSectionViewModel>(),
                Subjects = subjectsResponse.Success ? subjectsResponse.Data : new List<MstSubjectViewModel>()
            };
            return View(model);
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
            var classes = await _classClient.GetAllAsync();
            var subjectsResponse = await _subjectClient.GetAllAsync();
            var staff = await _hrClient.GetAllStaffAsync();
            var subjectGroups = await _subjectGroupClient.GetAllAsync();

            var model = new AddTimeTablePageViewModel
            {
                Classes = classes.Success ? classes.Data : new List<MstClassViewModel>(),
                Subjects = subjectsResponse.Success ? subjectsResponse.Data : new List<MstSubjectViewModel>(),
                Staff = staff.Success ? staff.Data : new List<HRStaffViewModel>(),
                SubjectGroups = subjectGroups.Success ? subjectGroups.Data : new List<MstSubjectGroupViewModel>()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddTimeTable(int classId, int sectionId, int subjectGroupId = 0)
        {
            var classes = await _classClient.GetAllAsync();
            var subjectsResponse = await _subjectClient.GetAllAsync();
            var staff = await _hrClient.GetAllStaffAsync();
            var subjectGroups = await _subjectGroupClient.GetAllAsync();

            var allSubjects = subjectsResponse.Success ? subjectsResponse.Data : new List<MstSubjectViewModel>();
            var allSubjectGroups = subjectGroups.Success ? subjectGroups.Data : new List<MstSubjectGroupViewModel>();

            if (subjectGroupId > 0)
            {
                var groupResponse = await _subjectGroupClient.GetByIDAsync(subjectGroupId);
                if (groupResponse.Success && groupResponse.Data != null && groupResponse.Data.SubjectIds != null && groupResponse.Data.SubjectIds.Any())
                {
                    allSubjects = allSubjects.Where(s => groupResponse.Data.SubjectIds.Contains(s.SubjectID)).ToList();
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

        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> ClassTimeTable(int classId = 0, int sectionId = 0)
        {
            var classes = await _classClient.GetAllAsync();
            var model = new AddTimeTablePageViewModel
            {
                Classes = classes.Success ? classes.Data : new List<MstClassViewModel>(),
                SelectedClassId = classId,
                SelectedSectionId = sectionId
            };
            if (classId > 0 && sectionId > 0)
            {
                var slots = await _academicsClient.GetTimeTableByClassAsync(classId, sectionId);
                model.TimeTableSlots = slots.Success ? slots.Data : new List<TimeTableViewModel>();
            }
            return View(model);
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
            var staff = await _hrClient.GetAllStaffAsync();

            var model = new TeacherTimeTablePageViewModel
            {
                Staff = staff.Success ? staff.Data : new List<HRStaffViewModel>(),
                SelectedStaffId = 0
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> TeacherTimeTable(int staffId)
        {
            var staff = await _hrClient.GetAllStaffAsync();

            var model = new TeacherTimeTablePageViewModel
            {
                Staff = staff.Success ? staff.Data : new List<HRStaffViewModel>(),
                SelectedStaffId = staffId
            };

            if (staffId > 0)
            {
                var slots = await _academicsClient.GetTimeTableByStaffAsync(staffId);
                model.TimeTableSlots = slots.Success ? slots.Data : new List<TimeTableViewModel>();
            }

            return View(model);
        }

        public async Task<IActionResult> AssignClassTeacher()
        {
            var classes = await _classClient.GetAllAsync();
            var staff = await _hrClient.GetAllStaffAsync();
            var assignments = await _academicsClient.GetAllClassTeachersAsync();

            var model = new AssignClassTeacherPageViewModel
            {
                Classes = classes.Success ? classes.Data : new List<MstClassViewModel>(),
                Staff = staff.Success ? staff.Data : new List<HRStaffViewModel>(),
                Assignments = assignments.Success ? assignments.Data : new List<ClassTeacherViewModel>()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveClassTeacher([FromBody] ClassTeacherUpsertRequest request)
        {
            var response = await _academicsClient.UpsertClassTeacherAsync(request);
            return Json(new { success = response.Success, message = response.Message });
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
        public async Task<IActionResult> PromoteStudents(int? classId, int? sectionId, int? nextSessionId, int? nextClassId, int? nextSectionId)
        {
            var classes = await _classClient.GetAllAsync();
            var sessions = await _sessionClient.GetAllAsync();
            
            var model = new PromoteStudentsPageViewModel
            {
                Classes = classes.Success ? classes.Data : new List<MstClassViewModel>(),
                Sessions = sessions.Success ? sessions.Data : new List<MstSessionViewModel>(),
                SelectedClassId = classId,
                SelectedSectionId = sectionId,
                NextSessionId = nextSessionId,
                NextClassId = nextClassId,
                NextSectionId = nextSectionId
            };

            if (classId.HasValue && sectionId.HasValue)
            {
                var students = await _academicsClient.GetStudentsForPromotionAsync(classId.Value, sectionId.Value);
                if (students.Success) model.Students = students.Data;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SavePromotion([FromBody] PromotionRequest request)
        {
            var response = await _academicsClient.PromoteStudentsAsync(request);
            return Json(new { success = response.Success, message = response.Message });
        }
    }
}
