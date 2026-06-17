using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This interface defines the rules for how the application communicates with the main system to manage the different ways payments can be made (like Razorpay).
    /// </summary>
    public interface IPaymentMethodClientService
    {
        /// <summary>
        /// Asks the main system for a list of all payment methods.
        /// </summary>
        Task<ApiResponse<List<MstPaymentMethodViewModel>>> GetAllPaymentMethodsAsync();

        /// <summary>
        /// Asks the main system for details about a specific payment method using its ID.
        /// </summary>
        Task<ApiResponse<MstPaymentMethodViewModel>> GetPaymentMethodByIdAsync(int paymentId);
        Task<ApiResponse<MstPaymentMethodViewModel>> GetPaymentMethodByCompanyAsync(int companyId);

        /// <summary>
        /// Sends information to the main system to either add a new payment method or update an existing one with keys and secrets.
        /// </summary>
        Task<ApiResponse<object>> UpsertPaymentMethodAsync(MstPaymentMethodUpsertRequest request);

        /// <summary>
        /// Tells the main system to remove a specific payment method.
        /// </summary>
        Task<ApiResponse<object>> DeletePaymentMethodAsync(int paymentId);

        /// <summary>
        /// Tells the main system to turn a payment method's active status on or off.
        /// </summary>
        Task<ApiResponse<object>> ToggleStatusAsync(int id, bool isActive);
    }
}
