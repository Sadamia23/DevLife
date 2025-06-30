using DevLife.Dtos;
using DevLife.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DevLife.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
     private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Invalid data provided"
                });
            }

            var result = await _authService.RegisterAsync(registerDto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            HttpContext.Session.SetString("Username", registerDto.Username);
            HttpContext.Session.SetString("UserId", result.User!.Id.ToString());

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Username is required"
                });
            }

            var result = await _authService.LoginAsync(loginDto);

            if (!result.Success)
            {
                return NotFound(result);
            }

            HttpContext.Session.SetString("Username", loginDto.Username);
            HttpContext.Session.SetString("UserId", result.User!.Id.ToString());

            return Ok(result);
        }

        [HttpGet("profile")]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new { message = "User not logged in" });
            }

            var userProfile = await _authService.GetUserProfileAsync(username);

            if (userProfile == null)
            {
                return NotFound(new { message = "User profile not found" });
            }

            return Ok(userProfile);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpGet("status")]
        public IActionResult GetAuthStatus()
        {
            var username = HttpContext.Session.GetString("Username");
            var isAuthenticated = !string.IsNullOrEmpty(username);

            return Ok(new
            {
                isAuthenticated,
                username = isAuthenticated ? username : null
            });
        }

        [HttpGet("zodiac")]
        public IActionResult GetZodiacSign([FromQuery] DateTime date)
        {
            var zodiacSign = _authService.CalculateZodiacSign(date);
            return Ok(new { date, zodiacSign });
        }
    }
}
