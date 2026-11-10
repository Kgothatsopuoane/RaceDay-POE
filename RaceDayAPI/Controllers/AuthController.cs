
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;
using RaceDayAPI.Services;
using System.Security.Claims;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly PasswordService _passwordService;
        private readonly IWebHostEnvironment _environment;

        public AuthController(
            RaceDayDbContext context,
            PasswordService passwordService,
            IWebHostEnvironment environment)
        {
            _context = context;
            _passwordService = passwordService;
            _environment = environment;
        }

        // REGISTER
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromForm] RegisterDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.Role != UserRole.Organiser &&
                request.Role != UserRole.Participant)
            {
                return BadRequest(
                    "Role must be Organiser or Participant.");
            }

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == request.Email);

            if (emailExists)
                return BadRequest("Email already exists.");

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash =
                    _passwordService.HashPassword(request.Password),
                Role = request.Role.ToString()
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Registration successful.",
                userId = user.UserId,
                role = user.Role
            });
        }

        // LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromForm] LoginDto request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                return Unauthorized("Invalid email or password.");

            bool passwordValid =
                _passwordService.VerifyPassword(
                    request.Password,
                    user.PasswordHash);

            if (!passwordValid)
                return Unauthorized("Invalid email or password.");

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.UserId.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    user.Email),

                new Claim(
                    ClaimTypes.Role,
                    user.Role)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            return Ok(new
            {
                message = "Login successful.",
                userId = user.UserId,
                email = user.Email,
                role = user.Role
            });
        }

        // TEMPORARY PASSWORD RESET - DEVELOPMENT ONLY
        [HttpPost("reset-participant-password")]
        public async Task<IActionResult> ResetParticipantPassword(
            [FromForm] string email,
            [FromForm] string newPassword)
        {
            if (!_environment.IsDevelopment())
                return NotFound();

            if (string.IsNullOrWhiteSpace(newPassword) ||
                newPassword.Length < 8)
            {
                return BadRequest(
                    "Password must be at least 8 characters.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return NotFound("Participant account not found.");

            if (user.Role != "Participant")
                return BadRequest(
                    "This account is not a participant.");

            user.PasswordHash =
                _passwordService.HashPassword(newPassword);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Participant password reset successfully."
            });
        }

        // LOGOUT
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return Ok(new
            {
                message = "Logout successful."
            });
        }
    }
}
