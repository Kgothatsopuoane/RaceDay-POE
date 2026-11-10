
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RaceDayMVC.Services;
using RaceDayMVC.Models;
using System.Text.Json;

namespace RaceDayMVC.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly ApiService _apiService;

        public ParticipantController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // PARTICIPANT DASHBOARD
        public async Task<IActionResult> Dashboard()
        {
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Login", "Account");

            if (role != "Participant")
                return RedirectToAction("Index", "Home");

            ViewBag.Email = HttpContext.Session.GetString("Email");

            try
            {
                ViewBag.Events =
                    await _apiService.GetAsync<JsonElement>("api/Events");
            }
            catch (Exception ex)
            {
                ViewBag.ApiError = ex.Message;
            }

            return View();
        }

        // BROWSE EVENTS
        public async Task<IActionResult> BrowseEvents()
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
                return RedirectToAction("Login", "Account");

            try
            {
                ViewBag.Events =
                    await _apiService.GetAsync<JsonElement>("api/Events");

                ViewBag.Categories =
                    await _apiService.GetAsync<JsonElement>("api/Categories");
            }
            catch (Exception ex)
            {
                ViewBag.ApiError = ex.Message;
            }

            return View();
        }

        // VIEW EVENT DETAILS
        public async Task<IActionResult> EventDetails(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
                return RedirectToAction("Login", "Account");

            if (id <= 0)
                return NotFound();

            try
            {
                var raceEvent =
                    await _apiService.GetAsync<JsonElement>(
                        $"api/Events/{id}");

                if (raceEvent.ValueKind != JsonValueKind.Object)
                    return NotFound();

                return View(raceEvent);
            }
            catch (Exception ex)
            {
                ViewBag.ApiError = ex.Message;
                return View();
            }
        }

        // ENROL IN AN EVENT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(int eventId, int categoryId)
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
                return RedirectToAction("Login", "Account");

            try
            {
                var response = await _apiService.PostFormAsync(
                    "api/Enrolments",
                    new Dictionary<string, string>
                    {
                        ["EventId"] = eventId.ToString(),
                        ["CategoryId"] = categoryId.ToString()
                    });

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] =
                        "You have successfully enrolled in this event.";

                    return RedirectToAction("MyEnrolments");
                }

                var error = await response.Content.ReadAsStringAsync();

                TempData["EnrolError"] =
                    string.IsNullOrWhiteSpace(error)
                        ? $"Enrolment failed. Status: {(int)response.StatusCode}"
                        : error;
            }
            catch (Exception ex)
            {
                TempData["EnrolError"] = ex.Message;
            }

            return RedirectToAction("BrowseEvents");
        }

        // MY ENROLMENTS
        public async Task<IActionResult> MyEnrolments()
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
                return RedirectToAction("Login", "Account");

            try
            {
                ViewBag.Enrolments =
                    await _apiService.GetAsync<JsonElement>(
                        "api/Enrolments/my");
            }
            catch (Exception ex)
            {
                ViewBag.ApiError = ex.Message;
            }

            return View();
        }

        // MY RESULTS
        public async Task<IActionResult> MyResults()
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
                return RedirectToAction("Login", "Account");

            try
            {
                ViewBag.Results =
                    await _apiService.GetAsync<JsonElement>(
                        "api/Results/my");
            }
            catch (Exception ex)
            {
                ViewBag.ApiError = ex.Message;
            }

            return View();
        }

        // MY PROFILE
        public async Task<IActionResult> Profile()
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
                return RedirectToAction("Login", "Account");

            try
            {
                var profile =
                    await _apiService.GetAsync<ParticipantProfileViewModel>(
                        "api/Users/profile");

                if (profile == null)
                {
                    ViewBag.ApiError = "Could not load your profile.";
                    return View(new ParticipantProfileViewModel());
                }

                return View(profile);
            }
            catch (Exception ex)
            {
                ViewBag.ApiError = ex.Message;
                return View(new ParticipantProfileViewModel());
            }
        }

        // UPDATE PARTICIPANT PROFILE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(
            string firstName,
            string lastName)
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
                return RedirectToAction("Login", "Account");

            firstName = firstName?.Trim() ?? "";
            lastName = lastName?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName))
            {
                TempData["ProfileError"] =
                    "First name and last name are required.";

                return RedirectToAction("Profile");
            }

            try
            {
                var response = await _apiService.PutFormAsync(
                    "api/Users/profile",
                    new Dictionary<string, string>
                    {
                        ["firstName"] = firstName,
                        ["lastName"] = lastName
                    });

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] =
                        "Your profile details were updated successfully.";
                }
                else
                {
                    var error =
                        await response.Content.ReadAsStringAsync();

                    TempData["ProfileError"] =
                        string.IsNullOrWhiteSpace(error)
                            ? $"Profile update failed. Status: {(int)response.StatusCode}"
                            : error;
                }
            }
            catch (Exception ex)
            {
                TempData["ProfileError"] = ex.Message;
            }

            return RedirectToAction("Profile");
        }

        // DISPLAY PROFILE PICTURE THROUGH MVC
        [HttpGet]
        public async Task<IActionResult> ProfilePicture()
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
                return Unauthorized();

            try
            {
                var response = await _apiService.GetFileAsync(
                    "api/Images/profile-picture");

                if (!response.IsSuccessStatusCode)
                    return NotFound();

                var imageBytes =
                    await response.Content.ReadAsByteArrayAsync();

                var contentType =
                    response.Content.Headers.ContentType?.MediaType
                    ?? "application/octet-stream";

                return File(imageBytes, contentType);
            }
            catch
            {
                return NotFound();
            }
        }

        // UPLOAD PROFILE PICTURE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
                return RedirectToAction("Login", "Account");

            if (file == null || file.Length == 0)
            {
                TempData["ProfileError"] = "Please select a picture.";
                return RedirectToAction("Profile");
            }

            if (!file.ContentType.StartsWith(
                "image/", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ProfileError"] = "Please select a valid image.";
                return RedirectToAction("Profile");
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                TempData["ProfileError"] = "Image must not exceed 5 MB.";
                return RedirectToAction("Profile");
            }

            try
            {
                var response = await _apiService.PostFileAsync(
                    "api/Images/profile-picture", file);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] =
                        "Profile picture uploaded successfully.";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();

                    TempData["ProfileError"] =
                        string.IsNullOrWhiteSpace(error)
                            ? $"Upload failed. Status: {(int)response.StatusCode}"
                            : error;
                }
            }
            catch (Exception ex)
            {
                TempData["ProfileError"] = ex.Message;
            }

            return RedirectToAction("Profile");
        }

        // DISPLAY EVENT BANNER THROUGH MVC
        [HttpGet]
        public async Task<IActionResult> EventBanner(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
                return Unauthorized();

            try
            {
                var response = await _apiService.GetFileAsync(
                    $"api/Images/event-banner/{id}");

                if (!response.IsSuccessStatusCode)
                    return NotFound();

                var imageBytes =
                    await response.Content.ReadAsByteArrayAsync();

                var contentType =
                    response.Content.Headers.ContentType?.MediaType
                    ?? "image/jpeg";

                return File(imageBytes, contentType);
            }
            catch
            {
                return NotFound();
            }
        }
    }
}
