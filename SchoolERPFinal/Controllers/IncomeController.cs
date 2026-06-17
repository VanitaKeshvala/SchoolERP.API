using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Services.Clients;
using SchoolERP.Net.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchoolERP.Net.Controllers
{
    /// <summary>
    /// This controller manages the school's income, such as student fees or other payments received.
    /// </summary>
    public class IncomeController : Controller
    {
        private readonly IAccountHeadClientService _headClient;
        private readonly IAccountEntryClientService _entryClient;

        public IncomeController(IAccountHeadClientService headClient, IAccountEntryClientService entryClient)
        {
            _headClient = headClient;
            _entryClient = entryClient;
        }

        /// <summary>
        /// Retrieves and displays the main list of income entries and available income heads.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Fetch income entries and heads from the respective services
            var resEntries = await _entryClient.GetAllAccountEntriesAsync("Income");
            var resHeads = await _headClient.GetAllAccountHeadsAsync("Income");

            if (!resEntries.Success) ViewBag.ErrorMessage = resEntries.Message;

            // Prepare the view model for the income index page
            var model = new AccountEntryPageViewModel
            {
                Items = resEntries.Success ? resEntries.Data : new List<AccountEntryViewModel>(),
                Heads = resHeads.Success ? resHeads.Data : new List<AccountHeadViewModel>(),
                EntryType = "Income"
            };
            return View(model);
        }


        /// <summary>
        /// Shows the list of income categories (heads) configured in the system.
        /// </summary>
        public async Task<IActionResult> IncomeHead()
        {
            // Retrieve all income-related heads
            var res = await _headClient.GetAllAccountHeadsAsync("Income");
            var model = new AccountHeadPageViewModel
            {
                Items = res.Success ? res.Data : new List<AccountHeadViewModel>(),
                HeadType = "Income"
            };
            return View(model);
        }

        public IActionResult Search()
        {
            var model = new AccountEntrySearchViewModel { EntryType = "Income" };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Search(AccountEntrySearchRequest req)
        {
            var res = await _entryClient.SearchAccountEntriesAsync(req);
            var model = new AccountEntrySearchViewModel
            {
                Results = res.Success ? res.Data : new List<AccountEntryViewModel>(),
                EntryType = req.EntryType,
                SearchType = req.SearchType,
                DateFrom = req.DateFrom,
                DateTo = req.DateTo
            };
            if (!res.Success) ViewBag.ErrorMessage = res.Message;
            return View(model);
        }

        #region Account Head API Endpoints
        [HttpGet]
        public async Task<IActionResult> GetAccountHeadByID(int id)
        {
            var res = await _headClient.GetAccountHeadByIDAsync(id);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> UpsertAccountHead([FromBody] AccountHeadUpsertRequest req)
        {
            var res = await _headClient.UpsertAccountHeadAsync(req);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccountHead([FromBody] List<int> id)
        {
            var res = await _headClient.DeleteAccountHeadAsync(id);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleAccountHeadStatus(int id, bool isActive)
        {
            var res = await _headClient.ToggleAccountHeadStatusAsync(id, isActive);
            return Json(res);
        }
        #endregion

        #region Account Entry API Endpoints
        [HttpGet]
        public async Task<IActionResult> GetAccountEntryByID(int id)
        {
            var res = await _entryClient.GetAccountEntryByIDAsync(id);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> UpsertAccountEntry([FromBody] AccountEntryUpsertRequest req)
        {
            var res = await _entryClient.UpsertAccountEntryAsync(req);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccountEntry([FromBody] List<int> id)
        {
            var res = await _entryClient.DeleteAccountEntryAsync(id);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleAccountEntryStatus(int id, bool isActive)
        {
            var res = await _entryClient.ToggleAccountEntryStatusAsync(id, isActive);
            return Json(res);
        }
        #endregion
    }
}
