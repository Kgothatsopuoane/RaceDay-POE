

using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Services;

namespace RaceDayMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiService _apiService;

        public HomeController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var events = await _apiService.GetAsync<object>("api/Events");

                ViewBag.ApiStatus = "Successfully connected to RaceDayAPI!";
                ViewBag.Events = events;
            }
            catch (Exception ex)
            {
                ViewBag.ApiStatus = "API Error: " + ex.Message;
                ViewBag.Events = null;
            }

            return View();
        }

        public async Task<IActionResult> ParticipantDashboard()
        {
            try
            {
                var events = await _apiService.GetAsync<object>("api/Events");

                ViewBag.Events = events;
                ViewBag.WelcomeMessage =
                    "Welcome, " + HttpContext.Session.GetString("Email");
            }
            catch (Exception ex)
            {
                ViewBag.Events = null;
                ViewBag.ApiError = ex.Message;
            }

            return View();
        }
    }
}
