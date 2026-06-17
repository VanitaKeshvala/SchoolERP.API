using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Interfaces;
using SchoolERP.API.Models;
using SchoolERP.API.Models.Common;

namespace SchoolERP.API.Controllers
{
    /// <summary>
    /// API controller responsible for user authentication operations
    /// such as login and token generation.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly IAuthServices _authService;

        public AuthController(IAuthServices service)
        {
            _authService = service;
        }

        /// <summary>
        /// Authenticates a user using the provided username and password.
        /// If authentication is successful, user session information and
        /// authentication token are returned.
        /// </summary>
        /// <param name="request">
        /// Login request containing username and password.
        /// </param>
        /// <returns>
        /// Returns user session details on successful authentication;
        /// otherwise returns an error response.
        /// </returns>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(
                    ApiResponse<UserSessionModel>
                    .ErrorResponse("Invalid request parameters"));
            }

            var result = await _authService.LoginAsync(
                request.Username,
                request.Password);

            if (result.Success)
                return Ok(result);

            return Unauthorized(result);
        }
    }
}
