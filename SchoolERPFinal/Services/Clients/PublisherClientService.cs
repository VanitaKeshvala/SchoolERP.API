using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public class PublisherClientService: BaseApiClient,IPublisherClientService
    {
        public PublisherClientService(HttpClient client) : base(client) { }

        public async Task<ApiResponse<dynamic>> UpsertPublisherAsync(PublisherRequest request)
        {
            return await PostAsync<dynamic>("api/PublisherAPI/Save", request);
        }
        public async Task<ApiResponse<PublisherModel>> GetByIDAsync(int id)
        {
            return await GetAsync<PublisherModel>($"api/PublisherAPI/GetById/{id}");
        }
        public async Task<ApiResponse<PagedResult<PublisherModel>>> GetAllPublishertWithPageAsync(SearchRequest request)
        {
            return await PostAsync<PagedResult<PublisherModel>>("api/PublisherAPI/GetAllPublishertWithPage", request);
        }
        public async Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids)
        {
            return await PostAsync<dynamic>($"api/PublisherAPI/Delete", ids);
        }

        public async Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request)
        {
            return await PostAsync<dynamic>($"api/PublisherAPI/ToggleStatus", request);
        }
    }
}
