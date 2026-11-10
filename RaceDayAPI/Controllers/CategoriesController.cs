using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .Include(c => c.Event)
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/Categories/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound("Category not found.");
            }

            return Ok(category);
        }

        // POST: api/Categories
        [Authorize(Roles = "Organiser")]
        [HttpPost]
        public async Task<IActionResult> CreateCategory(
            [FromForm] Category category)
        {
            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == category.EventId);

            if (raceEvent == null)
            {
                return NotFound("Event not found.");
            }

            int organiserId = int.Parse(
                User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier
                )!.Value
            );

            if (raceEvent.OrganiserId != organiserId)
            {
                return Forbid();
            }

            _context.Categories.Add(category);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.CategoryId },
                category
            );
        }

        // PUT: api/Categories/1
        [Authorize(Roles = "Organiser")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(
            int id,
            [FromForm] Category updatedCategory)
        {
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound("Category not found.");
            }

            int organiserId = int.Parse(
                User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier
                )!.Value
            );

            if (category.Event == null ||
                category.Event.OrganiserId != organiserId)
            {
                return Forbid();
            }

            category.CategoryName = updatedCategory.CategoryName;

            await _context.SaveChangesAsync();

            return Ok(category);
        }

        // DELETE: api/Categories/1
        [Authorize(Roles = "Organiser")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound("Category not found.");
            }

            int organiserId = int.Parse(
                User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier
                )!.Value
            );

            if (category.Event == null ||
                category.Event.OrganiserId != organiserId)
            {
                return Forbid();
            }

            _context.Categories.Remove(category);

            await _context.SaveChangesAsync();

            return Ok("Category deleted successfully.");
        }
    }
}