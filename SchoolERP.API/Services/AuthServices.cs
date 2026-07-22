using Dapper;
using Microsoft.Data.SqlClient;
using SchoolERP.API.DTOs;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models;
using Common = SchoolERP.Shared.Models.Common;
using SchoolERP.API.Utilities;
using System.Data;

namespace SchoolERP.API.Services
{
    public class AuthServices: IAuthServices
    {
        private readonly IConfiguration _configuration;
        private readonly JwtHelper _jwtHelper;

        public AuthServices(IConfiguration configuration, JwtHelper jwtHelper)
        {
            _configuration = configuration;
            _jwtHelper = jwtHelper;
        }

        // This function is used to authenticate a user, retrieve session details, and generate a JWT token.
        public async Task<Common.ApiResponse<UserSessionModel?>> LoginAsync(string username,string password)
        {
            try
            {
                using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

                var param = new DynamicParameters();

                param.Add("@Username", username);
                param.Add("@PasswordPlain", password);

                using var multi = await conn.QueryMultipleAsync(
                    "sp_User_Login_Secure",
                    param,
                    commandType: CommandType.StoredProcedure);

                var loginResult =
                    await multi.ReadFirstOrDefaultAsync<LoginResultDto>();

                if (loginResult == null)
                {
                    return Common.ApiResponse<UserSessionModel?>
                        .ErrorResponse("User not found");
                }

                if (loginResult.Result == 0)
                {
                    return Common.ApiResponse<UserSessionModel?>
                        .ErrorResponse(loginResult.Message);
                }

                var user =await multi.ReadFirstOrDefaultAsync<UserSessionModel>();

                // Use empty string if UserTypeName is null or empty
                var userTypeName =
                    !string.IsNullOrWhiteSpace(user.UserTypeName)
                        ? user.UserTypeName
                        : string.Empty;

               
                // Result set 3: role-specific context (present only for Student/Parent/Staff)
                // multi.IsConsumed is false only if the SP actually returned this result set,
                // but since the SP always issues a SELECT for known roles, just check the grid is not empty.
                StudentRoleContextDto? studentRoleContextDto = null;
                if (userTypeName.Equals("Student", StringComparison.OrdinalIgnoreCase)
                    || userTypeName.Equals("Parent", StringComparison.OrdinalIgnoreCase))
                {
                    studentRoleContextDto = await multi.ReadFirstOrDefaultAsync<StudentRoleContextDto>();
                    if (studentRoleContextDto != null)
                    {
                        user.StudentID = studentRoleContextDto.StudentID;
                        user.ParentUserID = studentRoleContextDto.ParentUserID;
                    }
                }
                else if (userTypeName.Equals("Parent", StringComparison.OrdinalIgnoreCase))
                {
                    var staffCtx = await multi.ReadFirstOrDefaultAsync<StaffRoleContextDto>();
                    if (staffCtx != null)
                    {
                        user.StaffID = staffCtx.StaffID;
                    }
                }
                else
                {
                    var staffCtx = await multi.ReadFirstOrDefaultAsync<int>();
                    if (staffCtx != null)
                    {
                        user.StaffID = staffCtx;
                    }
                }
                // Generate JWT token for authenticated user
                user.Token = _jwtHelper.GenerateToken(
                    user.Username,
                    user.DefaultRoleName,
                    user.UserID,
                    user.UserTypeID,
                    user.DefaultRoleID,
                    userTypeName,
                    user.StaffID= user.StaffID,
                    studentRoleContextDto= studentRoleContextDto);

                return Common.ApiResponse<UserSessionModel?>
                    .SuccessResponse(user, loginResult.Message);
            }
            catch (Exception ex)
            {
                return Common.ApiResponse<UserSessionModel?>
            .ErrorResponse(ex.Message);
            }            
        }

        public async Task<DashboardModel?> GetDashboardByIdAsync(int dashboardId)
        {
            using var conn = new SqlConnection(
               _configuration.GetConnectionString("DefaultConnection"));

            return await conn.QueryFirstOrDefaultAsync<DashboardModel>(
                "SP_Dashboard_GetById",
                new { DASHBOARDID = dashboardId },
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
