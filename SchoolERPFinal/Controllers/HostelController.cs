using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Models;
using SchoolERP.Net.Services.Clients;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchoolERP.Net.Controllers
{
    public class HostelController : Controller
    {
        private readonly IHostelClientService _client;
        public HostelController(IHostelClientService client) => _client = client;


        /// <summary>
        /// Displays a list of all available room types so you can see the different options for housing.
        /// </summary>
        public async Task<IActionResult> RoomType()
        {
            // Step 1: Ask the system to fetch the full list of room types.
            var res = await _client.GetAllRoomTypesAsync();
            
            // Step 2: Organize the data found or provide an empty list if nothing was retrieved.
            var model = new RoomTypePageViewModel
            {
                Items = res.Success ? res.Data : new List<RoomTypeViewModel>()
            };
            
            // Step 3: Send the room type information to the display page.
            return View(model);
        }

        /// <summary>
        /// Shows the main hostel page, including a list of all hostels and their available room types.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Step 1: Gather information about all hostels and room types from the system.
            var resHostel = await _client.GetAllHostelsAsync();
            var resRoomType = await _client.GetAllRoomTypesAsync();
            
            // Step 2: Prepare the data for the screen, handling cases where data might be missing.
            var model = new HostelPageViewModel
            {
                Items = resHostel.Success ? resHostel.Data : new List<HostelViewModel>(),
                RoomTypes = resRoomType.Success ? resRoomType.Data : new List<RoomTypeViewModel>()
            };
            
            // Step 3: Present the dashboard to the user.
            return View(model);
        }

        /// <summary>
        /// Displays details for specific hostel rooms, including which hostel they belong to and their room type.
        /// </summary>
        public async Task<IActionResult> HostelRoom()
        {
            // Step 1: Retrieve all room records, hostel names, and category types.
            var resRoom = await _client.GetAllHostelRoomsAsync();
            var resHostel = await _client.GetAllHostelsAsync();
            var resRoomType = await _client.GetAllRoomTypesAsync();

            var model = new HostelRoomPageViewModel
            {
                Items = resRoom.Success ? resRoom.Data : new List<HostelRoomViewModel>(),
                Hostels = resHostel.Success ? resHostel.Data : new List<HostelViewModel>(),
                RoomTypes = resRoomType.Success ? resRoomType.Data : new List<RoomTypeViewModel>()
            };
            return View(model);
        }

        #region Room Type API Endpoints
        [HttpGet]
        public async Task<IActionResult> GetRoomTypeByID(int id)
        {
            var res = await _client.GetRoomTypeByIDAsync(id);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> UpsertRoomType([FromBody] RoomTypeUpsertRequest req)
        {
            var res = await _client.UpsertRoomTypeAsync(req);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRoomType([FromBody] List<int> id)
        {
            var res = await _client.DeleteRoomTypeAsync(id);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleRoomTypeStatus(int id, bool isActive)
        {
            var res = await _client.ToggleRoomTypeStatusAsync(id, isActive);
            return Json(res);
        }
        #endregion

        #region Hostel API Endpoints
        [HttpGet]
        public async Task<IActionResult> GetHostelByID(int id)
        {
            var res = await _client.GetHostelByIDAsync(id);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> UpsertHostel([FromBody] HostelUpsertRequest req)
        {
            var res = await _client.UpsertHostelAsync(req);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteHostel([FromBody] List<int> id)
        {
            var res = await _client.DeleteHostelAsync(id);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleHostelStatus(int id, bool isActive)
        {
            var res = await _client.ToggleHostelStatusAsync(id, isActive);
            return Json(res);
        }
        #endregion

        #region Hostel Room API Endpoints
        [HttpGet]
        public async Task<IActionResult> GetHostelRoomByID(int id)
        {
            var res = await _client.GetHostelRoomByIDAsync(id);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> UpsertHostelRoom([FromBody] HostelRoomUpsertRequest req)
        {
            var res = await _client.UpsertHostelRoomAsync(req);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteHostelRoom([FromBody] List<int> id)
        {
            var res = await _client.DeleteHostelRoomAsync(id);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleHostelRoomStatus(int id, bool isActive)
        {
            var res = await _client.ToggleHostelRoomStatusAsync(id, isActive);
            return Json(res);
        }
        #endregion
    }
}
