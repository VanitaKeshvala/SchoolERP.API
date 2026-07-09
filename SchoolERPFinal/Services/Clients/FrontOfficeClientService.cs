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

        public Task<ApiResponse<dynamic>> TogglePurposeStatusAsync(StatusUpdateRequest request)
            => PostAsync<dynamic>($"api/FrontOfficeApi/TogglePurposeStatus", request);

        // ─── COMPLAINT TYPE ─────────────────────────────────────
        public Task<ApiResponse<List<MstFOComplaintTypeViewModel>>> GetAllComplaintTypesAsync(bool includeDeleted = false)
            => GetAsync<List<MstFOComplaintTypeViewModel>>($"api/FrontOfficeApi/GetAllComplaintTypes?includeDeleted={includeDeleted}");

        public Task<ApiResponse<MstFOComplaintTypeViewModel>> GetComplaintTypeByIDAsync(int id)
            => GetAsync<MstFOComplaintTypeViewModel>($"api/FrontOfficeApi/GetComplaintTypeByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertComplaintTypeAsync(MstFOComplaintTypeUpsertRequest req)
            => PostAsync<dynamic>("api/FrontOfficeApi/UpsertComplaintType", req);

        public Task<ApiResponse<dynamic>> DeleteComplaintTypeAsync(List<int> id)
            => PostAsync<dynamic>($"api/FrontOfficeApi/DeleteComplaintType", id!);

        public Task<ApiResponse<dynamic>> ToggleComplaintTypeStatusAsync(StatusUpdateRequest request)
            => PostAsync<dynamic>($"api/FrontOfficeApi/ToggleComplaintTypeStatus", request);

        // ─── SOURCE ─────────────────────────────────────────────
        public Task<ApiResponse<List<MstFOSourceViewModel>>> GetAllSourcesAsync(bool includeDeleted = false)
            => GetAsync<List<MstFOSourceViewModel>>($"api/FrontOfficeApi/GetAllSources?includeDeleted={includeDeleted}");

        public Task<ApiResponse<MstFOSourceViewModel>> GetSourceByIDAsync(int id)
            => GetAsync<MstFOSourceViewModel>($"api/FrontOfficeApi/GetSourceByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertSourceAsync(MstFOSourceUpsertRequest req)
            => PostAsync<dynamic>("api/FrontOfficeApi/UpsertSource", req);

        public Task<ApiResponse<dynamic>> DeleteSourceAsync(List<int> id)
            => PostAsync<dynamic>($"api/FrontOfficeApi/DeleteSource", id!);

        public Task<ApiResponse<dynamic>> ToggleSourceStatusAsync(StatusUpdateRequest request)
            => PostAsync<dynamic>($"api/FrontOfficeApi/ToggleSourceStatus", request);

        // ─── REFERENCE ──────────────────────────────────────────
        public Task<ApiResponse<List<MstFOReferenceViewModel>>> GetAllReferencesAsync(bool includeDeleted = false)
            => GetAsync<List<MstFOReferenceViewModel>>($"api/FrontOfficeApi/GetAllReferences?includeDeleted={includeDeleted}");

        public Task<ApiResponse<MstFOReferenceViewModel>> GetReferenceByIDAsync(int id)
            => GetAsync<MstFOReferenceViewModel>($"api/FrontOfficeApi/GetReferenceByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertReferenceAsync(MstFOReferenceUpsertRequest req)
            => PostAsync<dynamic>("api/FrontOfficeApi/UpsertReference", req);

        public Task<ApiResponse<dynamic>> DeleteReferenceAsync(List<int> id)
            => PostAsync<dynamic>($"api/FrontOfficeApi/DeleteReference", id!);

        public Task<ApiResponse<dynamic>> ToggleReferenceStatusAsync(StatusUpdateRequest request)
            => PostAsync<dynamic>($"api/FrontOfficeApi/ToggleReferenceStatus", request);

        // ─── COMPLAINT ──────────────────────────────────────────
        public Task<ApiResponse<List<FOComplaintViewModel>>> GetAllComplaintsAsync(bool includeDeleted = false)
            => GetAsync<List<FOComplaintViewModel>>($"api/FrontOfficeApi/GetAllComplaints?includeDeleted={includeDeleted}");

        public Task<ApiResponse<FOComplaintViewModel>> GetComplaintByIDAsync(int id)
            => GetAsync<FOComplaintViewModel>($"api/FrontOfficeApi/GetComplaintByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertComplaintAsync(FOComplaintUpsertRequest req)
            => PostAsync<dynamic>("api/FrontOfficeApi/UpsertComplaint", req);

        public Task<ApiResponse<dynamic>> DeleteComplaintAsync(List<int> id)
            => PostAsync<dynamic>($"api/FrontOfficeApi/DeleteComplaint/", id);

        public Task<ApiResponse<dynamic>> ToggleComplaintStatusAsync(StatusUpdateRequest request)
            => PostAsync<dynamic>($"api/FrontOfficeApi/ToggleComplaintStatus", request);

        // ─── POSTAL RECEIVE ─────────────────────────────────────
        public Task<ApiResponse<List<FOPostalReceiveViewModel>>> GetAllPostalReceivesAsync(bool includeDeleted = false)
            => GetAsync<List<FOPostalReceiveViewModel>>($"api/FrontOfficeApi/GetAllPostalReceives?includeDeleted={includeDeleted}");

        public Task<ApiResponse<FOPostalReceiveViewModel>> GetPostalReceiveByIDAsync(int id)
            => GetAsync<FOPostalReceiveViewModel>($"api/FrontOfficeApi/GetPostalReceiveByID/{id}");

        public Task<ApiResponse<UpsertPostalReceiveResponse>> UpsertPostalReceiveAsync(FOPostalReceiveUpsertRequest req)
            => PostAsync<UpsertPostalReceiveResponse>("api/FrontOfficeApi/UpsertPostalReceive", req);

        public Task<ApiResponse<dynamic>> DeletePostalReceiveAsync(List<int> id)
            => PostAsync<dynamic>($"api/FrontOfficeApi/DeletePostalReceive", id);

        public Task<ApiResponse<dynamic>> TogglePostalReceiveStatusAsync(StatusUpdateRequest request)
            => PostAsync<dynamic>($"api/FrontOfficeApi/TogglePostalReceiveStatus", request);

        // ─── POSTAL DISPATCH ─────────────────────────────────────
        public Task<ApiResponse<List<FOPostalDispatchViewModel>>> GetAllPostalDispatchesAsync(bool includeDeleted = false)
            => GetAsync<List<FOPostalDispatchViewModel>>($"api/FrontOfficeApi/GetAllPostalDispatches?includeDeleted={includeDeleted}");

        public async Task<ApiResponse<PagedResult<FOPostalDispatchViewModel>>> GetAllPostalDispatchesWithPageIndexAsync(FOPostalDispatchSearchRequest request)
        {
            return await PostAsync<PagedResult<FOPostalDispatchViewModel>>("api/FrontOfficeApi/GetAllPostalDispatchesWithPageIndex", request);
        }

        public Task<ApiResponse<FOPostalDispatchViewModel>> GetPostalDispatchByIDAsync(int id)
            => GetAsync<FOPostalDispatchViewModel>($"api/FrontOfficeApi/GetPostalDispatchByID/{id}");

        public Task<ApiResponse<UpsertPostalDispatchResponse>> UpsertPostalDispatchAsync(FOPostalDispatchUpsertRequest req)
            => PostAsync<UpsertPostalDispatchResponse>("api/FrontOfficeApi/UpsertPostalDispatch", req);

        public Task<ApiResponse<dynamic>> DeletePostalDispatchAsync(List<int> id)
            => PostAsync<dynamic>($"api/FrontOfficeApi/DeletePostalDispatch",id);

        public Task<ApiResponse<dynamic>> TogglePostalDispatchStatusAsync(StatusUpdateRequest request)
            => PostAsync<dynamic>($"api/FrontOfficeApi/TogglePostalDispatchStatus", request);

        // ─── PHONE CALL LOG ─────────────────────────────────────
        public Task<ApiResponse<List<FOPhoneCallLogViewModel>>> GetAllPhoneCallLogsAsync(bool includeDeleted = false)
            => GetAsync<List<FOPhoneCallLogViewModel>>($"api/FrontOfficeApi/GetAllPhoneCallLogs?includeDeleted={includeDeleted}");

        public Task<ApiResponse<PagedResult<FOPhoneCallLogViewModel>>> GetAllPhoneCallLogsWithPageAsync(FOPhoneCallLogSearchRequest req)
            => PostAsync<PagedResult<FOPhoneCallLogViewModel>>("api/FrontOfficeApi/GetAllPhoneCallLogsWithPage", req);

        public Task<ApiResponse<FOPhoneCallLogViewModel>> GetPhoneCallLogByIDAsync(int id)
            => GetAsync<FOPhoneCallLogViewModel>($"api/FrontOfficeApi/GetPhoneCallLogByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertPhoneCallLogAsync(FOPhoneCallLogUpsertRequest req)
            => PostAsync<dynamic>("api/FrontOfficeApi/UpsertPhoneCallLog", req);

        public Task<ApiResponse<dynamic>> DeletePhoneCallLogAsync(List<int> id)
            => PostAsync<dynamic>($"api/FrontOfficeApi/DeletePhoneCallLog", id!);

        public Task<ApiResponse<dynamic>> TogglePhoneCallLogStatusAsync(StatusUpdateRequest request)
            => PostAsync<dynamic>($"api/FrontOfficeApi/TogglePhoneCallLogStatus", request);

        // ─── VISITOR BOOK ───────────────────────────────────────
        public Task<ApiResponse<List<FOVisitorBookViewModel>>> GetAllVisitorsAsync(bool includeDeleted = false)
            => GetAsync<List<FOVisitorBookViewModel>>($"api/FrontOfficeApi/GetAllVisitors?includeDeleted={includeDeleted}");


        public async Task<ApiResponse<PagedResult<FOVisitorBookViewModel>>> GetAllVisitorsWithPageIndexAsync(FOVisitorBookSerchRequest request)
        {
            return await PostAsync<PagedResult<FOVisitorBookViewModel>>("api/FrontOfficeApi/GetAllVisitorsWithPageIndex", request);
        }

        public Task<ApiResponse<FOVisitorBookViewModel>> GetVisitorByIDAsync(int id)
            => GetAsync<FOVisitorBookViewModel>($"api/FrontOfficeApi/GetVisitorByID/{id}");

        public Task<ApiResponse<UpsertVisitorResponse>> UpsertVisitorAsync(FOVisitorBookUpsertRequest req)
            => PostAsync<UpsertVisitorResponse>("api/FrontOfficeApi/UpsertVisitor", req);

        public Task<ApiResponse<dynamic>> DeleteVisitorAsync(List<int> id)
            => PostAsync<dynamic>($"api/FrontOfficeApi/DeleteVisitor", id);

        public Task<ApiResponse<dynamic>> ToggleVisitorStatusAsync(StatusUpdateRequest request)
            => PostAsync<dynamic>($"api/FrontOfficeApi/ToggleVisitorStatus", request);

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

        public Task<ApiResponse<PagedResult<FOAdmissionInquiryViewModel>>> GetAllAdmissionInquiriesWithPageIndexAsync(EnquirySearchRequest req)
            => PostAsync<PagedResult<FOAdmissionInquiryViewModel>>("api/FrontOfficeApi/GetAllAdmissionInquiriesWithPageIndex", req);

        public Task<ApiResponse<dynamic>> UpsertVisitorAttachmentFileAsync(FOVisitorBookAttachmentUpsertRequest req)
            => PostAsync<dynamic>("api/FrontOfficeApi/UpsertVisitorAttachmentFile", req);

        public Task<ApiResponse<dynamic>> UpsertPostalDispatchAttachmentFileAsync(FOPostalDispatchAttachmentUpsertRequest req)
            => PostAsync<dynamic>("api/FrontOfficeApi/UpsertPostalDispatchAttachmentFile", req);

        public async Task<ApiResponse<PagedResult<FOPostalReceiveViewModel>>> GetAllPostalReceiveWithPageAsync(ClassSearchRequest request)
        {
            return await PostAsync<PagedResult<FOPostalReceiveViewModel>>("api/FrontOfficeApi/GetAllPostalReceiveWithPage", request);
        }

        public Task<ApiResponse<dynamic>> UpsertPostalReceiveAttachmentFileAsync(FOPostalReceiveAttachmentUpsertRequest req)
            => PostAsync<dynamic>("api/FrontOfficeApi/UpsertPostalReceiveAttachmentFile", req);

        public async Task<ApiResponse<PagedResult<FOComplaintViewModel>>> GetAllComplaintsWithPageAsync(ComplaintSearchRequest request)
        {
            return await PostAsync<PagedResult<FOComplaintViewModel>>("api/FrontOfficeApi/GetAllComplaintsWithPage", request);
        }

        public async Task<ApiResponse<PagedResult<MstFOPurposeViewModel>>> GetAllPurposesWithPageAsync(ClassSearchRequest request)
        {
            return await PostAsync<PagedResult<MstFOPurposeViewModel>>("api/FrontOfficeApi/GetAllPurposesWithPage", request);
        }

        public async Task<ApiResponse<PagedResult<MstFOComplaintTypeViewModel>>> GetAllComplaintTypesWithPageAsync(ClassSearchRequest request)
        {
            return await PostAsync<PagedResult<MstFOComplaintTypeViewModel>>("api/FrontOfficeApi/GetAllComplaintTypesWithPage", request);
        }
        public async Task<ApiResponse<PagedResult<MstFOSourceViewModel>>> GetAllSourceWithPageAsync(ClassSearchRequest request)
        {
            return await PostAsync<PagedResult<MstFOSourceViewModel>>("api/FrontOfficeApi/GetAllSourceWithPage", request);
        }
        public async Task<ApiResponse<PagedResult<MstFOReferenceViewModel>>> GetAllReferenceWithPageAsync(ClassSearchRequest request)
        {
            return await PostAsync<PagedResult<MstFOReferenceViewModel>>("api/FrontOfficeApi/GetAllReferenceWithPage", request);
        }
    }
}
