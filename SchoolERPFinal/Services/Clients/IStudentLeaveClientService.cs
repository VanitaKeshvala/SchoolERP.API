using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface IStudentLeaveClientService
    {
        /// <summary>
        /// Retrieves student leave applications.
        /// </summary>
        /// <param name="classId">Class identifier.</param>
        /// <param name="sectionId">Section identifier.</param>
        /// <param name="status">Leave status.</param>
        /// <returns>List of leave applications.</returns>
        Task<ApiResponse<List<StudentLeaveViewModel>>> GetLeaveApplications(
            int? classId,
            int? sectionId,
            int? status);

        /// <summary>
        /// Updates the status of a student leave application.
        /// </summary>
        /// <param name="leaveAppId">The leave application identifier.</param>
        /// <param name="status">The new leave status.</param>
        /// <returns>The operation result.</returns>
        Task<ApiResponse<object>> UpdateLeaveStatus(
            int leaveAppId,
            int status);

        /// <summary>
        /// Creates or updates a student leave application.
        /// </summary>
        /// <param name="request">Leave application details.</param>
        /// <returns>Operation result.</returns>
        Task<ApiResponse<UpsertLeaveApplicationResponse>> UpsertLeaveApplication(
            StudentLeaveUpsertRequest request);

        /// <summary>
        /// Retrieves a leave application attachment.
        /// </summary>
        /// <param name="leaveAppId">The leave application identifier.</param>
        /// <returns>File response.</returns>
        Task<HttpResponseMessage> GetLeaveAttachment(int leaveAppId);
        Task<ApiResponse<StudentLeaveViewModel>> GetLeaveApplicationsById(
            int? leaveAppId,
            int? companyId);

        /// <summary>
        /// Updates/replaces the attachment file for an existing student leave application
        /// without affecting the other leave application fields.
        /// </summary>
        /// <param name="req">
        /// Request payload containing the leave application ID and the new attachment
        /// (file data, type, and name).
        /// </param>
        /// <param name="userId">ID of the user performing the update (for audit/history).</param>
        /// <returns>
        /// A tuple indicating whether the update succeeded (<c>success</c>) and a corresponding
        /// user-friendly <c>message</c>.
        /// </returns>
        Task<ApiResponse<dynamic>> UpsertLeaveApplicationAttachmentFileAsync(LeaveApplicationAttachmentUpsertRequest req);

        Task<ApiResponse<object>> DeleteLeaveApplicationAsync(List<int> studentIds, int companyId);
    }
}
