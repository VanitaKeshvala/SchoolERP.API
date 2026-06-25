using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class AccountHeadClientService : BaseApiClient, IAccountHeadClientService
    {
        public AccountHeadClientService(HttpClient httpClient)
            : base(httpClient) { }

        public Task<ApiResponse<List<AccountHeadViewModel>>> GetAllAccountHeadsAsync(string headType, bool includeDeleted = false)
            => GetAsync<List<AccountHeadViewModel>>($"api/AccountHeadApi/GetAllAccountHeads?headType={headType}&includeDeleted={includeDeleted}");

        public Task<ApiResponse<AccountHeadViewModel>> GetAccountHeadByIDAsync(int id)
            => GetAsync<AccountHeadViewModel>($"api/AccountHeadApi/GetAccountHeadByID/{id}");

        public Task<ApiResponse<dynamic>> UpsertAccountHeadAsync(AccountHeadUpsertRequest req)
            => PostAsync<dynamic>("api/AccountHeadApi/UpsertAccountHead", req);

        public Task<ApiResponse<dynamic>> DeleteAccountHeadAsync(List<int> id)
            => PostAsync<dynamic>($"api/AccountHeadApi/DeleteAccountHead", id);

        public Task<ApiResponse<dynamic>> ToggleAccountHeadStatusAsync(int id, bool isActive)
            => PostAsync<dynamic>($"api/AccountHeadApi/ToggleAccountHeadStatus?id={id}&isActive={isActive}", null!);

       
    }
}
