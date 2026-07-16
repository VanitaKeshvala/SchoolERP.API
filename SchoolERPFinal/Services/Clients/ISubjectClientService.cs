using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    public interface ISubjectClientService
    {
        Task<ApiResponse<PagedResult<MstSubjectViewModel>>> GetAllAsync(SubjectSearchRequest request);
        Task<ApiResponse<MstSubjectViewModel>> GetByIDAsync(int id);
        Task<ApiResponse<dynamic>> UpsertAsync(MstSubjectUpsertRequest request);
        Task<ApiResponse<dynamic>> DeleteAsync(List<int> ids);
        Task<ApiResponse<dynamic>> ToggleStatusAsync(StatusUpdateRequest request);
        Task<ApiResponse<List<Dropdowbinding>>> SubjectsDropdowBindAsync(DropdowRequest request);
        Task<ApiResponse<List<DropdownModel>>> GetSubjectGropBySubjectDropdownList(int subjectGroupId);
    }
}
