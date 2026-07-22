using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface IHumanResourceClientService
    {
        Task<ApiResponse<List<HRDesignationViewModel>>> GetAllDesignationsAsync();
        Task<ApiResponse<HRDesignationViewModel>> GetDesignationByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertDesignationAsync(HRDesignationUpsertRequest req);
        Task<ApiResponse<dynamic>> DeleteDesignationAsync(List<int> id);
        Task<ApiResponse<dynamic>> ToggleDesignationStatusAsync(int id, bool isActive);

        Task<ApiResponse<DepartmentListResponse>> GetAllDepartmentsAsync();
        Task<ApiResponse<HRDepartmentViewModel>> GetDepartmentByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertDepartmentAsync(HRDepartmentUpsertRequest req);
        Task<ApiResponse<dynamic>> DeleteDepartmentAsync(List<int> id);
        Task<ApiResponse<dynamic>> ToggleDepartmentStatusAsync(int id, bool isActive);

        Task<ApiResponse<List<HRLeaveTypeViewModel>>> GetAllLeaveTypesAsync();
        Task<ApiResponse<HRLeaveTypeViewModel>> GetLeaveTypeByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertLeaveTypeAsync(HRLeaveTypeUpsertRequest req);
        Task<ApiResponse<dynamic>> DeleteLeaveTypeAsync(List<int> id);
        Task<ApiResponse<dynamic>> ToggleLeaveTypeStatusAsync(int id, bool isActive);

        Task<ApiResponse<List<HRStaffViewModel>>> GetAllStaffAsync(int companyId, int sessionId, int? staffId=null);
        Task<ApiResponse<HRStaffViewModel>> GetStaffByIDAsync(int id);
        Task<ApiResponse<SpResult>> UpsertStaffAsync(HRStaffUpsertRequest req);
        Task<ApiResponse<dynamic>> DeleteStaffAsync(List<int> id);
        Task<ApiResponse<string>> GetNewStaffCodeAsync();
        
        Task<ApiResponse<List<HRStaffAttendanceViewModel>>> GetStaffAttendanceAsync(DateTime date, int? roleId);
        Task<ApiResponse<dynamic>> SaveStaffAttendanceAsync(List<HRStaffAttendanceUpsertRequest> reqs);

        // --- Apply Leave ---
        Task<ApiResponse<List<HRApplyLeaveViewModel>>> GetAllApplyLeaveAsync();
        Task<ApiResponse<HRApplyLeaveViewModel>> GetApplyLeaveByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertApplyLeaveAsync(HRApplyLeaveUpsertRequest req);
        Task<ApiResponse<dynamic>> DeleteApplyLeaveAsync(int id);
        
        // --- Payroll ---
        Task<ApiResponse<List<HRPayrollViewModel>>> GetAllPayrollAsync(int month, int year, int? roleId);
        Task<ApiResponse<dynamic>> GeneratePayrollAsync(HRPayrollGenerateRequest req);

        Task<ApiResponse<HttpResponseMessage>> GetStaffDocumentAsync(
            int staffId,
            string docType);

        Task<ApiResponse<HttpResponseMessage>> GetApplyLeaveDocumentAsync(int id);
        Task<ApiResponse<HRPayrollGenerationViewModel>> GetPayrollGenerationDataAsync(
            int staffId,
            int month,
            int year);

        Task<ApiResponse<dynamic>> UpdateProfileAsync(ProfileRequest req);


        Task<ApiResponse<PagedResult<HRDepartmentViewModel>>> GetAllDepartmentsWithPageAsync(SearchRequest request);
        Task<ApiResponse<PagedResult<HRDesignationViewModel>>> GetAllDesignationsWithPageAsync(SearchRequest request);
        Task<ApiResponse<PagedResult<HRLeaveTypeViewModel>>> GetAllLeaveTypesWithPageAsync(SearchRequest request);
    }
}
