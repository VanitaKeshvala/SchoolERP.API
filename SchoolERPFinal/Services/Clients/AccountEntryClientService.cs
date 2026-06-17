using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class AccountEntryClientService : BaseApiClient, IAccountEntryClientService
    {
        public AccountEntryClientService(HttpClient httpClient) : base(httpClient) { }

        public Task<ApiResponse<List<AccountEntryViewModel>>> GetAllAccountEntriesAsync(string entryType, bool includeDeleted = false)
            => GetAsync<List<AccountEntryViewModel>>($"api/AccountEntryApi/GetAllAccountEntries?entryType={entryType}&includeDeleted={includeDeleted}");
        
        public Task<ApiResponse<List<AccountEntryViewModel>>> SearchAccountEntriesAsync(AccountEntrySearchRequest req)
            => PostAsync<List<AccountEntryViewModel>>("api/AccountEntryApi/SearchAccountEntries", req);

        public Task<ApiResponse<AccountEntryViewModel>> GetAccountEntryByIDAsync(int id)
            => GetAsync<AccountEntryViewModel>($"api/AccountEntryApi/GetAccountEntryByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertAccountEntryAsync(AccountEntryUpsertRequest req)
            => PostAsync<dynamic>("api/AccountEntryApi/UpsertAccountEntry", req);

        public Task<ApiResponse<dynamic>> DeleteAccountEntryAsync(List<int> id)
            => PostAsync<dynamic>($"api/AccountEntryApi/DeleteAccountEntry", id);

        public Task<ApiResponse<dynamic>> ToggleAccountEntryStatusAsync(int id, bool isActive)
            => PostAsync<dynamic>($"api/AccountEntryApi/ToggleAccountEntryStatus?id={id}&isActive={isActive}", null!);
    }
}
