using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class StudentLeaveClientService: BaseApiClient, IStudentLeaveClientService
    {
        public StudentLeaveClientService(HttpClient httpClient) : base(httpClient)
        {
        }
        /// <summary>
        /// Retrieves student leave applications.
        /// </summary>
        /// <param name="classId">Class identifier.</param>
        /// <param name="sectionId">Section identifier.</param>
        /// <param name="status">Leave status.</param>
        /// <returns>List of leave applications.</returns>
        public async Task<ApiResponse<List<StudentLeaveViewModel>>> GetLeaveApplications(
            int? classId,
            int? sectionId,
            int? status)
        {
            string url =
                $"api/StudentLeaveApi/GetLeaveApplications?classId={classId}&sectionId={sectionId}&status={status}";

            return await GetAsync<List<StudentLeaveViewModel>>(url);
        }

        /// <summary>
        /// Updates the status of a student leave application.
        /// </summary>
        /// <param name="leaveAppId">The leave application identifier.</param>
        /// <param name="status">The new leave status.</param>
        /// <returns>The operation result.</returns>
        public async Task<ApiResponse<object>> UpdateLeaveStatus(
            int leaveAppId,
            int status)
        {
            return await PostAsync<object>(
                $"api/StudentLeaveApi/UpdateLeaveStatus?leaveAppId={leaveAppId}&status={status}",
                null);
        }

        /// <summary>
        /// Creates or updates a student leave application.
        /// </summary>
        /// <param name="request">Leave application details.</param>
        /// <returns>Operation result.</returns>
        public async Task<ApiResponse<object>> UpsertLeaveApplication(
            StudentLeaveUpsertRequest request)
        {
            return await PostAsync<object>(
                "api/StudentLeaveApi/UpsertLeaveApplication",
                request);
        }

        /// <summary>
        /// Retrieves a leave application attachment.
        /// </summary>
        /// <param name="leaveAppId">The leave application identifier.</param>
        /// <returns>File response.</returns>
        public async Task<HttpResponseMessage> GetLeaveAttachment(
            int leaveAppId)
        {
            return await _httpClient.GetAsync(
                $"api/StudentLeaveApi/GetLeaveAttachment?leaveAppId={leaveAppId}");
        }
    }
}
