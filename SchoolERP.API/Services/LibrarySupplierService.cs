using Dapper;
using Microsoft.Data.SqlClient;
using SchoolERP.API.Interfaces;
using SchoolERP.Shared.Models.Common;
using SchoolERP.Shared.Models;
using System.Data;

namespace SchoolERP.API.Services
{
    public class LibrarySupplierService:ILibrarySupplierService
    {
        private readonly IConfiguration _configuration;
        public LibrarySupplierService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<ApiResponse> UpsertLibrarySupplierAsync(LibrarySupplierRequest model)
        {
            var response = new ApiResponse();

            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var parameters = new DynamicParameters();

                parameters.Add("@SupplierId", model.SupplierId);
                parameters.Add("@COMPANYID", model.CompanyID);
                parameters.Add("@SupplierName", model.SupplierName);
                parameters.Add("@Phone", model.Phone);
                parameters.Add("@Email", model.Email);
                parameters.Add("@Address", model.Address);
                parameters.Add("@PostalCode", model.PostalCode);
                parameters.Add("@CountryId", model.CountryId);
                parameters.Add("@StateId", model.StateId);
                parameters.Add("@CityId", model.CityId);
                parameters.Add("@FAX", model.FAX);
                parameters.Add("@WebSite", model.WebSite);
                parameters.Add("@MobileNo", model.MobileNo);
                parameters.Add("@ContactPerson", model.ContactPerson);
                parameters.Add("@ISACTIVE", model.IsActive);
                parameters.Add("@USERID", model.UserID);
                parameters.Add("@IPADDRESS", model.IPAddress);

                var result = await conn.QueryFirstOrDefaultAsync<SpResult>(
                    "SP_TBL_MST_LibrarySupplier_UPSERT",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                if (result != null)
                {
                    response.Result = result.Result;
                    response.Message = result.Message;
                    response.TechnicalMessage = result.TECHNICALMESSAGE;
                }
            }
            catch (Exception ex)
            {
                response.Result = 0;
                response.Message = "Unable to save hostel type. Please try again.";
                response.TechnicalMessage = ex.Message;
            }

            return response;
        }

        public async Task<LibrarySupplierModel?> GetLibrarySupplierByIdAsync(int supplierId)
        {
            try
            {
                using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

                var parameters = new DynamicParameters();
                parameters.Add("@SupplierId", supplierId);

                return await conn.QueryFirstOrDefaultAsync<LibrarySupplierModel>(
                    "SP_TBL_MST_LibrarySupplier_GETBYID",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public async Task<PagedResult<LibrarySupplierModel>> GetAllLibrarySupplierWithPage(SearchRequest req)
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                var param = new DynamicParameters();
                if (req.PageNumber == 0 && req.PageSize == 0)
                {
                    req.PageNumber = 1;
                    req.PageSize = 10;
                }


                param.Add("@COMPANYID", req.CompanyID);
                param.Add("@SEARCHKEYWORD", req.SearchKeyword);
                param.Add("@PAGENUMBER", req.PageNumber);
                param.Add("@PAGESIZE", req.PageSize);
                param.Add("@INCLUDEDELETED", 0);

                var result = (await conn.QueryAsync<LibrarySupplierModel>(
                "SP_TBL_MST_LibrarySupplier_GETALLWITHPAGEINDEX",
                param,
                commandType: CommandType.StoredProcedure)).ToList();


                int res = result.FirstOrDefault()?.Result ?? 0;
                int totalRecords = result.FirstOrDefault()?.TOTALRECORDS ?? 0;
                int pageIndex = result.FirstOrDefault()?.CURRENTPAGE ?? 0;
                int pageSize = result.FirstOrDefault()?.PageSize ?? 0;

                var userModel = new PagedResult<LibrarySupplierModel>
                {
                    Data = result,
                    TotalRecords = totalRecords,
                    PageNumber = pageIndex,
                    PageSize = pageSize
                };

                if (res == 0)
                {
                    userModel = new PagedResult<LibrarySupplierModel>
                    {
                        Data = null,
                        TotalRecords = totalRecords,
                        PageNumber = pageIndex,
                        PageSize = pageSize
                    };
                }
                return userModel;

            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public (bool success, string message) DeleteLibrarySupplier(List<int> ids, int userId)
        {
            try
            {
                if (ids == null || !ids.Any())
                {
                    return (false, "No library budget selected for deletion.");
                }

                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                string supplierId = string.Join(",", ids);

                var parameters = new DynamicParameters();
                parameters.Add("@SUPPLIERID", supplierId);
                parameters.Add("@USERID", userId);
                var result = conn.QueryFirstOrDefault<SpResult>(
                   "SP_MST_LibrarySupplier_DELETE",
                   parameters,
                   commandType: CommandType.StoredProcedure);

                return (
                    result?.Result == 1,
                    result?.Message ?? "Operation completed."
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool success, string message) ToggleLibrarySupplierStatus(StatusUpdateRequest request)
        {
            try
            {
                using var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var parameters = new DynamicParameters();
                parameters.Add("@SUPPLIERID", request.Ids);
                parameters.Add("@IsActive", request.IsActive);
                parameters.Add("@UserId", request.DoneBy);

                var result = conn.QueryFirstOrDefault<SpResult>(
                    "SP_MST_LibrarySupplier_TOGGLESTATUS",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return (
                    result?.Result == 1,
                    result?.Message ?? "Operation completed."
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
