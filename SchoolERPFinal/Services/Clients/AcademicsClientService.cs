using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class AcademicsClientService : BaseApiClient, IAcademicsClientService
    {
        public AcademicsClientService(HttpClient client) : base(client) { }

        public Task<ApiResponse<List<TimeTableViewModel>>> GetTimeTableByClassAsync(int classId, int sectionId)
            => GetAsync<List<TimeTableViewModel>>($"api/AcademicsApi/GetTimeTableByClass?classId={classId}&sectionId={sectionId}");

        public Task<ApiResponse<List<TimeTableViewModel>>> GetTimeTableByStaffAsync(int staffId)
            => GetAsync<List<TimeTableViewModel>>($"api/AcademicsApi/GetTimeTableByStaff/{staffId}");

        public Task<ApiResponse<dynamic>> UpsertTimeTableAsync(TimeTableUpsertRequest req)
            => PostAsync<dynamic>("api/AcademicsApi/UpsertTimeTable", req);

        public Task<ApiResponse<dynamic>> DeleteTimeTableSlotAsync(int id)
            => PostAsync<dynamic>($"api/AcademicsApi/DeleteTimeTableSlot/{id}", null!);

        //public Task<ApiResponse<List<ClassTeacherViewModel>>> GetAllClassTeachersAsync()
        //    => GetAsync<List<ClassTeacherViewModel>>("api/AcademicsApi/GetAllClassTeachers");

        public Task<ApiResponse<List<ClassTeacherViewModel>>> GetAllClassTeachersAsync(
            List<int>? classIds = null,
            List<int>? sectionIds = null, int? sessionID=null)
        {
            var query = new List<string>();

            if (classIds?.Any() == true)
                query.AddRange(classIds.Select(x => $"classId={x}"));

            if (sectionIds?.Any() == true)
                query.AddRange(sectionIds.Select(x => $"sectionId={x}"));
            
            if (sessionID.HasValue)
                query.Add($"sessionID={sessionID.Value}");

            var url = "api/AcademicsApi/GetAllClassTeachers";

            if (query.Any())
                url += "?" + string.Join("&", query);

            return GetAsync<List<ClassTeacherViewModel>>(url);
        }

        public Task<ApiResponse<dynamic>> UpsertClassTeacherAsync(ClassTeacherUpsertRequest req)
            => PostAsync<dynamic>("api/AcademicsApi/UpsertClassTeacher", req);

        public Task<ApiResponse<dynamic>> DeleteClassTeacherAsync(List<int> ids)
            => PostAsync<dynamic>($"api/AcademicsApi/DeleteClassTeacher", ids!);

        public Task<ApiResponse<List<StudentPromotionViewModel>>> GetStudentsForPromotionAsync(int classId, int sectionId, int companyId, int sessionId)
            => GetAsync<List<StudentPromotionViewModel>>($"api/AcademicsApi/GetStudentsForPromotion?classId={classId}&sectionId={sectionId}&companyId={companyId}&sessionId={sessionId}");

        public Task<ApiResponse<dynamic>> PromoteStudentsAsync(PromotionRequest req)
            => PostAsync<dynamic>("api/AcademicsApi/PromoteStudents", req);


        public async Task<ApiResponse<PagedResult<ClassTeacherViewModel>>> GetAllClassWithPageAsync(AcademicsSearchRequest request)
        {
            return await PostAsync<PagedResult<ClassTeacherViewModel>>("api/AcademicsApi/GetAllClassTeachersWithPage", request);
        }

        public Task<ApiResponse<PagedResult<FOAdmissionInquiryViewModel>>> GetAllAdmissionInquiriesWithPageIndexAsync(EnquirySearchRequest req)
            => PostAsync<PagedResult<FOAdmissionInquiryViewModel>>("api/FrontOfficeApi/GetAllAdmissionInquiriesWithPageIndex", req);

        public Task<ApiResponse<PagedResult<StudentPromotionViewModel>>> GetForPromotionPageIndexAsync(SearchPromotedStudent req)
           => PostAsync<PagedResult<StudentPromotionViewModel>>("api/AcademicsApi/GetForPromotionPageIndex", req);
    }
}
