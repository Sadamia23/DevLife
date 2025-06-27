using DevLife.Database;
using DevLife.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace Developers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all users (requires authentication)
        /// </summary>
        /// <returns>List of all users</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserProfileDto>>> GetUsers()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new { message = "Authentication required" });
            }

            var users = await _context.Users
                .Select(u => new UserProfileDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    DateOfBirth = u.DateOfBirth,
                    TechStack = u.TechStack,
                    Experience = u.Experience,
                    ZodiacSign = u.ZodiacSign,
                    Age = DateTime.Now.Year - u.DateOfBirth.Year,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        /// <param name="username">Username to search for</param>
        /// <returns>User profile</returns>
        [HttpGet("{username}")]
        public async Task<ActionResult<UserProfileDto>> GetUser(string username)
        {
            var currentUser = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(currentUser))
            {
                return Unauthorized(new { message = "Authentication required" });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var age = DateTime.Now.Year - user.DateOfBirth.Year;
            if (user.DateOfBirth.Date > DateTime.Now.AddYears(-age)) age--;

            var userProfile = new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateOfBirth = user.DateOfBirth,
                TechStack = user.TechStack,
                Experience = user.Experience,
                ZodiacSign = user.ZodiacSign,
                Age = age,
                CreatedAt = user.CreatedAt
            };

            return Ok(userProfile);
        }

        [HttpGet("debug/auth")]
        public IActionResult DebugAuth()
        {
            var username = HttpContext.Session.GetString("Username");
            var userId = HttpContext.Session.GetString("UserId");

            return Ok(new
            {
                HasSession = HttpContext.Session != null,
                SessionId = HttpContext.Session.Id,
                Username = username,
                UserId = userId,
                IsAuthenticated = !string.IsNullOrEmpty(username),
                SessionKeys = HttpContext.Session.Keys.ToList(),
                Headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                Cookies = Request.Cookies.ToDictionary(c => c.Key, c => c.Value)
            });
        }
    }
}
