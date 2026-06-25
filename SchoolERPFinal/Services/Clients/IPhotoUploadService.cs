using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public interface IPhotoUploadService
    {
        /// <summary>
        /// Saves photo to wwwroot/{Module}/Profile/{recordId}/{recordId}.ext
        /// Returns relative URL for DB storage.
        /// </summary>
        Task<PhotoUploadResult> UploadAsync(IFormFile photo, PhotoModule module, int recordId);

        /// <summary>
        /// Deletes all photos for a specific record (e.g. before re-upload).
        /// </summary>
        void DeleteExisting(PhotoModule module, int recordId);

        /// <summary>
        /// Builds the folder path for a module/record combination.
        /// </summary>
        string GetFolderPath(PhotoModule module, int recordId);

        /// <summary>
        /// Builds the relative URL from module/recordId/filename.
        /// </summary>
        string GetRelativeUrl(PhotoModule module, int recordId, string fileName);
    }
}
