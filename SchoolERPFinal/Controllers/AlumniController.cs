using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Services;
using SchoolERP.Shared.Models;
using SchoolERP.Net.Services.Clients;
using Microsoft.Extensions.Logging;
using SchoolERP.Net.Helpers;

namespace SchoolERP.Net.Controllers
{
    public class AlumniController : BaseController
    {
        private readonly IAlumniEventClientService _eventService;
        private readonly ICompanyClientService _companyService;
        private readonly ISessionClientService _sessionService;
        private readonly IClassClientService _classService;

        public AlumniController(IAlumniEventClientService eventService, ICompanyClientService companyService, ISessionClientService sessionService, IClassClientService classService, PermissionHelper permHelper) : base(permHelper)
        {
            _eventService = eventService;
            _companyService = companyService;
            _sessionService = sessionService;
            _classService = classService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("UserId")?.Value ?? "0");
        private async Task<int> GetCompanyId()
        {
            var response = await _companyService.GetUserCurrentCompanyAsync();
            return response?.Data ?? 0;
        }
        private async Task<int> GetSessionId()
        {
            var response = await _sessionService.GetUserCurrentSessionAsync();
            return response?.Data ?? 0;
        }
        public async Task<IActionResult> Events()
        {
            // Retrieves the logged-in user's access rights (View, Add, Edit, Delete, etc.)
            var perms = await GetPermissions(
               "/Alumni/Events"
           );
            ViewBag.SessionList = (await _sessionService.GetAllAsync()).Data;
            ViewBag.ClassList =(await _classService.GetAllAsync()).Data;
            return View(perms);
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents(string? searchText)
        {
            var response =await _eventService.GetEventsAsync(searchText);
            return Json(new { response.Success, response.Data });
        }

        [HttpPost]
        public async Task<IActionResult> UpsertEvent([FromForm] int EventID, [FromForm] string EventTitle, [FromForm] string? EventDescription,
                                        [FromForm] DateTime FromDate, [FromForm] DateTime ToDate, [FromForm] string? Location,
                                        [FromForm] int EventFor, [FromForm] int? SessionID, [FromForm] int? ClassID, [FromForm] string? SectionIDs,
                                        IFormFile? EventPhoto)
        {
            var req = new AlumniEventUpsertRequest
            {
                EventID = EventID,
                EventTitle = EventTitle,
                EventDescription = EventDescription,
                FromDate = FromDate,
                ToDate = ToDate,
                Location = Location,
                EventFor = EventFor,
                SessionID = SessionID,
                ClassID = ClassID,
                SectionIDs = SectionIDs
            };

            if (EventPhoto != null)
            {
                using (var ms = new MemoryStream())
                {
                    EventPhoto.CopyTo(ms);
                    req.EventPhoto = ms.ToArray();
                    req.EventPhotoName = EventPhoto.FileName;
                    req.EventPhotoType = EventPhoto.ContentType;
                }
            }

            var result = await _eventService.UpsertEventAsync(req);
            return Json(new { success = true, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userId = GetUserId();
            var result =await _eventService.DeleteEventAsync(id);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id, bool isActive)
        {
            var result =await _eventService.ToggleEventStatusAsync(id, isActive);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> DownloadPhoto(int id)
        {
          
            var response = await _eventService.GetEventPhotoAsync(id);

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var bytes = await response.Content.ReadAsByteArrayAsync();

            var contentType =
                response.Content.Headers.ContentType?.MediaType
                ?? "application/octet-stream";

            var fileName =
                response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                ?? "EventPhoto";
            if (bytes == null) return NotFound();
            return File(bytes, contentType ?? "image/jpeg", fileName ?? "event_photo");
        }
    }
}
