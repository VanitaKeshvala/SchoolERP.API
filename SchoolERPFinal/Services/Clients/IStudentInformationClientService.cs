using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface IStudentInformationClientService
    {
        Task<ApiResponse<List<StudentListViewModel>>> GetStudentListAsync(int? classId = null, int? sectionId = null, string? searchTerm = null);
        Task<ApiResponse<StudentDetailsViewModel>> GetStudentByIDAsync(int id);
        Task<ApiResponse<List<StudentDisableReasonViewModel>>> GetAllDisableReasons();
        Task<ApiResponse<List<StudentHouseViewModel>>> GetAllStudentHouses();
        Task<ApiResponse<List<StudentCategoryViewModel>>> GetAllStudentCategories();
        Task<ApiResponse<string>> GetNewStudentRollNo(Dictionary<string, string>? dynamicValues = null);
        Task<ApiResponse<string>> GetNextAdmissionNo();
        Task<ApiResponse<int>> UpsertStudentAdmission(StudentAdmissionUpsertRequest request);
        Task<ApiResponse<object>> BulkDeleteStudents(List<int> studentIds);
        Task<ApiResponse<List<StudentListViewModel>>> GetDisabledStudentList(
            int? classId = null,
            int? sectionId = null,
            string? searchTerm = null);
        Task<ApiResponse<object>> UpsertDisableReason(
            StudentDisableReasonUpsertRequest request);
        Task<ApiResponse<object>> DeleteDisableReason([FromBody] List<int> ids);
        Task<ApiResponse<object>> UpsertStudentHouse(
            StudentHouseUpsertRequest request);
        Task<ApiResponse<object>> DeleteStudentHouse(List<int> ids);
        Task<ApiResponse<object>> UpsertStudentCategory(
            StudentCategoryUpsertRequest request);
        Task<ApiResponse<object>> DeleteStudentCategory(List<int> ids);
        Task<ApiResponse<List<StudentTimelineViewModel>>> GetStudentTimeline(int studentId);
        Task<ApiResponse<object>> UpsertTimeline(
            StudentTimelineUpsertRequest request);
        Task<ApiResponse<object>> DeleteTimeline(int id);
        Task<ApiResponse<FileViewModel>> DownloadTimelineDoc(int id);
        Task<ApiResponse<object>> ToggleStatus(
            StudentStatusToggleRequest request);
        Task<ApiResponse<List<StudentListViewModel>>> GetMultiClassStudents(
            int? classId,
            int? sectionId,
            string? searchTerm);
        Task<ApiResponse<object>> UpsertMultiClass(
            StudentMultiClassUpsertRequest request);
        Task<ApiResponse<object>> DeleteMultiClass(int id);
    }
}
