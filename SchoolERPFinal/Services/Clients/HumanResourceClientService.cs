using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class HumanResourceClientService : BaseApiClient, IHumanResourceClientService
    {
        public HumanResourceClientService(HttpClient httpClient) : base(httpClient) { }

        public Task<ApiResponse<List<HRDesignationViewModel>>> GetAllDesignationsAsync()
            => GetAsync<List<HRDesignationViewModel>>("api/HumanResourceApi/GetAllDesignations");

        public Task<ApiResponse<HRDesignationViewModel>> GetDesignationByIDAsync(int id)
            => GetAsync<HRDesignationViewModel>($"api/HumanResourceApi/GetDesignationByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertDesignationAsync(HRDesignationUpsertRequest req)
            => PostAsync<dynamic>("api/HumanResourceApi/UpsertDesignation", req);

        public Task<ApiResponse<dynamic>> DeleteDesignationAsync(List<int> id)
            => PostAsync<dynamic>($"api/HumanResourceApi/DeleteDesignation", id);

        public Task<ApiResponse<dynamic>> ToggleDesignationStatusAsync(int id, bool isActive)
            => PostAsync<dynamic>($"api/HumanResourceApi/ToggleDesignationStatus?id={id}&isActive={isActive}", null!);

        public Task<ApiResponse<DepartmentListResponse>> GetAllDepartmentsAsync()
            => GetAsync<DepartmentListResponse>("api/HumanResourceApi/GetAllDepartments");

        public Task<ApiResponse<HRDepartmentViewModel>> GetDepartmentByIDAsync(int id)
            => GetAsync<HRDepartmentViewModel>($"api/HumanResourceApi/GetDepartmentByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertDepartmentAsync(HRDepartmentUpsertRequest req)
            => PostAsync<dynamic>("api/HumanResourceApi/UpsertDepartment", req);

        public Task<ApiResponse<dynamic>> DeleteDepartmentAsync(List<int> id)
            => PostAsync<dynamic>($"api/HumanResourceApi/DeleteDepartment",id);

        public Task<ApiResponse<dynamic>> ToggleDepartmentStatusAsync(int id, bool isActive)
            => PostAsync<dynamic>($"api/HumanResourceApi/ToggleDepartmentStatus?id={id}&isActive={isActive}", null!);

        public Task<ApiResponse<List<HRLeaveTypeViewModel>>> GetAllLeaveTypesAsync()
            => GetAsync<List<HRLeaveTypeViewModel>>("api/HumanResourceApi/GetAllLeaveTypes");

        public Task<ApiResponse<HRLeaveTypeViewModel>> GetLeaveTypeByIDAsync(int id)
            => GetAsync<HRLeaveTypeViewModel>($"api/HumanResourceApi/GetLeaveTypeByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertLeaveTypeAsync(HRLeaveTypeUpsertRequest req)
            => PostAsync<dynamic>("api/HumanResourceApi/UpsertLeaveType", req);

        public Task<ApiResponse<dynamic>> DeleteLeaveTypeAsync(List<int> id)
            => PostAsync<dynamic>($"api/HumanResourceApi/DeleteLeaveType", id);

        public Task<ApiResponse<dynamic>> ToggleLeaveTypeStatusAsync(int id, bool isActive)
            => PostAsync<dynamic>($"api/HumanResourceApi/ToggleLeaveTypeStatus?id={id}&isActive={isActive}", null!);

        public Task<ApiResponse<List<HRStaffViewModel>>> GetAllStaffAsync(int companyId, int sessionId, int? staffId=null)
            => GetAsync<List<HRStaffViewModel>>($"api/HumanResourceApi/GetAllStaff?companyId={companyId}&sessionId={sessionId}&staffId={staffId}");

        public Task<ApiResponse<HRStaffViewModel>> GetStaffByIDAsync(int id)
            => GetAsync<HRStaffViewModel>($"api/HumanResourceApi/GetStaffByID/{id}");

        public Task<ApiResponse<SpResult>> UpsertStaffAsync(HRStaffUpsertRequest req)
            => PostAsync<SpResult>("api/HumanResourceApi/UpsertStaff", req);

        public Task<ApiResponse<dynamic>> DeleteStaffAsync(List<int> id)
            => PostAsync<dynamic>($"api/HumanResourceApi/DeleteStaff", id);

        public Task<ApiResponse<string>> GetNewStaffCodeAsync()
            => GetAsync<string>("api/HumanResourceApi/GetNewStaffCode");

        public Task<ApiResponse<List<HRStaffAttendanceViewModel>>> GetStaffAttendanceAsync(DateTime date, int? roleId)
            => GetAsync<List<HRStaffAttendanceViewModel>>($"api/HumanResourceApi/GetStaffAttendance?date={date:yyyy-MM-dd}&roleId={roleId}");

        public Task<ApiResponse<dynamic>> SaveStaffAttendanceAsync(List<HRStaffAttendanceUpsertRequest> reqs)
            => PostAsync<dynamic>("api/HumanResourceApi/SaveStaffAttendance", reqs);

        // --- Apply Leave ---
        public Task<ApiResponse<List<HRApplyLeaveViewModel>>> GetAllApplyLeaveAsync()
            => GetAsync<List<HRApplyLeaveViewModel>>("api/HumanResourceApi/GetAllApplyLeave");

        public Task<ApiResponse<HRApplyLeaveViewModel>> GetApplyLeaveByIDAsync(int id)
            => GetAsync<HRApplyLeaveViewModel>($"api/HumanResourceApi/GetApplyLeaveByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertApplyLeaveAsync(HRApplyLeaveUpsertRequest req)
            => PostAsync<dynamic>("api/HumanResourceApi/UpsertApplyLeave", req);

        public Task<ApiResponse<dynamic>> DeleteApplyLeaveAsync(int id)
            => PostAsync<dynamic>($"api/HumanResourceApi/DeleteApplyLeave/{id}", null!);

        // --- Payroll ---
        public Task<ApiResponse<List<HRPayrollViewModel>>> GetAllPayrollAsync(int month, int year, int? roleId)
            => GetAsync<List<HRPayrollViewModel>>($"api/HumanResourceApi/GetAllPayroll?month={month}&year={year}&roleId={roleId}");

        public Task<ApiResponse<dynamic>> GeneratePayrollAsync(HRPayrollGenerateRequest req)
            => PostAsync<dynamic>("api/HumanResourceApi/GeneratePayroll", req);

        /// <summary>
        /// Retrieves a staff document.
        /// </summary>
        /// <param name="staffId">Staff identifier.</param>
        /// <param name="docType">Document type.</param>
        /// <returns>Document information.</returns>
        public Task<ApiResponse<HttpResponseMessage>> GetStaffDocumentAsync(
            int staffId,
            string docType)
            => GetAsync<HttpResponseMessage>(
                $"api/HumanResourceApi/GetStaffDocument?staffId={staffId}&docType={Uri.EscapeDataString(docType)}");

        /// <summary>
        /// Retrieves an applied leave attachment document.
        /// </summary>
        /// <param name="id">Apply leave identifier.</param>
        /// <returns>Document information.</returns>
        public Task<ApiResponse<HttpResponseMessage>> GetApplyLeaveDocumentAsync(int id)
            => GetAsync<HttpResponseMessage>(
                $"api/HumanResourceApi/GetApplyLeaveDocument?id={id}");

        /// <summary>
        /// Retrieves payroll generation data for a staff member.
        /// </summary>
        /// <param name="staffId">Staff identifier.</param>
        /// <param name="month">Payroll month.</param>
        /// <param name="year">Payroll year.</param>
        /// <returns>Payroll generation data.</returns>
        public Task<ApiResponse<HRPayrollGenerationViewModel>> GetPayrollGenerationDataAsync(
            int staffId,
            int month,
            int year)
            => GetAsync<HRPayrollGenerationViewModel>(
                $"api/HumanResourceApi/GetPayrollGenerationData?staffId={staffId}&month={month}&year={year}");


        public Task<ApiResponse<dynamic>> UpdateProfileAsync(ProfileRequest req)
           => PostAsync<dynamic>("api/HumanResourceApi/UpdateProfile", req);

        public async Task<ApiResponse<PagedResult<HRDepartmentViewModel>>> GetAllDepartmentsWithPageAsync(SearchRequest request)
        {
            return await PostAsync<PagedResult<HRDepartmentViewModel>>("api/HumanResourceApi/GetAllDepartmentsWithPage", request);
        }

        public async Task<ApiResponse<PagedResult<HRDesignationViewModel>>> GetAllDesignationsWithPageAsync(SearchRequest request)
        {
            return await PostAsync<PagedResult<HRDesignationViewModel>>("api/HumanResourceApi/GetAllDesignationsWithPage", request);
        }

        public async Task<ApiResponse<PagedResult<HRLeaveTypeViewModel>>> GetAllLeaveTypesWithPageAsync(SearchRequest request)
        {
            return await PostAsync<PagedResult<HRLeaveTypeViewModel>>("api/HumanResourceApi/GetAllLeaveTypesWithPage", request);
        }
    }
}
