using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface IAccountEntryClientService
    {
        Task<ApiResponse<List<AccountEntryViewModel>>> GetAllAccountEntriesAsync(string entryType, bool includeDeleted = false);
        Task<ApiResponse<List<AccountEntryViewModel>>> SearchAccountEntriesAsync(AccountEntrySearchRequest req);
        Task<ApiResponse<AccountEntryViewModel>> GetAccountEntryByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertAccountEntryAsync(AccountEntryUpsertRequest req);
        Task<ApiResponse<dynamic>> DeleteAccountEntryAsync(List<int> id);
        Task<ApiResponse<dynamic>> ToggleAccountEntryStatusAsync(int id, bool isActive);
    }
}
