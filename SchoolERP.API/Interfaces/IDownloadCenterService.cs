using SchoolERP.API.Models;

namespace SchoolERP.API.Interfaces
{
    public interface IDownloadCenterService
    {
        List<ContentTypeViewModel> GetContentTypeList(int companyId, string? searchTerm);
        ContentTypeViewModel GetContentTypeById(int id);
        (bool Success, string Message) UpsertContentType(ContentTypeUpsertRequest req, int companyId, int userId);
        (bool Success, string Message) DeleteContentType(List<int> id, int userId);
        (bool Success, string Message) ToggleContentTypeStatus(int id, int userId, int companyId);

        // Video Tutorials
        List<VideoTutorialViewModel> GetVideoTutorialList(int companyId, int? classId, int? sectionId, string? searchTerm);
        VideoTutorialViewModel GetVideoTutorialById(int id);
        (bool Success, string Message) UpsertVideoTutorial(VideoTutorialUpsertRequest req, int companyId, int userId);
        (bool Success, string Message) DeleteVideoTutorial(int id, int userId);
        (bool Success, string Message) ToggleVideoTutorialStatus(int id, int userId, int companyId);

        // Upload Content
        List<ContentViewModel> GetContentList(int companyId, string? searchTerm);
        (bool Success, string Message) SaveContent(ContentViewModel content, int companyId, int userId);
        (bool Success, string Message) DeleteContent(int id, int userId);

        // Shared Links
        (bool Success, string Message, string Token) GenerateSharedLink(SharedLinkUpsertRequest req, int companyId, int userId);
        List<SharedLinkViewModel> GetSharedLinkList(int companyId);
        (bool Success, string Message) DeleteSharedLink(List<int> id,int companyId);
        (bool Success, string Message) SaveUploadContent(string title, int typeId, string fileType, string fileName, string filePath, string fileSize, int companyId, int userId);
    }
}
