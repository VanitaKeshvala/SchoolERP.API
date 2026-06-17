using SchoolERP.API.Models;

namespace SchoolERP.API.Interfaces
{
    /// <summary>
    /// Interface for managing account heads (e.g., 'Tuition Fees', 'Electricity Expense').
    /// </summary>
    public interface IAccountHeadService
    {
        /// <summary>
        /// Retrieves a list of all account heads for a specific category (e.g., 'Income' or 'Expense').
        /// </summary>
        List<AccountHeadViewModel> GetAllAccountHeads(int companyId, int sessionId, string headType, bool includeDeleted = false);

        /// <summary>
        /// Finds and returns the details of a specific account head using its unique ID.
        /// </summary>
        AccountHeadViewModel? GetAccountHeadByID(int id);

        /// <summary>
        /// Adds a new account head or updates an existing one in the database.
        /// </summary>
        (bool Success, string Message) UpsertAccountHead(AccountHeadUpsertRequest req, int companyId, int sessionId, int userId);

        /// <summary>
        /// Removes an account head from the system.
        /// </summary>
        (bool Success, string Message) DeleteAccountHead(List<int> id, int userId);

        /// <summary>
        /// Turns an account head's active status on or off.
        /// </summary>
        (bool Success, string Message) ToggleAccountHeadStatus(int id, bool isActive, int userId);
    }
}
