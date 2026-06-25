using SchoolERP.Shared.Models;
namespace SchoolERP.Net.Services.Clients
{
    public class PhotoUploadService :BaseApiClient, IPhotoUploadService
    {
        private readonly IWebHostEnvironment _env;
        
        // Allowed image types
        private static readonly string[] AllowedTypes = {
            "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp"
        };

        // Max file size: 2MB
        private const long MaxFileSizeBytes = 2 * 1024 * 1024;

        public PhotoUploadService(IWebHostEnvironment env, HttpClient httpClient) : base(httpClient)
        {
            _env = env;
        }

        // ── UPLOAD ────────────────────────────────────────────────
        public async Task<PhotoUploadResult> UploadAsync(
            IFormFile photo, PhotoModule module, int recordId)
        {
            // ── Validate: file present ─────────────────────────
            if (photo == null || photo.Length == 0)
                return Fail("No photo file provided.");

            // ── Validate: file type ────────────────────────────
            if (!AllowedTypes.Contains(photo.ContentType.ToLower()))
                return Fail("Only JPG, PNG, GIF, and WEBP images are allowed.");

            // ── Validate: file size ────────────────────────────
            if (photo.Length > MaxFileSizeBytes)
                return Fail("Photo size must not exceed 2MB.");

            // ── Validate: recordId ─────────────────────────────
            if (recordId <= 0)
                return Fail("Invalid record ID. Save the record first before uploading photo.");

            try
            {
                // ── Build folder path ──────────────────────────
                // e.g. wwwroot/Staff/Profile/101/
                var folderPath = GetFolderPath(module, recordId);

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

               
                // ── Build file name: {recordId}.ext ───────────
                // e.g.  101.png  →  clean, predictable, no collision
                var ext = Path.GetExtension(photo.FileName).ToLower();
                //var fileName = $"{recordId}{ext}";
                
                var existingFiles = Directory.GetFiles(folderPath, $"{recordId}_V*.*");
                int nextVersion = 1;
                if (existingFiles.Any())
                {
                    nextVersion = existingFiles
                        .Select(f =>
                        {
                            var name = Path.GetFileNameWithoutExtension(f);

                            // 17_V3
                            var parts = name.Split("_V");

                            return parts.Length > 1 &&
                                   int.TryParse(parts[1], out int v)
                                   ? v
                                   : 0;
                        })
                        .Max() + 1;
                }
                var fileName = $"{recordId}_V{nextVersion}{ext}";
                var fullPath = Path.Combine(folderPath, fileName);
                // ── Save to disk ───────────────────────────────
                using var stream = new FileStream(fullPath, FileMode.Create);
                await photo.CopyToAsync(stream);

                // ── Return relative URL ────────────────────────
                var relativeUrl = GetRelativeUrl(module, recordId, fileName);

                return new PhotoUploadResult
                {
                    Success = true,
                    Message = "Photo uploaded successfully.",
                    PhotoUrl = relativeUrl,
                    FileName = fileName
                };
            }
            catch (Exception ex)
            {
                return Fail($"Upload failed: {ex.Message}");
            }
        }

        // ── DELETE EXISTING ───────────────────────────────────────
        public void DeleteExisting(PhotoModule module, int recordId)
        {
            var folderPath = GetFolderPath(module, recordId);
            if (!Directory.Exists(folderPath)) return;

            foreach (var file in Directory.GetFiles(folderPath))
            {
                try { System.IO.File.Delete(file); }
                catch { /* ignore individual delete failures */ }
            }
        }

        // ── GET FOLDER PATH ───────────────────────────────────────
        // e.g. wwwroot/Staff/Profile/101/
        public string GetFolderPath(PhotoModule module, int recordId)
        {
            return Path.Combine(
                _env.WebRootPath,
                module.ToString(),   // Staff / Student / Employee / User
                "Profile",
                recordId.ToString()
            );
        }

        // ── GET RELATIVE URL ──────────────────────────────────────
        // e.g. /Staff/Profile/101/101.png
        public string GetRelativeUrl(PhotoModule module, int recordId, string fileName)
        {
            return $"/{module}/Profile/{recordId}/{fileName}";
        }

        // ── PRIVATE HELPERS ───────────────────────────────────────
        private static PhotoUploadResult Fail(string message) => new()
        {
            Success = false,
            Message = message,
            PhotoUrl = string.Empty
        };
    }
}

