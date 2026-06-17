using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface IAccountHeadClientService
    {
        Task<ApiResponse<List<AccountHeadViewModel>>> GetAllAccountHeadsAsync(string headType, bool includeDeleted = false);
        Task<ApiResponse<AccountHeadViewModel>> GetAccountHeadByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertAccountHeadAsync(AccountHeadUpsertRequest req);
        Task<ApiResponse<dynamic>> DeleteAccountHeadAsync(List<int> id);
        Task<ApiResponse<dynamic>> ToggleAccountHeadStatusAsync(int id, bool isActive);
    }
}
