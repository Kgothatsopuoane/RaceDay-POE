
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
using System.Globalization;
using System.Text.Json;

namespace RaceDayMVC.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly ApiService _apiService;

        public OrganiserController(ApiService apiService)
        {
            _apiService = apiService;
        }

        private bool IsOrganiser()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Organiser";
        }

        // UPLOAD EVENT BANNER TO AZURE
        private async Task<string?> UploadEventBannerAsync(
            IFormFile? eventBanner)
        {
            if (eventBanner == null || eventBanner.Length == 0)
                return null;

            if (!eventBanner.ContentType.StartsWith(
                "image/", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Please select a valid image.");
            }

            if (eventBanner.Length > 5 * 1024 * 1024)
            {
                throw new InvalidOperationException(
                    "Event banner must not exceed 5 MB.");
            }

            var response = await _apiService.PostFileAsync(
                "api/Images/event-banner", eventBanner);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(responseBody)
                        ? "Event banner upload failed."
                        : responseBody);
            }

            using var document =
                JsonDocument.Parse(responseBody);

            if (document.RootElement.TryGetProperty(
                "imageUrl", out var imageUrl))
            {
                return imageUrl.GetString();
            }

            throw new InvalidOperationException(
                "The API did not return the event banner URL.");
        }

        // ORGANISER DASHBOARD
        public async Task<IActionResult> Dashboard()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            ViewBag.Email = HttpContext.Session.GetString("Email");

            try
            {
                var userId = HttpContext.Session.GetString("UserId");

                if (!int.TryParse(userId, out int organiserId))
                    return RedirectToAction("Login", "Account");

                // GET ALL EVENTS
                var events =
                    await _apiService.GetAsync<List<EventViewModel>>(
                        "api/Events");

                // GET ALL ENROLMENTS
                var enrolments =
                    await _apiService.GetAsync<List<EnrolmentViewModel>>(
                        "api/Enrolments");

                // FILTER EVENTS BELONGING TO THIS ORGANISER
                var organiserEvents = events?
                    .Where(e => e.OrganiserId == organiserId)
                    .ToList() ?? new List<EventViewModel>();

                // FILTER ENROLMENTS BELONGING TO THIS ORGANISER
                var organiserEnrolments = enrolments?
                    .Where(e =>
                        e.Event != null &&
                        e.Event.OrganiserId == organiserId)
                    .ToList() ?? new List<EnrolmentViewModel>();

                // DASHBOARD STATISTICS
                ViewBag.TotalEvents = organiserEvents.Count;

                ViewBag.UpcomingEvents = organiserEvents.Count(e =>
                    e.EventDate >= DateTime.Today);

                ViewBag.TotalEnrolments = organiserEnrolments.Count;

                // UPCOMING EVENTS
                ViewBag.UpcomingEventList = organiserEvents
                    .Where(e => e.EventDate >= DateTime.Today)
                    .OrderBy(e => e.EventDate)
                    .Take(5)
                    .ToList();
            }
            catch (Exception ex)
            {
                ViewBag.ApiError =
                    "Could not load dashboard information: " + ex.Message;

                ViewBag.TotalEvents = 0;
                ViewBag.UpcomingEvents = 0;
                ViewBag.TotalEnrolments = 0;

                ViewBag.UpcomingEventList =
                    new List<EventViewModel>();
            }

            return View();
        }

        // DISPLAY ALL EVENTS BELONGING TO THE ORGANISER
        public async Task<IActionResult> ManageEvents()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var events = await _apiService.GetAsync<List<EventViewModel>>(
                "api/Events");

            var userId = HttpContext.Session.GetString("UserId");

            if (int.TryParse(userId, out int organiserId))
            {
                events = events?
                    .Where(e => e.OrganiserId == organiserId)
                    .ToList();
            }

            return View(events ?? new List<EventViewModel>());
        }

        // DISPLAY CREATE EVENT FORM
        [HttpGet]
        public IActionResult CreateEvent()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            return View();
        }

        // CREATE EVENT AND UPLOAD BANNER
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(
            EventViewModel model,
            IFormFile? eventBanner)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            try
            {
                // UPLOAD BANNER IF SELECTED
                if (eventBanner != null && eventBanner.Length > 0)
                {
                    model.EventBannerUrl =
                        await UploadEventBannerAsync(eventBanner);
                }

                var data = new Dictionary<string, string>
                {
                    { "EventName", model.EventName },
                    { "Description", model.Description },
                    { "EventDate", model.EventDate.ToString("yyyy-MM-ddTHH:mm:ss") },
                    { "Location", model.Location },
                    { "DistanceKm", model.DistanceKm.ToString(CultureInfo.InvariantCulture) },
                    { "EventType", model.EventType },
                    { "EventBannerUrl", model.EventBannerUrl ?? "" }
                };

                var response = await _apiService.PostFormAsync(
                    "api/Events", data);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] =
                        "Event created successfully!";

                    return RedirectToAction("ManageEvents");
                }

                ViewBag.Error =
                    await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            return View(model);
        }

        // DISPLAY EDIT EVENT FORM
        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var raceEvent = await _apiService.GetAsync<EventViewModel>(
                $"api/Events/{id}");

            if (raceEvent == null)
                return NotFound("Event not found.");

            var userId = HttpContext.Session.GetString("UserId");

            if (!int.TryParse(userId, out int organiserId) ||
                raceEvent.OrganiserId != organiserId)
            {
                return Forbid();
            }

            return View(raceEvent);
        }

        // SAVE EDITED EVENT AND OPTIONAL NEW BANNER
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(
            int id,
            EventViewModel model,
            IFormFile? eventBanner)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetString("UserId");

            if (!int.TryParse(userId, out int organiserId))
                return Forbid();

            var existingEvent =
                await _apiService.GetAsync<EventViewModel>(
                    $"api/Events/{id}");

            if (existingEvent == null)
                return NotFound("Event not found.");

            if (existingEvent.OrganiserId != organiserId)
                return Forbid();

            try
            {
                model.EventId = id;
                model.OrganiserId = organiserId;

                // KEEP EXISTING BANNER UNLESS A NEW ONE IS UPLOADED
                model.EventBannerUrl = existingEvent.EventBannerUrl;

                if (eventBanner != null && eventBanner.Length > 0)
                {
                    model.EventBannerUrl =
                        await UploadEventBannerAsync(eventBanner);
                }

                var response = await _apiService.PutAsync(
                    $"api/Events/{id}", model);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] =
                        "Event updated successfully!";

                    return RedirectToAction("ManageEvents");
                }

                ViewBag.Error =
                    await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            return View(model);
        }

        // DELETE EVENT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var response = await _apiService.DeleteAsync(
                $"api/Events/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] =
                    "Event deleted successfully!";
            }
            else
            {
                TempData["Error"] =
                    "Could not delete event: " +
                    await response.Content.ReadAsStringAsync();
            }

            return RedirectToAction("ManageEvents");
        }

        // ORGANISER ENROLMENTS PAGE
        public async Task<IActionResult> Enrolments()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var enrolments =
                await _apiService.GetAsync<List<EnrolmentViewModel>>(
                    "api/Enrolments");

            return View(enrolments ?? new List<EnrolmentViewModel>());
        }

        // DISPLAY RACE RESULTS
        public async Task<IActionResult> Results()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var results =
                await _apiService.GetAsync<List<ResultViewModel>>(
                    "api/Results");

            return View(results ?? new List<ResultViewModel>());
        }

        // LOAD ENROLLED PARTICIPANTS
        private async Task PopulateResultDropdowns(
            CreateResultViewModel model)
        {
            var enrolments =
                await _apiService.GetAsync<List<EnrolmentViewModel>>(
                    "api/Enrolments");

            var userId = HttpContext.Session.GetString("UserId");

            if (!int.TryParse(userId, out int organiserId))
            {
                model.Enrolments =
                    new List<ResultEnrolmentOptionViewModel>();
                return;
            }

            var organiserEnrolments = enrolments?
                .Where(e =>
                    e.Event != null &&
                    e.Event.OrganiserId == organiserId &&
                    e.Participant != null)
                .ToList();

            model.Enrolments = organiserEnrolments?
                .Select(e => new ResultEnrolmentOptionViewModel
                {
                    EnrolmentId = e.EnrolmentId,

                    EventName =
                        e.Event?.EventName ?? "Unknown event",

                    ParticipantName =
                        (e.Participant?.FirstName ?? "") + " " +
                        (e.Participant?.LastName ?? ""),

                    CategoryName =
                        e.Category?.CategoryName ?? "Unknown category"
                })
                .ToList() ?? new List<ResultEnrolmentOptionViewModel>();
        }

        // DISPLAY ADD RESULT FORM
        [HttpGet]
        public async Task<IActionResult> CreateResult()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var model = new CreateResultViewModel();

            await PopulateResultDropdowns(model);

            return View(model);
        }

        // SAVE PARTICIPANT RESULT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateResult(
            CreateResultViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetString("UserId");

            if (!int.TryParse(userId, out int organiserId))
                return Forbid();

            var enrolments =
                await _apiService.GetAsync<List<EnrolmentViewModel>>(
                    "api/Enrolments");

            var selectedEnrolment = enrolments?
                .FirstOrDefault(e =>
                    e.EnrolmentId == model.EnrolmentId &&
                    e.Event != null &&
                    e.Event.OrganiserId == organiserId);

            if (selectedEnrolment == null)
            {
                ViewBag.Error =
                    "Please select a participant enrolled in one of your events.";

                await PopulateResultDropdowns(model);
                return View(model);
            }

            if (model.FinishingPosition < 1)
            {
                ViewBag.Error =
                    "Finishing position must be at least 1.";

                await PopulateResultDropdowns(model);
                return View(model);
            }

            if (model.FinishTime < TimeSpan.Zero)
            {
                ViewBag.Error =
                    "Finish time cannot be negative.";

                await PopulateResultDropdowns(model);
                return View(model);
            }

            var data = new Dictionary<string, string>
            {
                { "EnrolmentId", model.EnrolmentId.ToString() },
                { "FinishingPosition", model.FinishingPosition.ToString() },
                { "FinishTime", model.FinishTime.ToString(@"hh\:mm\:ss") }
            };

            var response = await _apiService.PostFormAsync(
                "api/Results", data);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] =
                    "Race result recorded successfully!";

                return RedirectToAction("Results");
            }

            ViewBag.Error =
                await response.Content.ReadAsStringAsync();

            await PopulateResultDropdowns(model);

            return View(model);
        }
    }
}
