
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Services;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ImagesController : ControllerBase
    {
        private readonly AzureStorageService _storageService;
        private readonly RaceDayDbContext _context;

        public ImagesController(
            AzureStorageService storageService,
            RaceDayDbContext context)
        {
            _storageService = storageService;
            _context = context;
        }

        // UPLOAD PARTICIPANT PROFILE PICTURE
        [HttpPost("profile-picture")]
        public async Task<IActionResult> UploadProfilePicture(
            IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please select a profile picture.");

            if (!file.ContentType.StartsWith(
                "image/", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only image files are allowed.");

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("Image must not exceed 5 MB.");

            try
            {
                var claim = User.FindFirst(
                    ClaimTypes.NameIdentifier);

                if (claim == null || !int.TryParse(
                    claim.Value, out int userId))
                    return Unauthorized();

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                    return NotFound("User not found.");

                string imageUrl =
                    await _storageService.UploadFileAsync(
                        file, "profile-pictures");

                user.ProfilePictureUrl = imageUrl;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Profile picture uploaded successfully."
                });
            }
            catch (Exception)
            {
                return StatusCode(500,
                    "Profile picture upload failed.");
            }
        }

        // DISPLAY PARTICIPANT PROFILE PICTURE
        [HttpGet("profile-picture")]
        public async Task<IActionResult> GetProfilePicture()
        {
            try
            {
                var claim = User.FindFirst(
                    ClaimTypes.NameIdentifier);

                if (claim == null || !int.TryParse(
                    claim.Value, out int userId))
                    return Unauthorized();

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                    return NotFound("User not found.");

                if (string.IsNullOrEmpty(user.ProfilePictureUrl))
                    return NotFound("No profile picture uploaded.");

                var image = await _storageService.DownloadFileAsync(
                    user.ProfilePictureUrl);

                return File(image.Content, image.ContentType);
            }
            catch (Exception)
            {
                return StatusCode(500,
                    "Unable to retrieve profile picture.");
            }
        }

        // UPLOAD EVENT BANNER (ORGANISER ONLY)
        [HttpPost("event-banner")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UploadEventBanner(
            IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please select an event banner.");

            if (!file.ContentType.StartsWith(
                "image/", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only image files are allowed.");

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("Image must not exceed 5 MB.");

            try
            {
                string imageUrl =
                    await _storageService.UploadFileAsync(
                        file, "event-banners");

                return Ok(new
                {
                    message = "Event banner uploaded successfully.",
                    imageUrl
                });
            }
            catch (Exception)
            {
                return StatusCode(500,
                    "Event banner upload failed.");
            }
        }

        // DISPLAY EVENT BANNER (PARTICIPANT ONLY)
        [HttpGet("event-banner/{eventId}")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetEventBanner(int eventId)
        {
            try
            {
                var raceEvent = await _context.Events
                    .FirstOrDefaultAsync(e => e.EventId == eventId);

                if (raceEvent == null)
                    return NotFound("Event not found.");

                if (string.IsNullOrEmpty(raceEvent.EventBannerUrl))
                    return NotFound("No event banner uploaded.");

                var image = await _storageService.DownloadFileAsync(
                    raceEvent.EventBannerUrl);

                return File(image.Content, image.ContentType);
            }
            catch (Exception)
            {
                return StatusCode(500,
                    "Unable to retrieve event banner.");
            }
        }
    }
}
