using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public class CopySessionServices:BaseApiClient, ICopySessionServices
    {
        public CopySessionServices(HttpClient httpClient) : base(httpClient)
        {
        }
        
        public async Task<ApiResponse<List<StudentListViewModel>>> GetStudentListAsync(
        int companyId,
        int sessionId)
        {
            return await GetAsync<List<StudentListViewModel>>($"api/CopySessionAPI/Student?companyId={companyId}&sessionId={sessionId}");
        }

        public Task<ApiResponse<SpResult>> CopyStudentAsync(CopyRequest req)
            => PostAsync<SpResult>("api/CopySessionAPI/CopyStudents", req);

        public Task<ApiResponse<SpResult>> CopySubjectAsync(CopyRequest req)
            => PostAsync<SpResult>("api/CopySessionAPI/CopySubject", req);

        public Task<ApiResponse<SpResult>> CopyClasssAsync(CopyRequest req)
            => PostAsync<SpResult>("api/CopySessionAPI/CopyClasss", req);

        public Task<ApiResponse<SpResult>> CopyStudentHouseAsync(CopyRequest req)
            => PostAsync<SpResult>("api/CopySessionAPI/CopyStudentHouse", req);

        public Task<ApiResponse<SpResult>> CopyRoomTypesAsync(CopyRequest req)
            => PostAsync<SpResult>("api/CopySessionAPI/CopyRoomTypes", req);

        public Task<ApiResponse<SpResult>> CopyHostelsAsync(CopyRequest req)
            => PostAsync<SpResult>("api/CopySessionAPI/CopyHostels", req);

        public Task<ApiResponse<SpResult>> CopyHostelTypeAsync(CopyRequest req)
            => PostAsync<SpResult>("api/CopySessionAPI/CopyHostelType", req);
        public Task<ApiResponse<SpResult>> CopyHolidaysAsync(CopyRequest req)
           => PostAsync<SpResult>("api/CopySessionAPI/CopyHolidays", req);

        public Task<ApiResponse<SpResult>> CopyHolidayTypeAsync(CopyRequest req)
           => PostAsync<SpResult>("api/CopySessionAPI/CopyHolidayType", req);

        public Task<ApiResponse<SpResult>> CopyCountrysAsync(CopyRequest req)
          => PostAsync<SpResult>("api/CopySessionAPI/CopyCountrs", req);

        public Task<ApiResponse<SpResult>> CopyStatesAsync(CopyRequest req)
          => PostAsync<SpResult>("api/CopySessionAPI/CopyStates", req);
    }
}
