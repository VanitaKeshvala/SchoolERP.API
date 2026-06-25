using SchoolERP.Shared.Models;

namespace SchoolERP.API.Interfaces
{
    /// <summary>
    /// Interface for managing account entries (Income and Expense transactions).
    /// </summary>
    public interface IAccountEntryService
    {
        /// <summary>
        /// Retrieves a list of all account entries for a specific category (e.g., 'Income' or 'Expense').
        /// </summary>
        List<AccountEntryViewModel> GetAllAccountEntries(int companyId, int sessionId, string entryType, bool includeDeleted = false);

        /// <summary>
        /// Searches for account entries within a specific date range and category.
        /// </summary>
        List<AccountEntryViewModel> SearchAccountEntries(int companyId, int sessionId, string entryType, string searchType, string? dateFrom, string? dateTo);

        /// <summary>
        /// Finds and returns the details of a specific account entry using its unique ID.
        /// </summary>
        AccountEntryViewModel? GetAccountEntryByID(int id);

        /// <summary>
        /// Adds a new account entry or updates an existing one, including details like amount, date, and invoice number.
        /// </summary>
        (bool Success, string Message) UpsertAccountEntry(AccountEntryUpsertRequest req, int companyId, int sessionId, int userId);

        /// <summary>
        /// Removes an account entry from the system.
        /// </summary>
        (bool Success, string Message) DeleteAccountEntry(List<int> id, int userId);

        /// <summary>
        /// Turns an account entry's active status on or off.
        /// </summary>
        (bool Success, string Message) ToggleAccountEntryStatus(int id, bool isActive, int userId);
    }
}
