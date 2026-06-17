using SchoolERP.API.Models;
using SchoolERP.API.Models.Common;

namespace SchoolERP.API.Interfaces
{
    public interface IAuthServices
    {
        Task<ApiResponse<UserSessionModel?>> LoginAsync(string username, string password);
    }
}
