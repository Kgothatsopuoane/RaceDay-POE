
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;
using System.Security.Claims;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Results
        // Organisers view results for their own events.
        [Authorize(Roles = "Organiser")]
        [HttpGet]
        public async Task<IActionResult> GetResults()
        {
            int organiserId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Event)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Participant)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Category)
                .Where(r =>
                    r.Enrolment!.Event!.OrganiserId == organiserId)
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/Results/my
        // Participants view their own results.
        [Authorize(Roles = "Participant")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyResults()
        {
            int participantId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Event)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Category)
                .Where(r =>
                    r.Enrolment!.ParticipantId == participantId)
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/Results/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetResult(int id)
        {
            var result = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Event)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Participant)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Category)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null)
                return NotFound("Result not found.");

            int userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            bool isOrganiser =
                User.IsInRole("Organiser") &&
                result.Enrolment?.Event?.OrganiserId == userId;

            bool isParticipant =
                User.IsInRole("Participant") &&
                result.Enrolment?.ParticipantId == userId;

            if (!isOrganiser && !isParticipant)
                return Forbid();

            return Ok(result);
        }

        // POST: api/Results
        // Organisers record a participant's result.
        [Authorize(Roles = "Organiser")]
        [HttpPost]
        public async Task<IActionResult> CreateResult(
            [FromForm] CreateResultDto request)
        {
            int organiserId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e =>
                    e.EnrolmentId == request.EnrolmentId);

            if (enrolment == null)
                return NotFound("Enrolment not found.");

            if (enrolment.Event?.OrganiserId != organiserId)
                return Forbid();

            bool alreadyHasResult = await _context.Results
                .AnyAsync(r =>
                    r.EnrolmentId == request.EnrolmentId);

            if (alreadyHasResult)
                return BadRequest(
                    "A result has already been recorded for this enrolment.");

            if (request.FinishingPosition < 1)
                return BadRequest(
                    "Finishing position must be greater than zero.");

            var result = new Result
            {
                EnrolmentId = request.EnrolmentId,
                FinishTime = request.FinishTime,
                FinishingPosition = request.FinishingPosition,
                FinishTimeId = null
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Race result recorded successfully.",
                resultId = result.ResultId
            });
        }

        // PUT: api/Results/5
        [Authorize(Roles = "Organiser")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResult(
            int id,
            [FromBody] Result updatedResult)
        {
            int organiserId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Event)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null)
                return NotFound("Result not found.");

            if (result.Enrolment?.Event?.OrganiserId != organiserId)
                return Forbid();

            if (updatedResult.FinishingPosition < 1)
                return BadRequest(
                    "Finishing position must be greater than zero.");

            result.FinishingPosition =
                updatedResult.FinishingPosition;

            result.FinishTime = updatedResult.FinishTime;

            await _context.SaveChangesAsync();

            return Ok("Result updated successfully.");
        }

        // DELETE: api/Results/5
        [Authorize(Roles = "Organiser")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResult(int id)
        {
            int organiserId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Event)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null)
                return NotFound("Result not found.");

            if (result.Enrolment?.Event?.OrganiserId != organiserId)
                return Forbid();

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();

            return Ok("Result deleted successfully.");
        }
    }
}

