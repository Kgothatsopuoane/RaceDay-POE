using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Enrolments
        [Authorize(Roles = "Organiser")]
        [HttpGet]
        public async Task<IActionResult> GetEnrolments()
        {
            int organiserId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var enrolments = await _context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Where(e => e.Event!.OrganiserId == organiserId)
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: api/Enrolments/my
        [Authorize(Roles = "Participant")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            int participantId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var enrolments = await _context.Enrolments
                .Where(e => e.ParticipantId == participantId)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.ParticipantId,
                    e.EventId,
                    e.CategoryId,
                    e.EnrolmentDate,
                    EventName = e.Event!.EventName,
                    CategoryName = e.Category!.CategoryName
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: api/Enrolments/1
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEnrolment(int id)
        {
            var enrolment = await _context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.EnrolmentId == id);

            if (enrolment == null)
            {
                return NotFound("Enrolment not found.");
            }

            int userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            bool isOrganiser =
                User.IsInRole("Organiser") &&
                enrolment.Event?.OrganiserId == userId;

            bool isParticipant =
                User.IsInRole("Participant") &&
                enrolment.ParticipantId == userId;

            if (!isOrganiser && !isParticipant)
            {
                return Forbid();
            }

            return Ok(enrolment);
        }

        // POST: api/Enrolments
        [Authorize(Roles = "Participant")]
        [HttpPost]
        public async Task<IActionResult> CreateEnrolment(
            [FromForm] CreateEnrolmentDto request)
        {
            int participantId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == request.EventId);

            if (raceEvent == null)
            {
                return NotFound("Event not found.");
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.CategoryId == request.CategoryId &&
                    c.EventId == request.EventId);

            if (category == null)
            {
                return BadRequest(
                    "The category does not belong to this event."
                );
            }

            bool alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e =>
                    e.ParticipantId == participantId &&
                    e.EventId == request.EventId);

            if (alreadyEnrolled)
            {
                return BadRequest(
                    "You are already enrolled in this event."
                );
            }

            var enrolment = new Enrolment
            {
                ParticipantId = participantId,
                EventId = request.EventId,
                CategoryId = request.CategoryId,
                EnrolmentDate = DateTime.UtcNow
            };

            _context.Enrolments.Add(enrolment);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEnrolment),
                new { id = enrolment.EnrolmentId },
                new
                {
                    enrolment.EnrolmentId,
                    enrolment.ParticipantId,
                    enrolment.EventId,
                    enrolment.CategoryId,
                    enrolment.EnrolmentDate
                }
            );
        }

        // DELETE: api/Enrolments/1
        [Authorize(Roles = "Participant")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnrolment(int id)
        {
            int participantId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var enrolment = await _context.Enrolments
                .FirstOrDefaultAsync(e =>
                    e.EnrolmentId == id &&
                    e.ParticipantId == participantId);

            if (enrolment == null)
            {
                return NotFound("Enrolment not found.");
            }

            _context.Enrolments.Remove(enrolment);

            await _context.SaveChangesAsync();

            return Ok("Enrolment cancelled successfully.");
        }
    }
}