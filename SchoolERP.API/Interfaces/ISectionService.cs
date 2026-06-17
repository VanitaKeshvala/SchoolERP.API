using SchoolERP.API.Models;

namespace SchoolERP.API.Interfaces
{
    /// <summary>
    /// Interface for managing class sections (e.g., Section A, Section B).
    /// </summary>
    public interface ISectionService
    {
        /// <summary>
        /// Retrieves a list of all sections for a specific school and academic session.
        /// </summary>
        List<MstSectionViewModel> GetAllSections(int companyId, int sessionId, bool includeDeleted = false);

        /// <summary>
        /// Gets all sections that are linked to a specific class.
        /// </summary>
        List<MstSectionViewModel> GetSectionsByClass(int classId);

        /// <summary>
        /// Finds and returns the details of a specific section using its unique ID.
        /// </summary>
        MstSectionViewModel? GetSectionByID(int sectionId);

        /// <summary>
        /// Adds a new section or updates an existing one in the database.
        /// </summary>
        (bool success, string message) UpsertSection(MstSectionUpsertRequest request, int companyId, int sessionId, int userId);

        /// <summary>
        /// Removes a section from the system.
        /// </summary>
        (bool success, string message) DeleteSection(List<int> sectionId, int userId);

        /// <summary>
        /// Turns a section's active status on or off.
        /// </summary>
        (bool success, string message) ToggleSectionStatus(int sectionId, bool isActive, int userId);
    }
}
