using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Interfaces;
using SchoolERP.API.Models;
using SchoolERP.API.Models.Common;
using System.ComponentModel.Design;
using System.Security.Claims;

namespace SchoolERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class DownloadCenterController : ControllerBase
    {
        private readonly IDownloadCenterService _downloadCenterService;
        private readonly ICompanyService _companyService;
        private readonly ISessionService _sessionService;
        private readonly IAttendanceService _attendanceService;

        public DownloadCenterController(IDownloadCenterService downloadCenterService,
            ICompanyService companyService,
            ISessionService sessionService,
            IAttendanceService attendanceService)
        {
            _downloadCenterService = downloadCenterService;
            _companyService = companyService;
            _sessionService = sessionService;
            _attendanceService = attendanceService;
        }

        private int GetUserId() => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("UserId")?.Value, out var id) ? id : 0;
        private int GetCompanyId() => _companyService.GetUserCurrentCompany(GetUserId()) ?? 0;
        private int GetSessionId() => _sessionService.GetUserCurrentSession(GetUserId()) ?? 0;

        /// <summary>
        /// Retrieves all content types for the current company.
        /// Optionally filters the results using the supplied search term.
        /// </summary>
        /// <param name="searchTerm">
        /// Optional text used to filter content types by name or description.
        /// </param>
        /// <returns>
        /// A collection of content type records.
        /// </returns>
        [HttpGet("GetContentTypeList")]
        public IActionResult GetContentTypeList(string? searchTerm)
        {
            var result = _downloadCenterService.GetContentTypeList(GetCompanyId(), searchTerm);

            return Ok(new ApiResponse<List<ContentTypeViewModel>>
            {
                Success = true,
                Data = result
            });
        }

        /// <summary>
        /// Retrieves a specific content type by its identifier.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the content type.
        /// </param>
        /// <returns>
        /// The requested content type details.
        /// </returns>
        [HttpGet("GetContentTypeById")]
        public IActionResult GetContentTypeById(int id)
        {
            var result = _downloadCenterService.GetContentTypeById(id);

            return Ok(new ApiResponse<ContentTypeViewModel>
            {
                Success = result != null,
                Data = result,
                Message = result != null ? "Record found." : "Record not found."
            });
        }

        /// <summary>
        /// Creates a new content type or updates an existing content type.
        /// This method is used to save content type information.
        /// </summary>
        /// <param name="request">
        /// The content type information to be saved.
        /// </param>
        /// <returns>
        /// A response indicating whether the save operation was successful.
        /// </returns>
        [HttpPost("UpsertContentType")]
        public IActionResult UpsertContentType(ContentTypeUpsertRequest request)
        {
            var result = _downloadCenterService.UpsertContentType(
                request,
                GetCompanyId(),
                GetUserId());

            return Ok(new ApiResponse<bool>
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Success
            });
        }

        /// <summary>
        /// Deletes the specified content type.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the content type.
        /// </param>
        /// <returns>
        /// A response indicating whether the delete operation was successful.
        /// </returns>
        [HttpPost("DeleteContentType")]
        public IActionResult DeleteContentType(List<int> id)
        {
            var res = _downloadCenterService.DeleteContentType(
                id,
                GetUserId());

            return Ok(new ApiResponse<bool>
            {
                Success = res.Success,
                Message = res.Message,
                Data = res.Success
            });
        }

        /// <summary>
        /// Changes the active status of a content type.
        /// If the content type is active, it will be deactivated.
        /// If it is inactive, it will be activated.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the content type.
        /// </param>
        /// <returns>
        /// A response indicating whether the status change was successful.
        /// </returns>
        [HttpPost("ToggleContentTypeStatus")]
        public IActionResult ToggleContentTypeStatus(int id)
        {
            var result = _downloadCenterService.ToggleContentTypeStatus(
                id,
                GetUserId(),
                GetCompanyId());

            return Ok(new ApiResponse<bool>
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Success
            });
        }
        /// <summary>
        /// Retrieves video tutorials filtered by class, section, and search term.
        /// </summary>
        /// <param name="classId">
        /// Optional class identifier.
        /// </param>
        /// <param name="sectionId">
        /// Optional section identifier.
        /// </param>
        /// <param name="searchTerm">
        /// Optional search text.
        /// </param>
        /// <returns>
        /// A collection of video tutorials.
        /// </returns>
        [HttpGet("GetVideoTutorialList")]
        public IActionResult GetVideoTutorialList(
            int? classId,
            int? sectionId,
            string? searchTerm)
        {
            var result = _downloadCenterService
                .GetVideoTutorialList(
                GetCompanyId(),
                classId,
                sectionId,
                searchTerm);

            return Ok(new ApiResponse<List<VideoTutorialViewModel>>
            {
                Success = true,
                Data = result
            });
        }

        /// <summary>
        /// Retrieves a specific video tutorial by its identifier.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the video tutorial.
        /// </param>
        /// <returns>
        /// The video tutorial details.
        /// </returns>
        [HttpGet("GetVideoTutorialById")]
        public IActionResult GetVideoTutorialById(int id)
        {
            var result = _downloadCenterService.GetVideoTutorialById(id);

            return Ok(new ApiResponse<VideoTutorialViewModel>
            {
                Success = result != null,
                Data = result,
                Message = result != null ? "Record found." : "Record not found."
            });
        }
        /// <summary>
        /// Creates a new video tutorial or updates an existing video tutorial.
        /// </summary>
        /// <param name="request">
        /// The video tutorial information to save.
        /// </param>
        /// <returns>
        /// A response indicating whether the operation was successful.
        /// </returns>
        [HttpPost("UpsertVideoTutorial")]
        public IActionResult UpsertVideoTutorial(VideoTutorialUpsertRequest request)
        {
            var result = _downloadCenterService.UpsertVideoTutorial(
                request,
                GetCompanyId(),
                GetUserId());

            return Ok(new ApiResponse<bool>
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Success
            });
        }

        /// <summary>
        /// Deletes a video tutorial.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the video tutorial.
        /// </param>
        /// <returns>
        /// A response indicating whether the delete operation was successful.
        /// </returns>
        [HttpPost("DeleteVideoTutorial")]
        public IActionResult DeleteVideoTutorial(int id)
        {
            var result = _downloadCenterService.DeleteVideoTutorial(
                id,
                GetUserId());

            return Ok(new ApiResponse<bool>
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Success
            });
        }

        /// <summary>
        /// Changes the active status of a video tutorial.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the video tutorial.
        /// </param>
        /// <returns>
        /// A response indicating whether the status change was successful.
        /// </returns>
        [HttpPost("ToggleVideoTutorialStatus")]
        public IActionResult ToggleVideoTutorialStatus(int id)
        {
            var result = _downloadCenterService.ToggleVideoTutorialStatus(
                id,
                GetUserId(),
                GetCompanyId());

            return Ok(new ApiResponse<bool>
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Success
            });
        }
        /// <summary>
        /// Retrieves uploaded content records.
        /// </summary>
        /// <param name="searchTerm">
        /// Optional text used to filter content records.
        /// </param>
        /// <returns>
        /// A collection of uploaded content records.
        /// </returns>
        [HttpGet("GetContentList")]
        public IActionResult GetContentList(string? searchTerm)
        {
            var result = _downloadCenterService.GetContentList(
                GetCompanyId(),
                searchTerm);

            return Ok(new ApiResponse<List<ContentViewModel>>
            {
                Success = true,
                Data = result
            });
        }

        /// <summary>
        /// Deletes a downloadable content record.
        /// </summary>
        /// <param name="id">
        /// The content identifier.
        /// </param>
        /// <returns>
        /// A response indicating whether the delete operation succeeded.
        /// </returns>
        [HttpPost("DeleteContent")]
        public IActionResult DeleteContent(int id)
        {
            var result = _downloadCenterService.DeleteContent(
                id,
                GetUserId());

            return Ok(new ApiResponse<bool>
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Success
            });
        }

        /// <summary>
        /// Generates a shareable link for the selected downloadable content items.
        /// The generated link can be shared with users to access the selected content.
        /// </summary>
        /// <param name="request">
        /// The shared link information including title, dates, and content identifiers.
        /// </param>
        /// <returns>
        /// A response containing the generated share token.
        /// </returns>
        [HttpPost("GenerateSharedLink")]
        public IActionResult GenerateSharedLink(SharedLinkUpsertRequest request)
        {
            var result = _downloadCenterService.GenerateSharedLink(
                request,
                GetCompanyId(),
                GetUserId());

            return Ok(new ApiResponse<string>
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Token
            });
        }

        /// <summary>
        /// Retrieves all shared links created for downloadable content.
        /// </summary>
        /// <returns>
        /// A collection of shared link records.
        /// </returns>
        [HttpGet("GetSharedLinkList")]
        public IActionResult GetSharedLinkList()
        {
            var result = _downloadCenterService.GetSharedLinkList(GetCompanyId());

            return Ok(new ApiResponse<List<SharedLinkViewModel>>
            {
                Success = true,
                Data = result
            });
        }

        /// <summary>
        /// Deletes an existing shared link.
        /// </summary>
        /// <param name="id">
        /// The shared link identifier.
        /// </param>
        /// <returns>
        /// A response indicating whether the delete operation succeeded.
        /// </returns>
        [HttpPost("DeleteSharedLink")]
        public IActionResult DeleteSharedLink(List<int> id)
        {
            var result = _downloadCenterService.DeleteSharedLink(id, GetCompanyId());

            return Ok(new ApiResponse<bool>
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Success
            });
        }

        /// <summary>
        /// Saves uploaded download center content.
        /// </summary>
        /// <param name="title">Content title.</param>
        /// <param name="typeId">Content type identifier.</param>
        /// <param name="fileType">File type.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="filePath">File path.</param>
        /// <param name="fileSize">File size.</param>
        /// <returns>Operation result.</returns>
        [HttpPost]
        [Route("SaveUploadContent")]
        public IActionResult SaveUploadContent(
            string title,
            int typeId,
            string fileType,
            string fileName,
            string filePath,
            string fileSize)
        {
            try
            {
                int companyId = Convert.ToInt32(
                    User.FindFirst("CompanyID")?.Value ?? "0");

                int userId = Convert.ToInt32(
                    User.FindFirst("UserID")?.Value ?? "0");

                var result = _downloadCenterService.SaveUploadContent(
                    title,
                    typeId,
                    fileType,
                    fileName,
                    filePath,
                    fileSize,
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
    }
}
