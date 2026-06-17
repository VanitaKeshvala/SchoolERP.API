using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Services.Clients;
using SchoolERP.Net.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchoolERP.Net.Controllers
{
    /// <summary>
    /// This controller manages the school's transport system, including bus routes, pickup points, and assigning vehicles to those routes.
    /// </summary>
    public class TransportController : Controller
    {
        private readonly IPickupPointClientService _pickupPointClient;
        private readonly IRouteClientService _routeClient;
        private readonly IVehicleClientService _vehicleClient;
        private readonly IVehicleAssignClientService _vehicleAssignClient;
        private readonly IRoutePickupPointClientService _rppClient;

        public TransportController(
            IPickupPointClientService pickupPointClient, 
            IRouteClientService routeClient,
            IVehicleClientService vehicleClient,
            IVehicleAssignClientService vehicleAssignClient,
            IRoutePickupPointClientService rppClient)
        {
            _pickupPointClient = pickupPointClient;
            _routeClient = routeClient;
            _vehicleClient = vehicleClient;
            _vehicleAssignClient = vehicleAssignClient;
            _rppClient = rppClient;
        }

        #region Pickup Point Endpoints
        public async Task<IActionResult> PickupPoint()
        {
            var res = await _pickupPointClient.GetAllPickupPointsAsync();
            var model = new PickupPointPageViewModel
            {
                Items = res.Success ? res.Data : new List<PickupPointViewModel>()
            };
            if (!res.Success) ViewBag.ErrorMessage = res.Message;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetPickupPointByID(int id)
        {
            var res = await _pickupPointClient.GetPickupPointByIDAsync(id);
            return Json(new { success = res.Success, data = res.Data, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> UpsertPickupPoint([FromBody] PickupPointUpsertRequest request)
        {
            var res = await _pickupPointClient.UpsertPickupPointAsync(request);
            return Json(new { success = res.Success, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeletePickupPoint([FromBody] List<int> id)
        {
            var res = await _pickupPointClient.DeletePickupPointAsync(id);
            return Json(new { success = true, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> TogglePickupPointStatus(int id, bool isActive)
        {
            var res = await _pickupPointClient.TogglePickupPointStatusAsync(id, isActive);
            return Json(new { success = res.Success, message = res.Message });
        }
        #endregion

        #region Routes Endpoints
        public async Task<IActionResult> Routes()
        {
            var res = await _routeClient.GetAllRoutesAsync();
            var model = new RoutePageViewModel
            {
                Items = res.Success ? res.Data : new List<RouteViewModel>()
            };
            if (!res.Success) ViewBag.ErrorMessage = res.Message;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetRouteByID(int id)
        {
            var res = await _routeClient.GetRouteByIDAsync(id);
            return Json(new { success = res.Success, data = res.Data, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> UpsertRoute([FromBody] RouteUpsertRequest request)
        {
            var res = await _routeClient.UpsertRouteAsync(request);
            return Json(new { success = res.Success, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRoute([FromBody] List<int> id)
        {
            var res = await _routeClient.DeleteRouteAsync(id);
            return Json(new { success = res.Success, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleRouteStatus(int id, bool isActive)
        {
            var res = await _routeClient.ToggleRouteStatusAsync(id, isActive);
            return Json(new { success = res.Success, message = res.Message });
        }
        #endregion

        #region Vehicles Endpoints
        public async Task<IActionResult> Vehicles()
        {
            var res = await _vehicleClient.GetAllVehiclesAsync();
            var model = new VehiclePageViewModel
            {
                Items = res.Success ? res.Data : new List<VehicleViewModel>()
            };
            if (!res.Success) ViewBag.ErrorMessage = res.Message;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetVehicleByID(int id)
        {
            var res = await _vehicleClient.GetVehicleByIDAsync(id);
            return Json(new { success = res.Success, data = res.Data, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> UpsertVehicle([FromForm] VehicleFormModel form)
        {
            var res = await _vehicleClient.UpsertVehicleAsync(form);
            return Json(new { success = res.Success, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVehicle([FromForm] List<int> id)
        {
            var res = await _vehicleClient.DeleteVehicleAsync(id);
            return Json(new { success = res.Success, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleVehicleStatus(int id, bool isActive)
        {
            var res = await _vehicleClient.ToggleVehicleStatusAsync(id, isActive);
            return Json(new { success = res.Success, message = res.Message });
        }
        #endregion

        #region Vehicle Assign Endpoints
        public async Task<IActionResult> VehicleAssign()
        {
            var res = await _vehicleAssignClient.GetAllAssignmentsAsync();
            var routesRes = await _routeClient.GetAllRoutesAsync();
            var vehiclesRes = await _vehicleClient.GetAllVehiclesAsync();
 
            var model = new VehicleAssignPageViewModel
            {
                Items = res.Success ? res.Data : new List<VehicleAssignViewModel>(),
                Routes = routesRes.Success ? routesRes.Data : new List<RouteViewModel>(),
                Vehicles = vehiclesRes.Success ? vehiclesRes.Data : new List<VehicleViewModel>()
            };
            
            if (!res.Success) ViewBag.ErrorMessage = res.Message;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpsertAssignments([FromBody] VehicleAssignUpsertRequest request)
        {
            var res = await _vehicleAssignClient.UpsertAssignmentsAsync(request);
            return Json(new { success = res.Success, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAssignment([FromBody] List<int> id)
        {
            var res = await _vehicleAssignClient.DeleteAssignmentAsync(id);
            return Json(new { success = res.Success, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleAssignmentStatus(int id, bool isActive)
        {
            var res = await _vehicleAssignClient.ToggleAssignmentStatusAsync(id, isActive);
            return Json(new { success = res.Success, message = res.Message });
        }
        #endregion

        #region Route Pickup Points Endpoints
        public async Task<IActionResult> RoutePickupPoints()
        {
            var res = await _rppClient.GetAllRoutePickupPointsAsync();
            var routesRes = await _routeClient.GetAllRoutesAsync();
            var pointsRes = await _pickupPointClient.GetAllPickupPointsAsync();

            var model = new RoutePickupPointPageViewModel
            {
                Items = res.Success ? res.Data : new List<RoutePickupPointViewModel>(),
                Routes = routesRes.Success ? routesRes.Data : new List<RouteViewModel>(),
                PickupPoints = pointsRes.Success ? pointsRes.Data : new List<PickupPointViewModel>()
            };
            
            if (!res.Success) ViewBag.ErrorMessage = res.Message;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetRoutePickupPointByID(int id)
        {
            var res = await _rppClient.GetRoutePickupPointByIDAsync(id);
            return Json(new { success = res.Success, data = res.Data, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> UpsertRoutePickupPoint([FromBody] RoutePickupPointUpsertRequest request)
        {
            var res = await _rppClient.UpsertRoutePickupPointAsync(request);
            return Json(new { success = res.Success, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRoutePickupPoint([FromBody] List<int> id)
        {
            var res = await _rppClient.DeleteRoutePickupPointAsync(id);
            return Json(new { success = true, message = res.Message });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleRoutePickupPointStatus(int id, bool isActive)
        {
            var res = await _rppClient.ToggleRoutePickupPointStatusAsync(id, isActive);
            return Json(new { success = res.Success, message = res.Message });
        }
        #endregion
    }
}
