using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Interfaces;
using SchoolERP.API.Models;
using SchoolERP.API.Models.Common;
using System.Security.Claims;

namespace SchoolERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AlumniEventApiController : ControllerBase
    {
        private readonly IAlumniEventService _alumniEventService;
        private readonly ICompanyService _companyService;
        private readonly ISessionService _sessionService;
        public AlumniEventApiController(IAlumniEventService alumniEventService,
            ICompanyService companyService,
            ISessionService sessionService)
        {
            _alumniEventService = alumniEventService;
            _companyService = companyService;
            _sessionService = sessionService;
        }
        private int GetUserId() => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("UserId")?.Value, out var id) ? id : 0;
        private int GetCompanyId() => _companyService.GetUserCurrentCompany(GetUserId()) ?? 0;
        private int GetSessionId() => _sessionService.GetUserCurrentSession(GetUserId()) ?? 0;
        /// <summary>
        /// Retrieves alumni events.
        /// </summary>
        /// <param name="searchText">Search text.</param>
        /// <returns>List of alumni events.</returns>
        [HttpGet]
        [Route("GetEvents")]
        public IActionResult GetEvents(string? searchText)
        {
            try
            {
                int companyId = GetCompanyId();

                var result = _alumniEventService.GetEvents(
                    searchText,
                    companyId);

                return Ok(new ApiResponse<List<AlumniEventViewModel>>
                {
                    Success = true,
                    Data = result,
                    Message = "Events retrieved successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<List<AlumniEventViewModel>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Creates or updates an alumni event.
        /// </summary>
        /// <param name="request">Event details.</param>
        /// <returns>Operation result.</returns>
        [HttpPost]
        [Route("UpsertEvent")]
        public  IActionResult UpsertEvent(
            [FromBody] AlumniEventUpsertRequest request)
        {
            try
            {
                int companyId =GetCompanyId();

                int userId =GetUserId();

                var result = _alumniEventService.UpsertEvent(
                    request,
                    companyId,
                    userId);

                return Ok(new ApiResponse<object>
                {
                    Success = result.Success,
                    Message = result.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes an alumni event.
        /// </summary>
        /// <param name="eventId">Event identifier.</param>
        /// <returns>Operation result.</returns>
        [HttpDelete]
        [Route("DeleteEvent")]
        public IActionResult DeleteEvent(int eventId)
        {
            try
            {
                int companyId = GetCompanyId();
                int userId = GetUserId();

                var result = _alumniEventService.DeleteEvent(
                    eventId,
                    companyId,
                    userId);

                return Ok(new ApiResponse<object>
                {
                    Success = result.Success,
                    Message = result.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Activates or deactivates an alumni event.
        /// </summary>
        /// <param name="eventId">Event identifier.</param>
        /// <param name="isActive">Active status.</param>
        /// <returns>Operation result.</returns>
        [HttpPost]
        [Route("ToggleEventStatus")]
        public IActionResult ToggleEventStatus(
            int eventId,
            bool isActive)
        {
            try
            {
                int companyId = GetCompanyId();
                int userId = GetUserId();

                var result = _alumniEventService.ToggleEventStatus(
                    eventId,
                    isActive,
                    companyId,
                    userId);

                return Ok(new ApiResponse<object>
                {
                    Success = result.Success,
                    Message = result.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Retrieves the photo associated with an alumni event.
        /// </summary>
        /// <param name="eventId">Event identifier.</param>
        /// <returns>Event photo file.</returns>
        [HttpGet]
        [Route("GetEventPhoto")]
        public IActionResult GetEventPhoto(int eventId)
        {
            try
            {
                int companyId = GetCompanyId();

                var result = _alumniEventService.GetEventPhoto(
                    eventId,
                    companyId);

                if (result.Bytes == null)
                    return NotFound();

                return File(
                    result.Bytes,
                    result.ContentType ?? "application/octet-stream",
                    result.FileName ?? "EventPhoto");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
