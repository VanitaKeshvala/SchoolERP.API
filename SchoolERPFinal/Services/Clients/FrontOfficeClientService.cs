using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class FrontOfficeClientService : BaseApiClient, IFrontOfficeClientService
    {
        public FrontOfficeClientService(HttpClient httpClient) : base(httpClient) { }

        // ─── PURPOSE ────────────────────────────────────────────
        public Task<ApiResponse<List<MstFOPurposeViewModel>>> GetAllPurposesAsync(bool includeDeleted = false)
            => GetAsync<List<MstFOPurposeViewModel>>($"api/FrontOfficeApi/GetAllPurposes?includeDeleted={includeDeleted}");

        public Task<ApiResponse<MstFOPurposeViewModel>> GetPurposeByIDAsync(int id)
            => GetAsync<MstFOPurposeViewModel>($"api/FrontOfficeApi/GetPurposeByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertPurposeAsync(MstFOPurposeUpsertRequest req)
            => PostAsync<dynamic>("api/FrontOfficeApi/UpsertPurpose", req);

        public Task<ApiResponse<dynamic>> DeletePurposeAsync(List<int> id)
            => PostAsync<dynamic>($"api/FrontOfficeApi/DeletePurpose", id!);

        public Task<ApiResponse<dynamic>> TogglePurposeStatusAsync(int id, bool isActive)
            => PostAsync<dynamic>($"api/FrontOfficeApi/TogglePurposeStatus?id={id}&isActive={isActive}", null!);

        // ─── COMPLAINT TYPE ─────────────────────────────────────
        public Task<ApiResponse<List<MstFOComplaintTypeViewModel>>> GetAllComplaintTypesAsync(bool includeDeleted = false)
            => GetAsync<List<MstFOComplaintTypeViewModel>>($"api/FrontOfficeApi/GetAllComplaintTypes?includeDeleted={includeDeleted}");

        public Task<ApiResponse<MstFOComplaintTypeViewModel>> GetComplaintTypeByIDAsync(int id)
            => GetAsync<MstFOComplaintTypeViewModel>($"api/FrontOfficeApi/GetComplaintTypeByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertComplaintTypeAsync(MstFOComplaintTypeUpsertRequest req)
            => PostAsync<dynamic>("api/FrontOfficeApi/UpsertComplaintType", req);

        public Task<ApiResponse<dynamic>> DeleteComplaintTypeAsync(List<int> id)
            => PostAsync<dynamic>($"api/FrontOfficeApi/DeleteComplaintType", id!);

        public Task<ApiResponse<dynamic>> ToggleComplaintTypeStatusAsync(int id, bool isActive)
            => PostAsync<dynamic>($"api/FrontOfficeApi/ToggleComplaintTypeStatus?id={id}&isActive={isActive}", null!);

        // ─── SOURCE ─────────────────────────────────────────────
        public Task<ApiResponse<List<MstFOSourceViewModel>>> GetAllSourcesAsync(bool includeDeleted = false)
            => GetAsync<List<MstFOSourceViewModel>>($"api/FrontOfficeApi/GetAllSources?includeDeleted={includeDeleted}");

        public Task<ApiResponse<MstFOSourceViewModel>> GetSourceByIDAsync(int id)
            => GetAsync<MstFOSourceViewModel>($"api/FrontOfficeApi/GetSourceByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertSourceAsync(MstFOSourceUpsertRequest req)
            => PostAsync<dynamic>("api/FrontOfficeApi/UpsertSource", req);

        public Task<ApiResponse<dynamic>> DeleteSourceAsync(List<int> id)
            => PostAsync<dynamic>($"api/FrontOfficeApi/DeleteSource", id!);

        public Task<ApiResponse<dynamic>> ToggleSourceStatusAsync(int id, bool isActive)
            => PostAsync<dynamic>($"api/FrontOfficeApi/ToggleSourceStatus?id={id}&isActive={isActive}", null!);

        // ─── REFERENCE ──────────────────────────────────────────
        public Task<ApiResponse<List<MstFOReferenceViewModel>>> GetAllReferencesAsync(bool includeDeleted = false)
            => GetAsync<List<MstFOReferenceViewModel>>($"api/FrontOfficeApi/GetAllReferences?includeDeleted={includeDeleted}");

        public Task<ApiResponse<MstFOReferenceViewModel>> GetReferenceByIDAsync(int id)
            => GetAsync<MstFOReferenceViewModel>($"api/FrontOfficeApi/GetReferenceByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertReferenceAsync(MstFOReferenceUpsertRequest req)
            => PostAsync<dynamic>("api/FrontOfficeApi/UpsertReference", req);

        public Task<ApiResponse<dynamic>> DeleteReferenceAsync(List<int> id)
            => PostAsync<dynamic>($"api/FrontOfficeApi/DeleteReference", id!);

        public Task<ApiResponse<dynamic>> ToggleReferenceStatusAsync(int id, bool isActive)
            => PostAsync<dynamic>($"api/FrontOfficeApi/ToggleReferenceStatus?id={id}&isActive={isActive}", null!);

        // ─── COMPLAINT ──────────────────────────────────────────
        public Task<ApiResponse<List<FOComplaintViewModel>>> GetAllComplaintsAsync(bool includeDeleted = false)
            => GetAsync<List<FOComplaintViewModel>>($"api/FrontOfficeApi/GetAllComplaints?includeDeleted={includeDeleted}");

        public Task<ApiResponse<FOComplaintViewModel>> GetComplaintByIDAsync(int id)
            => GetAsync<FOComplaintViewModel>($"api/FrontOfficeApi/GetComplaintByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertComplaintAsync(FOComplaintUpsertRequest req)
            => PostAsync<dynamic>("api/FrontOfficeApi/UpsertComplaint", req);

        public Task<ApiResponse<dynamic>> DeleteComplaintAsync(List<int> id)
            => PostAsync<dynamic>($"api/FrontOfficeApi/DeleteComplaint/", id);

        public Task<ApiResponse<dynamic>> ToggleComplaintStatusAsync(int id, bool isActive)
            => PostAsync<dynamic>($"api/FrontOfficeApi/ToggleComplaintStatus?id={id}&isActive={isActive}", null!);

        // ─── POSTAL RECEIVE ─────────────────────────────────────
        public Task<ApiResponse<List<FOPostalReceiveViewModel>>> GetAllPostalReceivesAsync(bool includeDeleted = false)
            => GetAsync<List<FOPostalReceiveViewModel>>($"api/FrontOfficeApi/GetAllPostalReceives?includeDeleted={includeDeleted}");

        public Task<ApiResponse<FOPostalReceiveViewModel>> GetPostalReceiveByIDAsync(int id)
            => GetAsync<FOPostalReceiveViewModel>($"api/FrontOfficeApi/GetPostalReceiveByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertPostalReceiveAsync(FOPostalReceiveUpsertRequest req)
            => PostAsync<dynamic>("api/FrontOfficeApi/UpsertPostalReceive", req);

        public Task<ApiResponse<dynamic>> DeletePostalReceiveAsync(List<int> id)
            => PostAsync<dynamic>($"api/FrontOfficeApi/DeletePostalReceive", id);

        public Task<ApiResponse<dynamic>> TogglePostalReceiveStatusAsync(int id, bool isActive)
            => PostAsync<dynamic>($"api/FrontOfficeApi/TogglePostalReceiveStatus?id={id}&isActive={isActive}", null!);

        // ─── POSTAL DISPATCH ─────────────────────────────────────
        public Task<ApiResponse<List<FOPostalDispatchViewModel>>> GetAllPostalDispatchesAsync(bool includeDeleted = false)
            => GetAsync<List<FOPostalDispatchViewModel>>($"api/FrontOfficeApi/GetAllPostalDispatches?includeDeleted={includeDeleted}");

        public Task<ApiResponse<FOPostalDispatchViewModel>> GetPostalDispatchByIDAsync(int id)
            => GetAsync<FOPostalDispatchViewModel>($"api/FrontOfficeApi/GetPostalDispatchByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertPostalDispatchAsync(FOPostalDispatchUpsertRequest req)
            => PostAsync<dynamic>("api/FrontOfficeApi/UpsertPostalDispatch", req);

        public Task<ApiResponse<dynamic>> DeletePostalDispatchAsync(List<int> id)
            => PostAsync<dynamic>($"api/FrontOfficeApi/DeletePostalDispatch",id);

        public Task<ApiResponse<dynamic>> TogglePostalDispatchStatusAsync(int id, bool isActive)
            => PostAsync<dynamic>($"api/FrontOfficeApi/TogglePostalDispatchStatus?id={id}&isActive={isActive}", null!);

        // ─── PHONE CALL LOG ─────────────────────────────────────
        public Task<ApiResponse<List<FOPhoneCallLogViewModel>>> GetAllPhoneCallLogsAsync(bool includeDeleted = false)
            => GetAsync<List<FOPhoneCallLogViewModel>>($"api/FrontOfficeApi/GetAllPhoneCallLogs?includeDeleted={includeDeleted}");

        public Task<ApiResponse<FOPhoneCallLogViewModel>> GetPhoneCallLogByIDAsync(int id)
            => GetAsync<FOPhoneCallLogViewModel>($"api/FrontOfficeApi/GetPhoneCallLogByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertPhoneCallLogAsync(FOPhoneCallLogUpsertRequest req)
            => PostAsync<dynamic>("api/FrontOfficeApi/UpsertPhoneCallLog", req);

        public Task<ApiResponse<dynamic>> DeletePhoneCallLogAsync(List<int> id)
            => PostAsync<dynamic>($"api/FrontOfficeApi/DeletePhoneCallLog", id!);

        public Task<ApiResponse<dynamic>> TogglePhoneCallLogStatusAsync(int id, bool isActive)
            => PostAsync<dynamic>($"api/FrontOfficeApi/TogglePhoneCallLogStatus?id={id}&isActive={isActive}", null!);

        // ─── VISITOR BOOK ───────────────────────────────────────
        public Task<ApiResponse<List<FOVisitorBookViewModel>>> GetAllVisitorsAsync(bool includeDeleted = false)
            => GetAsync<List<FOVisitorBookViewModel>>($"api/FrontOfficeApi/GetAllVisitors?includeDeleted={includeDeleted}");

        public Task<ApiResponse<FOVisitorBookViewModel>> GetVisitorByIDAsync(int id)
            => GetAsync<FOVisitorBookViewModel>($"api/FrontOfficeApi/GetVisitorByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertVisitorAsync(FOVisitorBookUpsertRequest req)
            => PostAsync<dynamic>("api/FrontOfficeApi/UpsertVisitor", req);

        public Task<ApiResponse<dynamic>> DeleteVisitorAsync(List<int> id)
            => PostAsync<dynamic>($"api/FrontOfficeApi/DeleteVisitor", id);

        public Task<ApiResponse<dynamic>> ToggleVisitorStatusAsync(int id, bool isActive)
            => PostAsync<dynamic>($"api/FrontOfficeApi/ToggleVisitorStatus?id={id}&isActive={isActive}", null!);

        // Admission Inquiry
        public Task<ApiResponse<List<FOAdmissionInquiryViewModel>>> GetAllAdmissionInquiriesAsync(System.DateTime? fromDate = null, System.DateTime? toDate = null, int sourceId = 0, int classId = 0, string? status = null)
        {
            var url = $"api/FrontOfficeApi/GetAllAdmissionInquiries?sourceId={sourceId}&classId={classId}";
            if (fromDate.HasValue) url += $"&fromDate={fromDate.Value:yyyy-MM-dd}";
            if (toDate.HasValue) url += $"&toDate={toDate.Value:yyyy-MM-dd}";
            if (!string.IsNullOrEmpty(status)) url += $"&status={status}";
            return GetAsync<List<FOAdmissionInquiryViewModel>>(url);
        }

        public Task<ApiResponse<FOAdmissionInquiryViewModel>> GetAdmissionInquiryByIDAsync(int id)
            => GetAsync<FOAdmissionInquiryViewModel>($"api/FrontOfficeApi/GetAdmissionInquiryByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertAdmissionInquiryAsync(FOAdmissionInquiryUpsertRequest req)
            => PostAsync<dynamic>("api/FrontOfficeApi/UpsertAdmissionInquiry", req);

        public Task<ApiResponse<dynamic>> DeleteAdmissionInquiryAsync(List<int> id)
            => PostAsync<dynamic>($"api/FrontOfficeApi/DeleteAdmissionInquiry/", id);

        public Task<ApiResponse<dynamic>> SaveInquiryFollowUpAsync(FOInquiryFollowUpSaveRequest req)
            => PostAsync<dynamic>("api/FrontOfficeApi/SaveInquiryFollowUp", req);
    }
}
