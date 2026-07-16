using SchoolERP.Shared.Models;

namespace SchoolERP.Net.Services.Clients
{
    public interface IPhotoUploadService
    {
        /// <summary>
        /// Saves photo to wwwroot/{Module}/Profile/{recordId}/{recordId}.ext
        /// Returns relative URL for DB storage.
        /// </summary>
        Task<PhotoUploadResult> UploadAsync(IFormFile photo, PhotoModule module, FolderNameModule folderNameModule, int recordId);

        /// <summary>
        /// Deletes all photos for a specific record (e.g. before re-upload).
        /// </summary>
        void DeleteExisting(PhotoModule module, FolderNameModule folderNameModule, int recordId);

        /// <summary>
        /// Builds the folder path for a module/record combination.
        /// </summary>
        string GetFolderPath(PhotoModule module, FolderNameModule folderNameModule, int recordId);

        /// <summary>
        /// Builds the relative URL from module/recordId/filename.
        /// </summary>
        string GetRelativeUrl(PhotoModule module, FolderNameModule folderNameModule, int recordId, string fileName);


        // ─────────────────────────────────────────────────────────
        // POST  /UploadProfilePhoto
        // ─────────────────────────────────────────────────────────
        // Called from JS AFTER the main record is saved.
        // JS already sends: photo (file), recordId (int), module (string)
        // No JS or HTML changes needed — just wire this endpoint.
        //
        // Example JS call (already in your pages via fileToBase64 pattern):
        //   formData.append('photo',    photoFile);
        //   formData.append('recordId', savedId);
        //   formData.append('module',   'Staff');   // Staff/Student/Employee/User
        //   fetch('/UploadProfilePhoto', { method: 'POST', body: formData })
        // ─────────────────────────────────────────────────────────
        Task<PhotoUploadResult> SaveBase64PhotoAsync(
           string base64String,
           string originalFileName,
           PhotoModule module,
           FolderNameModule folderNameModule,
           int recordId);
    }
}
