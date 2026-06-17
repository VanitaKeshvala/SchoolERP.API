using Dapper;
using Microsoft.Data.SqlClient;
using SchoolERP.API.Data;
using SchoolERP.API.Interfaces;
using SchoolERP.API.Models;
using System.Data;

namespace SchoolERP.API.Services
{
    /// <summary>
    /// This service manages leave applications submitted by students.
    /// It handles the submission of requests, status updates (approvals/rejections), 
    /// and any supporting documents or reasons provided.
    /// </summary>
    public class StudentLeaveService: IStudentLeaveService
    {
        private readonly IConfiguration _configuration;
        public StudentLeaveService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Retrieves all student leave applications based on class, section, and status filters.
        /// </summary>
        public List<StudentLeaveViewModel> GetLeaveApplications(int? classId, int? sectionId, int? status, int companyId)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();
            parameters.Add("@Action", "LIST");
            parameters.Add("@CompanyID", companyId);

            if (classId.HasValue && classId > 0)
                parameters.Add("@ClassID", classId.Value);

            if (sectionId.HasValue && sectionId > 0)
                parameters.Add("@SectionID", sectionId.Value);

            if (status.HasValue && status >= 0)
                parameters.Add("@Status", status.Value);

            return conn.Query<StudentLeaveViewModel>(
                "sp_Student_LeaveApp_CRUD",
                parameters,
                commandType: CommandType.StoredProcedure)
                .ToList();
        }
        /// <summary>
        /// Updates the status of a student's leave application.
        /// </summary>
        public (bool Success, string Message) UpdateLeaveStatus(
            int leaveAppId,
            int status,
            int companyId,
            int userId)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();
            parameters.Add("@Action", "UPDATE_STATUS");
            parameters.Add("@LeaveAppID", leaveAppId);
            parameters.Add("@Status", status);
            parameters.Add("@CompanyID", companyId);
            parameters.Add("@UserID", userId);

            var result = conn.QueryFirstOrDefault<dynamic>(
                "sp_Student_LeaveApp_CRUD",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (result != null)
            {
                return (true, Convert.ToString(result.Message));
            }

            return (false, "Failed to update status");
        }
        /// <summary>
        /// Creates a new leave application or updates an existing leave application.
        /// </summary>
        public (bool Success, string Message) UpsertLeaveApplication(
            StudentLeaveUpsertRequest req,
            int companyId,
            int userId)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();

            parameters.Add("@Action", "UPSERT");
            parameters.Add("@LeaveAppID", req.LeaveAppID);
            parameters.Add("@StudentID", req.StudentID);
            parameters.Add("@FromDate", req.FromDate);
            parameters.Add("@ToDate", req.ToDate);
            parameters.Add("@ApplyDate", req.ApplyDate);
            parameters.Add("@Reason", req.Reason);
            parameters.Add("@Attachment", req.Attachment, DbType.Binary);
            parameters.Add("@AttachmentType", req.AttachmentType);
            parameters.Add("@AttachmentName", req.AttachmentName);
            parameters.Add("@Status", req.Status);
            parameters.Add("@CompanyID", companyId);
            parameters.Add("@UserID", userId);

            var result = conn.QueryFirstOrDefault<dynamic>(
                "sp_Student_LeaveApp_CRUD",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (result != null)
            {
                return (
                    Convert.ToInt32(result.Result) == 1,
                    Convert.ToString(result.Message)
                );
            }

            return (false, "Failed to save leave application");
        }
        /// <summary>
        /// Retrieves the attachment file associated with a leave application.
        /// </summary>
        public (byte[]? Bytes, string? FileName, string? ContentType)
            GetLeaveAttachment(int leaveAppId, int companyId)
        {
            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var parameters = new DynamicParameters();
            parameters.Add("@Action", "GET_ATTACHMENT");
            parameters.Add("@LeaveAppID", leaveAppId);
            parameters.Add("@CompanyID", companyId);

            var result = conn.QueryFirstOrDefault<dynamic>(
                "sp_Student_LeaveApp_CRUD",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (result != null && result.Attachment != null)
            {
                return (
                    (byte[])result.Attachment,
                    Convert.ToString(result.AttachmentName),
                    Convert.ToString(result.AttachmentType)
                );
            }

            return (null, null, null);
        }
    }
}
