using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class StudentInformationClientService : BaseApiClient, IStudentInformationClientService
    {
        public StudentInformationClientService(HttpClient httpClient) : base(httpClient) { }

        public Task<ApiResponse<PagedResult<StudentListViewModel>>> GetStudentListAsync(int? sessionId,int? classId = null, int? sectionId = null, string? searchTerm = null, int? PageNumber=null, int? PageSize=null)
        {
            var url = $"api/StudentInformationApi/GetStudentList?sessionId={sessionId}&classId={classId}&sectionId={sectionId}&searchTerm={searchTerm}&PageNumber={PageNumber}&PageSize={PageSize}";
            return GetAsync<PagedResult<StudentListViewModel>>(url);
        }

        public Task<ApiResponse<StudentDetailsViewModel>> GetStudentByIDAsync(int id)
        {
            return GetAsync<StudentDetailsViewModel>($"api/StudentInformationApi/GetByID/{id}");
        }

        public Task<ApiResponse<List<StudentDisableReasonViewModel>>> GetAllDisableReasons(int sessionID)
        {
            return GetAsync<List<StudentDisableReasonViewModel>>($"api/StudentInformationApi/GetAllDisableReasons?sessionID={sessionID}");
        }

        public Task<ApiResponse<List<StudentHouseViewModel>>> GetAllStudentHouses(int sessionID)
        {
            return GetAsync<List<StudentHouseViewModel>>($"api/StudentInformationApi/GetAllStudentHouses?sessionID={sessionID}");
        }

        public Task<ApiResponse<List<StudentCategoryViewModel>>> GetAllStudentCategories(int sessionId)
        {
            return GetAsync<List<StudentCategoryViewModel>>($"api/StudentInformationApi/GetAllStudentCategories?sessionId={sessionId}");
        }

        /// <summary>
        /// Generates the next student roll number.
        /// </summary>
        public Task<ApiResponse<string>> GetNewStudentRollNo(StudentRollNoRequest request)
        {
            return PostAsync<string>(
                "api/StudentInformationApi/GetNewStudentRollNo",
                request);
        }

        /// <summary>
        /// Retrieves the next available admission number.
        /// </summary>
        /// <returns>Admission number response.</returns>
        public Task<ApiResponse<string>> GetNextAdmissionNo()
        {
            return GetAsync<string>(
                "api/StudentInformationApi/GetNextAdmissionNo");
        }

        /// <summary>
        /// Creates or updates a student admission.
        /// </summary>
        public Task<ApiResponse<int>> UpsertStudentAdmission(
            StudentAdmissionUpsertRequest request)
        {
            return PostAsync<int>(
                "api/StudentInformationApi/UpsertStudentAdmission",
                request);
        }

        /// <summary>
        /// Deletes multiple students.
        /// </summary>
        /// <param name="studentIds">List of Student IDs.</param>
        /// <returns>Operation status and message.</returns>
        public Task<ApiResponse<object>> BulkDeleteStudents(List<int> studentIds)
        {
            return PostAsync<object>(
                "api/StudentInformationApi/BulkDeleteStudents",
                studentIds);
        }

        /// <summary>
        /// Retrieves the list of disabled students.
        /// </summary>
        public Task<ApiResponse<List<StudentListViewModel>>> GetDisabledStudentList(
            int? classId = null,
            int? sectionId = null,
            string? searchTerm = null, int? sessionId=null)
        {
            return GetAsync<List<StudentListViewModel>>(
                $"api/StudentInformationApi/GetDisabledStudentList" +
                $"?classId={classId}" +
                $"&sectionId={sectionId}" +
                $"&searchTerm={Uri.EscapeDataString(searchTerm ?? string.Empty)}" +
                $"&sessionId={sessionId}");
        }

        /// <summary>
        /// Creates or updates a student disable reason.
        /// </summary>
        /// <param name="request">Disable reason information.</param>
        /// <returns>Operation result.</returns>
        public Task<ApiResponse<object>> UpsertDisableReason(
            StudentDisableReasonUpsertRequest request)
        {
            return PostAsync<object>(
                "api/StudentInformationApi/UpsertDisableReason",
                request);
        }

        /// <summary>
        /// Deletes a student disable reason.
        /// </summary>
        /// <param name="id">Disable Reason ID.</param>
        /// <returns>Operation result.</returns>
        public Task<ApiResponse<object>> DeleteDisableReason([FromBody] List<int> ids)
        {
            return PostAsync<object>(
                $"api/StudentInformationApi/DeleteDisableReason", ids);
        }

        /// <summary>
        /// Creates or updates a student house.
        /// </summary>
        /// <param name="request">Student house information.</param>
        /// <returns>Operation result.</returns>
        public Task<ApiResponse<object>> UpsertStudentHouse(
            StudentHouseUpsertRequest request)
        {
            return PostAsync<object>(
                "api/StudentInformationApi/UpsertStudentHouse",
                request);
        }

        /// <summary>
        /// Deletes a student house.
        /// </summary>
        /// <param name="id">Student House ID.</param>
        /// <returns>Operation result.</returns>
        public Task<ApiResponse<object>> DeleteStudentHouse(List<int> ids)
        {
            return PostAsync<object>(
                $"api/StudentInformationApi/DeleteStudentHouse", ids);
        }

        /// <summary>
        /// Creates or updates a student category.
        /// </summary>
        /// <param name="request">Student category information.</param>
        /// <returns>Operation result.</returns>
        public Task<ApiResponse<object>> UpsertStudentCategory(
            StudentCategoryUpsertRequest request)
        {
            return PostAsync<object>(
                "api/StudentInformationApi/UpsertStudentCategory",
                request);
        }

        /// <summary>
        /// Deletes a student category.
        /// </summary>
        /// <param name="id">Student Category ID.</param>
        /// <returns>Operation result.</returns>
        public Task<ApiResponse<object>> DeleteStudentCategory(List<int> ids)
        {
            return PostAsync<object>(
                $"api/StudentInformationApi/DeleteStudentCategory", ids);                
        }

        /// <summary>
        /// Retrieves timeline history for a student.
        /// </summary>
        /// <param name="studentId">Student ID.</param>
        /// <returns>Student timeline data.</returns>
        public Task<ApiResponse<List<StudentTimelineViewModel>>> GetStudentTimeline(int studentId)
        {
            return GetAsync<List<StudentTimelineViewModel>>(
                $"api/StudentInformationApi/GetStudentTimeline/{studentId}");
        }

        /// <summary>
        /// Creates or updates a student timeline entry.
        /// </summary>
        /// <param name="request">Timeline information.</param>
        /// <returns>Operation result.</returns>
        public Task<ApiResponse<object>> UpsertTimeline(
            StudentTimelineUpsertRequest request)
        {
            return PostAsync<object>(
                "api/StudentInformationApi/UpsertTimeline",
                request);
        }

        /// <summary>
        /// Deletes a student timeline entry.
        /// </summary>
        /// <param name="id">Timeline ID.</param>
        /// <returns>Operation result.</returns>
        public Task<ApiResponse<object>> DeleteTimeline(int id)
        {
            return PostAsync<object>(
                $"api/StudentInformationApi/DeleteTimeline/{id}",
                new { });
        }

        /// <summary>
        /// Downloads a student timeline document.
        /// </summary>
        /// <param name="id">Timeline ID.</param>
        /// <returns>Document bytes.</returns>
        public Task<ApiResponse<FileViewModel>> DownloadTimelineDoc(int id)
        {
            return GetAsync<FileViewModel>(
                $"api/StudentInformationApi/DownloadTimelineDoc/{id}");
        }

        /// <summary>
        /// Changes the status of a student.
        /// </summary>
        /// <param name="request">Student status information.</param>
        /// <returns>Operation result.</returns>
        public Task<ApiResponse<object>> ToggleStatus(
            StudentStatusToggleRequest request)
        {
            return PostAsync<object>(
                "api/StudentInformationApi/ToggleStatus",
                request);
        }

        /// <summary>
        /// Retrieves students available for multi-class assignment.
        /// </summary>
        /// <param name="classId">Class ID.</param>
        /// <param name="sectionId">Section ID.</param>
        /// <param name="searchTerm">Search term.</param>
        /// <returns>List of students.</returns>
        public Task<ApiResponse<List<StudentListViewModel>>> GetMultiClassStudents(
            int? classId,
            int? sectionId,
            string? searchTerm)
        {
            return GetAsync<List<StudentListViewModel>>(
                $"api/StudentInformationApi/GetMultiClassStudents" +
                $"?classId={classId}" +
                $"&sectionId={sectionId}" +
                $"&searchTerm={Uri.EscapeDataString(searchTerm ?? string.Empty)}");
        }

        /// <summary>
        /// Creates or updates a student's multi-class assignment.
        /// </summary>
        /// <param name="request">Multi-class assignment information.</param>
        /// <returns>Operation result.</returns>
        public Task<ApiResponse<object>> UpsertMultiClass(
            StudentMultiClassUpsertRequest request)
        {
            return PostAsync<object>(
                "api/StudentInformationApi/UpsertMultiClass",
                request);
        }
        /// <summary>
        /// Deletes a student multi-class assignment.
        /// </summary>
        /// <param name="id">Multi-Class ID.</param>
        /// <returns>Operation result.</returns>
        public Task<ApiResponse<object>> DeleteMultiClass(int id)
        {
            return PostAsync<object>(
                $"api/StudentInformationApi/DeleteMultiClass/{id}",
                new { });
        }

        public Task<ApiResponse<StudentCategoryViewModel>> GetStudentCategoryByIdAsync(int id)
        {
            return GetAsync<StudentCategoryViewModel>($"api/StudentInformationApi/GetStudentCategoryById/{id}");
        }

        public Task<ApiResponse<StudentHouseViewModel>> GetStudentHouseByIdAsync(int id)
        {
            return GetAsync<StudentHouseViewModel>($"api/StudentInformationApi/GetStudentHouseById/{id}");
        }

        public Task<ApiResponse<StudentDisableReasonViewModel>> GetDisableReasonsByID(int id)
        {
            return GetAsync<StudentDisableReasonViewModel>($"api/StudentInformationApi/GetDisableReasonsByID/{id}");
        }

        public Task<ApiResponse<SpResult>> UpdateStudentProfileAsync(ProfileRequest req)
           => PostAsync<SpResult>("api/StudentInformationApi/UpdateStudentProfile", req);


        
        public async Task<ApiResponse<PagedResult<StudentHouseViewModel>>> GetStudentHouseListAsync(SubjectSearchRequest request)
        {
            return await PostAsync<PagedResult<StudentHouseViewModel>>("api/StudentInformationApi/GetStudentHouseList", request);
        }
    }
}
