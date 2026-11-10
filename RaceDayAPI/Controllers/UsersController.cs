
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public UsersController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Users/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            int userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound("User not found.");

            return Ok(new
            {
                user.UserId,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role,
                user.ProfilePictureUrl
            });
        }

        // PUT: api/Users/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(
            [FromForm] string firstName,
            [FromForm] string lastName)
        {
            int userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound("User not found.");

            user.FirstName = firstName;
            user.LastName = lastName;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Profile updated successfully.",
                user.UserId,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role,
                user.ProfilePictureUrl
            });
        }
    }
}
