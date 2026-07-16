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

                int? staffId = 17;

                // Generate JWT token for authenticated user
                user.Token = _jwtHelper.GenerateToken(
                    user.Username,
                    user.DefaultRoleName,
                    user.UserID,
                    user.UserTypeID,
                    user.DefaultRoleID,
                    userTypeName,
                    user.StaffID= staffId);

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
