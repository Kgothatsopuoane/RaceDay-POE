
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using System.Net.Http.Json;

namespace RaceDayMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Email", model.Email),
                new KeyValuePair<string, string>("Password", model.Password)
            });

            var response = await client.PostAsync("api/Auth/login", form);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            if (!response.Headers.TryGetValues("Set-Cookie", out var cookieHeaders))
            {
                ModelState.AddModelError(
                    "",
                    "Login succeeded, but the API authentication cookie was not received.");

                return View(model);
            }

            var authCookie = cookieHeaders.FirstOrDefault(c =>
                c.StartsWith("RaceDayAuth=", StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(authCookie))
            {
                ModelState.AddModelError("", "The RaceDayAuth cookie was not found.");
                return View(model);
            }

            authCookie = authCookie.Split(';')[0];

            HttpContext.Session.SetString("ApiCookie", authCookie);

            var result = await response.Content.ReadFromJsonAsync<LoginResult>();

            if (result == null)
            {
                HttpContext.Session.Clear();
                ModelState.AddModelError("", "Could not read the login response.");
                return View(model);
            }

            // Save login details
            HttpContext.Session.SetString("UserId", result.UserId.ToString());
            HttpContext.Session.SetString("Email", result.Email);
            HttpContext.Session.SetString("Role", result.Role);

            // Redirect according to the actual role
            if (string.Equals(result.Role, "Organiser",
                StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Dashboard", "Organiser");
            }

            if (string.Equals(result.Role, "Participant",
                StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Dashboard", "Participant");
            }

            HttpContext.Session.Clear();

            ModelState.AddModelError("", "Your account has an invalid role.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("FirstName", model.FirstName),
                new KeyValuePair<string, string>("LastName", model.LastName),
                new KeyValuePair<string, string>("Email", model.Email),
                new KeyValuePair<string, string>("Password", model.Password),
                new KeyValuePair<string, string>("Role", model.Role)
            });

            var response = await client.PostAsync("api/Auth/register", form);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    "",
                    "Registration failed. Check your details or email.");

                return View(model);
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var apiCookie = HttpContext.Session.GetString("ApiCookie");

            if (!string.IsNullOrEmpty(apiCookie))
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(
                    "Cookie",
                    apiCookie);

                await client.PostAsync("api/Auth/logout", null);
            }

            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }

    public class LoginResult
    {
        public int UserId { get; set; }

        public string Email { get; set; } = "";

        public string Role { get; set; } = "";
    }
}
