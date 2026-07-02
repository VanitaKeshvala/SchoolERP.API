using Microsoft.AspNetCore.Mvc;
using SchoolERP.Net.Services.Clients;
using SchoolERP.Shared.Models;
using SchoolERP.Shared.Models.Common;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SchoolERP.Net.Controllers
{
    [Route("[controller]")]
    public class CopySessionController : Controller
    {
        private readonly ICopySessionServices _copySession;
        private readonly ISubjectClientService _subjectClient;
        private readonly IClassClientService _classClient;
        private readonly IStudentInformationClientService _studentService;
        private readonly IHostelTypeClientService _hostelTypeService;
        private readonly IHostelClientService _hostelclient;
        private readonly IHolidayTypeClientService _holidayTypeclient;
        private readonly IHolidayClientService _holidayclient;
        private readonly ICountryClientService _countryclient;
        private readonly IStateClientService _statelient;
        public CopySessionController(
            ICopySessionServices copySession, ISubjectClientService subjectClient, IClassClientService classClient,
            IStudentInformationClientService studentService, IHostelClientService hostelclient, 
            IHostelTypeClientService hostelTypeService, IHolidayTypeClientService holidayTypeclient,
            IHolidayClientService holidayclient, ICountryClientService countryclient, IStateClientService statelient)
        {
            _copySession = copySession;
            _subjectClient = subjectClient;
            _classClient = classClient;
            _studentService = studentService;
            _hostelclient = hostelclient;
            _hostelTypeService = hostelTypeService;
            _holidayTypeclient = holidayTypeclient;
            _holidayclient = holidayclient;
            _countryclient = countryclient;
            _statelient = statelient;
        }
        private int GetUserId() => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("UserId")?.Value, out var id) ? id : 0;
        
        [HttpGet("Students")]
        public async Task<IActionResult> GetStudentList(int sessionId, int companyId)
        {
            var data = await _copySession.GetStudentListAsync(
                companyId,
                sessionId);

            return Ok(new ApiResponse<List<StudentListViewModel>>
            {
                Success = true,
                Data = data.Data
            });
        }

        // ── COPY STUDENTS TO SESSION ──────────────────────────────────────
        /// <summary>
        /// all students from one session to another.
        /// POST /CopySession/CopyStudents
        /// </summary>
        [HttpPost("CopyStudents")]
        public async Task<IActionResult> CopyStudents([FromBody] CopyRequest request)
        {
            try
            {
                request.UserID = GetUserId();
                // ── Execute copy ──────────────────────────────────────────
                var result = await _copySession.CopyStudentAsync(request);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message
                    });
                }

                return Ok(new
                {
                    success = false,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "An unexpected error occurred." });
            }
        }

        [HttpGet("Subject")]
        public async Task<IActionResult> GetSubjectList(int sessionId, int companyId)
        {
            try
            {

                var request = new SubjectSearchRequest
                {
                    PageNumber = 1,
                    PageSize = 10,
                    SearchKeyword = null,
                    CompanyID = companyId,
                    SessionID = sessionId
                };
                var data = await _subjectClient.GetAllAsync(request);

                return Ok(new ApiResponse<List<MstSubjectViewModel>>
                {
                    Success = true,
                    Data = data.Data.Data
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<List<MstSubjectViewModel>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            
        }


        // ── COPY Subject TO SESSION ──────────────────────────────────────
        /// <summary>
        /// all Subject from one session to another.
        /// POST /CopySession/CopySubject
        /// </summary>
        [HttpPost("CopySubject")]
        public async Task<IActionResult> CopySubject([FromBody] CopyRequest request)
        {
            try
            {
                request.UserID = GetUserId();
                // ── Execute copy ──────────────────────────────────────────
                var result = await _copySession.CopySubjectAsync(request);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message
                    });
                }

                return Ok(new
                {
                    success = false,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "An unexpected error occurred." });
            }
        }

        [HttpGet("Classs")]
        public async Task<IActionResult> GetClassList(int sessionId, int companyId)
        {
            var data = await _classClient.GetAllAsync(
                false,
                sessionId);

            return Ok(new ApiResponse<List<MstClassViewModel>>
            {
                Success = true,
                Data = data.Data
            });
        }

        // ── COPY Class TO SESSION ──────────────────────────────────────
        /// <summary>
        /// all Class from one session to another.
        /// POST /CopySession/CopyClass
        /// </summary>
        [HttpPost("CopyClasss")]
        public async Task<IActionResult> CopyClasss([FromBody] CopyRequest request)
        {
            try
            {
                request.UserID = GetUserId();
                // ── Execute copy ──────────────────────────────────────────
                var result = await _copySession.CopyClasssAsync(request);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message
                    });
                }

                return Ok(new
                {
                    success = false,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "An unexpected error occurred." });
            }
        }

        [HttpGet("StudentHouse")]
        public async Task<IActionResult> GetStudentHouseList(int sessionId, int companyId)
        {
            var data = await _studentService.GetAllStudentHouses(sessionId);

            return Ok(new ApiResponse<List<StudentHouseViewModel>>
            {
                Success = true,
                Data = data.Data
            });
        }

        // ── COPY Student House TO SESSION ──────────────────────────────────────
        /// <summary>
        /// all Student House from one session to another.
        /// POST /CopySession/CopyStudentHouse
        /// </summary>
        [HttpPost("CopyStudentHouse")]
        public async Task<IActionResult> CopyStudentHouse([FromBody] CopyRequest request)
        {
            try
            {
                request.UserID = GetUserId();
                // ── Execute copy ──────────────────────────────────────────
                var result = await _copySession.CopyStudentHouseAsync(request);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message
                    });
                }

                return Ok(new
                {
                    success = false,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "An unexpected error occurred." });
            }
        }


        [HttpGet("RoomTypes")]
        public async Task<IActionResult> GetRoomTypeList(int sessionId, int companyId)
        {
            var data = await _hostelclient.GetAllRoomTypesAsync(false,sessionId, companyId);

            return Ok(new ApiResponse<List<RoomTypeViewModel>>
            {
                Success = true,
                Data = data.Data
            });
        }


        // ── COPY Room Type TO SESSION ──────────────────────────────────────
        /// <summary>
        /// all Room Type from one session to another.
        /// POST /CopySession/CopyRoomTypes
        /// </summary>
        [HttpPost("CopyRoomTypes")]
        public async Task<IActionResult> RoomTypes([FromBody] CopyRequest request)
        {
            try
            {
                request.UserID = GetUserId();
                // ── Execute copy ──────────────────────────────────────────
                var result = await _copySession.CopyRoomTypesAsync(request);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message
                    });
                }

                return Ok(new
                {
                    success = false,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "An unexpected error occurred." });
            }
        }

        [HttpGet("Hostels")]
        public async Task<IActionResult> GetHostelsList(int sessionId, int companyId)
        {
            try
            {
                var data = await _hostelclient.GetAllHostelsAsync(false, sessionId, companyId);

                return Ok(new ApiResponse<List<HostelViewModel>>
                {
                    Success = true,
                    Data = data.Data
                });
            }
            catch (Exception ex)
            {

                throw;
            }            
        }

        // ── COPY Hostel TO SESSION ──────────────────────────────────────
        /// <summary>
        /// all Hostel Type from one session to another.
        /// POST /CopySession/CopyHostels
        /// </summary>
        [HttpPost("CopyHostels")]
        public async Task<IActionResult> CopyHostels([FromBody] CopyRequest request)
        {
            try
            {
                request.UserID = GetUserId();
                // ── Execute copy ──────────────────────────────────────────
                var result = await _copySession.CopyHostelsAsync(request);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message
                    });
                }

                return Ok(new
                {
                    success = false,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "An unexpected error occurred." });
            }
        }

        [HttpGet("HostelType")]
        public async Task<IActionResult> GetHostelType(int sessionId, int companyId)
        {
            var data = await _hostelTypeService.GetAllAsync(
                companyId,
                sessionId);

            return Ok(new ApiResponse<List<HostelTypeModel>>
            {
                Success = true,
                Data = data.Data
            });
        }

        // ── COPY Hostel TO SESSION ──────────────────────────────────────
        /// <summary>
        /// all Hostel Type from one session to another.
        /// POST /CopySession/CopyHostels
        /// </summary>
        [HttpPost("CopyHostelType")]
        public async Task<IActionResult> CopyHostelType([FromBody] CopyRequest request)
        {
            try
            {
                request.UserID = GetUserId();
                // ── Execute copy ──────────────────────────────────────────
                var result = await _copySession.CopyHostelTypeAsync(request);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message
                    });
                }

                return Ok(new
                {
                    success = false,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "An unexpected error occurred." });
            }
        }

        [HttpGet("HolidayType")]
        public async Task<IActionResult> GetHolidayType(int sessionId, int companyId)
        {
            var data = await _holidayTypeclient.GetAllAsync(
                companyId,
                sessionId);

            return Ok(new ApiResponse<List<HolidayType>>
            {
                Success = true,
                Data = data.Data
            });
        }

        // ── COPY Hostel TO SESSION ──────────────────────────────────────
        /// <summary>
        /// all Hostel Type from one session to another.
        /// POST /CopySession/CopyHostels
        /// </summary>
        [HttpPost("CopyHolidayType")]
        public async Task<IActionResult> CopyHolidayType([FromBody] CopyRequest request)
        {
            try
            {
                request.UserID = GetUserId();
                // ── Execute copy ──────────────────────────────────────────
                var result = await _copySession.CopyHolidayTypeAsync(request);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message
                    });
                }

                return Ok(new
                {
                    success = false,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "An unexpected error occurred." });
            }
        }

        [HttpGet("Holidays")]
        public async Task<IActionResult> GetHolidays(int sessionId, int companyId)
        {
            var data = await _holidayclient.GetAllAsync(
                companyId,
                sessionId);

            return Ok(new ApiResponse<List<HolidayModel>>
            {
                Success = true,
                Data = data.Data
            });
        }

        // ── COPY Hostel TO SESSION ──────────────────────────────────────
        /// <summary>
        /// all Hostel Type from one session to another.
        /// POST /CopySession/CopyHostels
        /// </summary>
        [HttpPost("CopyHolidays")]
        public async Task<IActionResult> CopyHolidays([FromBody] CopyRequest request)
        {
            try
            {
                request.UserID = GetUserId();
                // ── Execute copy ──────────────────────────────────────────
                var result = await _copySession.CopyHolidaysAsync(request);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message
                    });
                }

                return Ok(new
                {
                    success = false,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "An unexpected error occurred." });
            }
        }

        [HttpGet("Countrys")]
        public async Task<IActionResult> GetCountrys(int sessionId, int companyId)
        {
            var data = await _countryclient.GetAllAsync(
                companyId,
                sessionId);

            return Ok(new ApiResponse<List<CountryModel>>
            {
                Success = true,
                Data = data.Data
            });
        }

        // ── COPY Country TO SESSION ──────────────────────────────────────
        /// <summary>
        /// all Country Type from one session to another.
        /// POST /CopySession/CopyCountrys
        /// </summary>
        [HttpPost("CopyCountrys")]
        public async Task<IActionResult> CopyCountrys([FromBody] CopyRequest request)
        {
            try
            {
                request.UserID = GetUserId();
                // ── Execute copy ──────────────────────────────────────────
                var result = await _copySession.CopyCountrysAsync(request);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message
                    });
                }

                return Ok(new
                {
                    success = false,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "An unexpected error occurred." });
            }
        }

        [HttpGet("States")]
        public async Task<IActionResult> GetStates(int sessionId, int companyId)
        {
            var data = await _statelient.GetAllAsync(
                companyId,
                sessionId);

            return Ok(new ApiResponse<List<StateModel>>
            {
                Success = true,
                Data = data.Data
            });
        }

        // ── COPY Country TO SESSION ──────────────────────────────────────
        /// <summary>
        /// all Country Type from one session to another.
        /// POST /CopySession/CopyCountrys
        /// </summary>
        [HttpPost("CopyStates")]
        public async Task<IActionResult> CopyStates([FromBody] CopyRequest request)
        {
            try
            {
                request.UserID = GetUserId();
                // ── Execute copy ──────────────────────────────────────────
                var result = await _copySession.CopyStatesAsync(request);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message
                    });
                }

                return Ok(new
                {
                    success = false,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "An unexpected error occurred." });
            }
        }
    }
}
