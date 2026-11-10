using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("public")]
        public IActionResult Public()
        {
            return Ok("Anyone can access this endpoint.");
        }

        [Authorize]
        [HttpGet("private")]
        public IActionResult Private()
        {
            return Ok("You are authenticated.");
        }

        [Authorize(Roles = "Organiser")]
        [HttpGet("organiser")]
        public IActionResult Organiser()
        {
            return Ok("You are an Organiser.");
        }

        [Authorize(Roles = "Participant")]
        [HttpGet("participant")]
        public IActionResult Participant()
        {
            return Ok("You are a Participant.");
        }
    }
}