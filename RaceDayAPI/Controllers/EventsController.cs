
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using RaceDayAPI.Data;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Events
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events
                .Include(e => e.Organiser)
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/Events/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var raceEvent = await _context.Events
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (raceEvent == null)
                return NotFound("Event not found.");

            return Ok(raceEvent);
        }

        // POST: api/Events
        [Authorize(Roles = "Organiser")]
        [HttpPost]
        public async Task<IActionResult> CreateEvent(
            [FromForm] Event raceEvent)
        {
            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userId, out int organiserId))
                return Unauthorized("Please log in as an organiser.");

            raceEvent.OrganiserId = organiserId;

            _context.Events.Add(raceEvent);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = raceEvent.EventId },
                raceEvent);
        }

        // PUT: api/Events/5
        [Authorize(Roles = "Organiser")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(
            int id,
            [FromBody] Event updatedEvent)
        {
            var existingEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (existingEvent == null)
                return NotFound("Event not found.");

            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userId, out int organiserId))
                return Unauthorized();

            if (existingEvent.OrganiserId != organiserId)
                return Forbid();

            existingEvent.EventName = updatedEvent.EventName;
            existingEvent.Description = updatedEvent.Description;
            existingEvent.EventDate = updatedEvent.EventDate;
            existingEvent.Location = updatedEvent.Location;
            existingEvent.DistanceKm = updatedEvent.DistanceKm;
            existingEvent.EventType = updatedEvent.EventType;

            // Save the Azure event banner URL
            existingEvent.EventBannerUrl =
                updatedEvent.EventBannerUrl;

            await _context.SaveChangesAsync();

            return Ok(existingEvent);
        }

        // DELETE: api/Events/5
        [Authorize(Roles = "Organiser")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var existingEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (existingEvent == null)
                return NotFound("Event not found.");

            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userId, out int organiserId))
                return Unauthorized();

            if (existingEvent.OrganiserId != organiserId)
                return Forbid();

            _context.Events.Remove(existingEvent);

            await _context.SaveChangesAsync();

            return Ok("Event deleted successfully.");
        }
    }
}
