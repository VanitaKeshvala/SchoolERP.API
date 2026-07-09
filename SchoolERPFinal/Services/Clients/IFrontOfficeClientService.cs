using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface IFrontOfficeClientService
    {
        // Purpose
        Task<ApiResponse<List<MstFOPurposeViewModel>>> GetAllPurposesAsync(bool includeDeleted = false);
        Task<ApiResponse<MstFOPurposeViewModel>> GetPurposeByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertPurposeAsync(MstFOPurposeUpsertRequest req);
        Task<ApiResponse<dynamic>> DeletePurposeAsync(List<int> id);
        Task<ApiResponse<dynamic>> TogglePurposeStatusAsync(StatusUpdateRequest request);

        // Complaint Type
        Task<ApiResponse<List<MstFOComplaintTypeViewModel>>> GetAllComplaintTypesAsync(bool includeDeleted = false);
        Task<ApiResponse<MstFOComplaintTypeViewModel>> GetComplaintTypeByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertComplaintTypeAsync(MstFOComplaintTypeUpsertRequest req);
        Task<ApiResponse<dynamic>> DeleteComplaintTypeAsync(List<int> id);
        Task<ApiResponse<dynamic>> ToggleComplaintTypeStatusAsync(StatusUpdateRequest request);

        // Source
        Task<ApiResponse<List<MstFOSourceViewModel>>> GetAllSourcesAsync(bool includeDeleted = false);
        Task<ApiResponse<MstFOSourceViewModel>> GetSourceByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertSourceAsync(MstFOSourceUpsertRequest req);
        Task<ApiResponse<dynamic>> DeleteSourceAsync(List<int> id);
        Task<ApiResponse<dynamic>> ToggleSourceStatusAsync(StatusUpdateRequest request);

        // Reference
        Task<ApiResponse<List<MstFOReferenceViewModel>>> GetAllReferencesAsync(bool includeDeleted = false);
        Task<ApiResponse<MstFOReferenceViewModel>> GetReferenceByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertReferenceAsync(MstFOReferenceUpsertRequest req);
        Task<ApiResponse<dynamic>> DeleteReferenceAsync(List<int> id);
        Task<ApiResponse<dynamic>> ToggleReferenceStatusAsync(StatusUpdateRequest request);

        // Complaint
        Task<ApiResponse<List<FOComplaintViewModel>>> GetAllComplaintsAsync(bool includeDeleted = false);
        Task<ApiResponse<FOComplaintViewModel>> GetComplaintByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertComplaintAsync(FOComplaintUpsertRequest req);
        Task<ApiResponse<dynamic>> DeleteComplaintAsync(List<int> id);
        Task<ApiResponse<dynamic>> ToggleComplaintStatusAsync(StatusUpdateRequest request);

        // Postal Receive
        Task<ApiResponse<List<FOPostalReceiveViewModel>>> GetAllPostalReceivesAsync(bool includeDeleted = false);
        Task<ApiResponse<FOPostalReceiveViewModel>> GetPostalReceiveByIDAsync(int id);
        Task<ApiResponse<UpsertPostalReceiveResponse>> UpsertPostalReceiveAsync(FOPostalReceiveUpsertRequest req);
        Task<ApiResponse<dynamic>> DeletePostalReceiveAsync(List<int> id);
        Task<ApiResponse<dynamic>> TogglePostalReceiveStatusAsync(StatusUpdateRequest request);

        // Postal Dispatch
        Task<ApiResponse<List<FOPostalDispatchViewModel>>> GetAllPostalDispatchesAsync(bool includeDeleted = false);
        Task<ApiResponse<PagedResult<FOPostalDispatchViewModel>>> GetAllPostalDispatchesWithPageIndexAsync(FOPostalDispatchSearchRequest request);
        Task<ApiResponse<FOPostalDispatchViewModel>> GetPostalDispatchByIDAsync(int id);
        Task<ApiResponse<UpsertPostalDispatchResponse>> UpsertPostalDispatchAsync(FOPostalDispatchUpsertRequest req);
        Task<ApiResponse<dynamic>> DeletePostalDispatchAsync(List<int> id);
        Task<ApiResponse<dynamic>> TogglePostalDispatchStatusAsync(StatusUpdateRequest request);

        // Phone Call Log
        Task<ApiResponse<List<FOPhoneCallLogViewModel>>> GetAllPhoneCallLogsAsync(bool includeDeleted = false);
        Task<ApiResponse<PagedResult<FOPhoneCallLogViewModel>>> GetAllPhoneCallLogsWithPageAsync(FOPhoneCallLogSearchRequest req);
        Task<ApiResponse<FOPhoneCallLogViewModel>> GetPhoneCallLogByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertPhoneCallLogAsync(FOPhoneCallLogUpsertRequest req);
        Task<ApiResponse<dynamic>> DeletePhoneCallLogAsync(List<int> id);
        Task<ApiResponse<dynamic>> TogglePhoneCallLogStatusAsync(StatusUpdateRequest request);

        // Visitor Book
        Task<ApiResponse<List<FOVisitorBookViewModel>>> GetAllVisitorsAsync(bool includeDeleted = false);
        Task<ApiResponse<PagedResult<FOVisitorBookViewModel>>> GetAllVisitorsWithPageIndexAsync(FOVisitorBookSerchRequest request);
        Task<ApiResponse<FOVisitorBookViewModel>> GetVisitorByIDAsync(int id);
        Task<ApiResponse<UpsertVisitorResponse>> UpsertVisitorAsync(FOVisitorBookUpsertRequest req);
        Task<ApiResponse<dynamic>> DeleteVisitorAsync(List<int> id);
        Task<ApiResponse<dynamic>> ToggleVisitorStatusAsync(StatusUpdateRequest request);

        // Admission Inquiry
        Task<ApiResponse<List<FOAdmissionInquiryViewModel>>> GetAllAdmissionInquiriesAsync(System.DateTime? fromDate = null, System.DateTime? toDate = null, int sourceId = 0, int classId = 0, string? status = null);
        Task<ApiResponse<FOAdmissionInquiryViewModel>> GetAdmissionInquiryByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertAdmissionInquiryAsync(FOAdmissionInquiryUpsertRequest req);
        Task<ApiResponse<dynamic>> DeleteAdmissionInquiryAsync(List<int> id);
        Task<ApiResponse<dynamic>> SaveInquiryFollowUpAsync(FOInquiryFollowUpSaveRequest req);

        Task<ApiResponse<PagedResult<FOAdmissionInquiryViewModel>>> GetAllAdmissionInquiriesWithPageIndexAsync(EnquirySearchRequest req);
        Task<ApiResponse<dynamic>> UpsertVisitorAttachmentFileAsync(FOVisitorBookAttachmentUpsertRequest req);
        Task<ApiResponse<dynamic>> UpsertPostalDispatchAttachmentFileAsync(FOPostalDispatchAttachmentUpsertRequest req);

        Task<ApiResponse<PagedResult<FOPostalReceiveViewModel>>> GetAllPostalReceiveWithPageAsync(ClassSearchRequest request);
        Task<ApiResponse<dynamic>> UpsertPostalReceiveAttachmentFileAsync(FOPostalReceiveAttachmentUpsertRequest req);
        Task<ApiResponse<PagedResult<FOComplaintViewModel>>> GetAllComplaintsWithPageAsync(ComplaintSearchRequest request);
        Task<ApiResponse<PagedResult<MstFOPurposeViewModel>>> GetAllPurposesWithPageAsync(ClassSearchRequest request);
        Task<ApiResponse<PagedResult<MstFOComplaintTypeViewModel>>> GetAllComplaintTypesWithPageAsync(ClassSearchRequest request);

        Task<ApiResponse<PagedResult<MstFOSourceViewModel>>> GetAllSourceWithPageAsync(ClassSearchRequest request);
        Task<ApiResponse<PagedResult<MstFOReferenceViewModel>>> GetAllReferenceWithPageAsync(ClassSearchRequest request);
    }
}
