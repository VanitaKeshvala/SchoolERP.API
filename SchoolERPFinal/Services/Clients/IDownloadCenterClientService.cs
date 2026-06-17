using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface IDownloadCenterClientService
    {
        /// <summary>
        /// Retrieves all content types.
        /// </summary>
        /// <param name="searchTerm">
        /// Optional text used to filter content types.
        /// </param>
        /// <returns>
        /// A collection of content type records.
        /// </returns>
        Task<ApiResponse<List<ContentTypeViewModel>>> GetContentTypeList(string? searchTerm = null);

        /// <summary>
        /// Retrieves a content type by its identifier.
        /// </summary>
        /// <param name="id">
        /// The content type identifier.
        /// </param>
        /// <returns>
        /// The requested content type details.
        /// </returns>
        Task<ApiResponse<ContentTypeViewModel>> GetContentTypeById(int id);

        /// <summary>
        /// Creates a new content type or updates an existing content type.
        /// </summary>
        /// <param name="request">
        /// The content type information to save.
        /// </param>
        /// <returns>
        /// A response indicating whether the operation succeeded.
        /// </returns>
        Task<ApiResponse<bool>> UpsertContentType(ContentTypeUpsertRequest request);

        /// <summary>
        /// Deletes the specified content type.
        /// </summary>
        /// <param name="id">
        /// The content type identifier.
        /// </param>
        /// <returns>
        /// A response indicating whether the operation succeeded.
        /// </returns>
        Task<ApiResponse<bool>> DeleteContentType(List<int> id);

        /// <summary>
        /// Changes the active status of a content type.
        /// </summary>
        /// <param name="id">
        /// The content type identifier.
        /// </param>
        /// <returns>
        /// A response indicating whether the operation succeeded.
        /// </returns>
        Task<ApiResponse<bool>> ToggleContentTypeStatus(int id);
        /// <summary>
        /// Retrieves video tutorials.
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
        Task<ApiResponse<List<VideoTutorialViewModel>>> GetVideoTutorialList(
            int? classId = null,
            int? sectionId = null,
            string? searchTerm = null);
        /// <summary>
        /// Retrieves a video tutorial by its identifier.
        /// </summary>
        /// <param name="id">
        /// The video tutorial identifier.
        /// </param>
        /// <returns>
        /// The requested video tutorial details.
        /// </returns>
        Task<ApiResponse<VideoTutorialViewModel>> GetVideoTutorialById(int id);
        /// <summary>
        /// Creates a new video tutorial or updates an existing video tutorial.
        /// </summary>
        /// <param name="request">
        /// The video tutorial details to save.
        /// </param>
        /// <returns>
        /// A response indicating whether the operation succeeded.
        /// </returns>
        Task<ApiResponse<bool>> UpsertVideoTutorial(VideoTutorialUpsertRequest request);
        /// <summary>
        /// Deletes a video tutorial.
        /// </summary>
        /// <param name="id">
        /// The video tutorial identifier.
        /// </param>
        /// <returns>
        /// A response indicating whether the operation succeeded.
        /// </returns>
        Task<ApiResponse<bool>> DeleteVideoTutorial(int id);
        /// <summary>
        /// Changes the active status of a video tutorial.
        /// </summary>
        /// <param name="id">
        /// The video tutorial identifier.
        /// </param>
        /// <returns>
        /// A response indicating whether the operation succeeded.
        /// </returns>
        Task<ApiResponse<bool>> ToggleVideoTutorialStatus(int id);
        /// <summary>
        /// Retrieves uploaded content records.
        /// </summary>
        /// <param name="searchTerm">
        /// Optional search text.
        /// </param>
        /// <returns>
        /// A collection of uploaded content records.
        /// </returns>
        Task<ApiResponse<List<ContentViewModel>>> GetContentList(string? searchTerm = null);

        /// <summary>
        /// Used to permanently remove downloadable content from the system.
        /// Typically accessed from the content management screen.
        /// </summary>
        Task<ApiResponse<bool>> DeleteContent(int id);
        /// <summary>
        /// Generates a shareable link for selected content.
        /// </summary>
        Task<ApiResponse<string>> GenerateSharedLink(SharedLinkUpsertRequest request);

        /// <summary>
        /// Retrieves all shared links.
        /// </summary>
        Task<ApiResponse<List<SharedLinkViewModel>>> GetSharedLinkList();

        /// <summary>
        /// Deletes a shared link.
        /// </summary>
        Task<ApiResponse<bool>> DeleteSharedLink(List<int> id);

        /// <summary>
        /// Saves uploaded content.
        /// </summary>
        Task<ApiResponse<dynamic>> SaveUploadContentAsync(
            string title,
            int typeId,
            string fileType,
            string fileName,
            string filePath,
            string fileSize);
    }
}
