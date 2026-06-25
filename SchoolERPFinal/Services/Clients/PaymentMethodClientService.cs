using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This service handles the actual work of sending requests to the main system to manage payment method details in the database via an API.
    /// </summary>
    public class PaymentMethodClientService : BaseApiClient, IPaymentMethodClientService
    {
        public PaymentMethodClientService(HttpClient httpClient) : base(httpClient) { }

        /// <summary>
        /// Sends a request to the server to get a list of all payment methods.
        /// </summary>
        public async Task<ApiResponse<List<MstPaymentMethodViewModel>>> GetAllPaymentMethodsAsync()
        {
            return await GetAsync<List<MstPaymentMethodViewModel>>("api/PaymentMethodApi/GetAll");
        }

        /// <summary>
        /// Sends a request to the server to look up details for a specific payment method by its ID.
        /// </summary>
        public async Task<ApiResponse<MstPaymentMethodViewModel>> GetPaymentMethodByIdAsync(int paymentId)
        {
            return await GetAsync<MstPaymentMethodViewModel>($"api/PaymentMethodApi/GetById/{paymentId}");
        }

        /// <summary>
        /// Sends a request to the server to look up details for a payment method by company ID.
        /// </summary>
        public async Task<ApiResponse<MstPaymentMethodViewModel>> GetPaymentMethodByCompanyAsync(int companyId)
        {
            return await GetAsync<MstPaymentMethodViewModel>($"api/PaymentMethodApi/GetByCompany/{companyId}");
        }

        /// <summary>
        /// Sends payment method details (like keys and secrets) to the server to be saved or updated.
        /// </summary>
        public async Task<ApiResponse<object>> UpsertPaymentMethodAsync(MstPaymentMethodUpsertRequest request)
        {
            return await PostAsync<object>("api/PaymentMethodApi/Upsert", request);
        }

        /// <summary>
        /// Sends a request to the server to delete a payment method record.
        /// </summary>
        public async Task<ApiResponse<object>> DeletePaymentMethodAsync(int paymentId)
        {
            return await PostAsync<object>($"api/PaymentMethodApi/Delete/{paymentId}", null);
        }

        /// <summary>
        /// Sends a request to the server to update whether a payment method is currently enabled.
        /// </summary>
        public async Task<ApiResponse<object>> ToggleStatusAsync(int id, bool isActive)
        {
            return await PostAsync<object>($"api/PaymentMethodApi/ToggleStatus?id={id}&isActive={isActive}", null);
        }
    }
}
