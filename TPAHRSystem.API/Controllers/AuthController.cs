using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPAHRSystem.API.Utilities;
using TPAHRSystem.Application.Services;

namespace TPAHRSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet("debug-password")]
        public IActionResult DebugPassword()
        {
            PasswordDebugger.DebugPasswordHashing();
            return Ok("Check console output for debug results");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            //try
            //{
                _logger.LogInformation($"Login attempt for email: {request.Email}");

                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new { success = false, message = "Email and password are required" });
                }

                var result = await _authService.LoginAsync(request.Email, request.Password);

                if (result.Success)
                {
                    _logger.LogInformation($"Login successful for: {request.Email}");

                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        token = result.Token,
                        user = new
                        {
                            id = result.User!.Id,
                            email = result.User.Email,
                            role = result.User.Role
                        }
                    });
                }
                else
                {
                    _logger.LogWarning($"Login failed for: {request.Email} - {result.Message}");
                    return BadRequest(new { success = false, message = result.Message });
                }
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, $"Error during login for: {request.Email}");
            //    return BadRequest(new { success = false, message = "An error occurred during login" });
            //}
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            try
            {
                var result = await _authService.LogoutAsync(request.Token);
                return Ok(new { success = result, message = result ? "Logout successful" : "Invalid token" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return BadRequest(new { success = false, message = "An error occurred during logout" });
            }
        }

        [HttpGet("validate")]
        public async Task<IActionResult> ValidateToken([FromQuery] string token)
        {
            try
            {
                var user = await _authService.GetUserByTokenAsync(token);

                if (user != null)
                {
                    return Ok(new
                    {
                        success = true,
                        user = new
                        {
                            id = user.Id,
                            email = user.Email,
                            role = user.Role
                        }
                    });
                }
                else
                {
                    return Unauthorized(new { success = false, message = "Invalid or expired token" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return BadRequest(new { success = false, message = "An error occurred validating token" });
            }
        }
       


    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LogoutRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}