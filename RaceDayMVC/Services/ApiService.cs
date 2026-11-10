
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;

namespace RaceDayMVC.Services
{
    public class ApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        // CREATE API CLIENT AND FORWARD AUTHENTICATION COOKIE
        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var apiCookie = _httpContextAccessor.HttpContext?
                .Session.GetString("ApiCookie");

            if (!string.IsNullOrEmpty(apiCookie))
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(
                    "Cookie", apiCookie);
            }

            return client;
        }

        // GET DATA FROM API
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            var client = CreateClient();

            return await client.GetFromJsonAsync<T>(endpoint);
        }

        // GET IMAGE OR FILE FROM API
        public async Task<HttpResponseMessage> GetFileAsync(
            string endpoint)
        {
            var client = CreateClient();

            return await client.GetAsync(endpoint);
        }

        // POST JSON DATA TO API
        public async Task<HttpResponseMessage> PostAsync<T>(
            string endpoint, T data)
        {
            var client = CreateClient();

            return await client.PostAsJsonAsync(endpoint, data);
        }

        // POST FORM DATA TO API
        public async Task<HttpResponseMessage> PostFormAsync(
            string endpoint,
            Dictionary<string, string> data)
        {
            var client = CreateClient();

            var content = new FormUrlEncodedContent(data);

            return await client.PostAsync(endpoint, content);
        }

        // PUT FORM DATA TO API
        public async Task<HttpResponseMessage> PutFormAsync(
            string endpoint,
            Dictionary<string, string> data)
        {
            var client = CreateClient();

            var content = new FormUrlEncodedContent(data);

            return await client.PutAsync(endpoint, content);
        }

        // POST IMAGE FILE TO API
        public async Task<HttpResponseMessage> PostFileAsync(
            string endpoint,
            IFormFile file)
        {
            var client = CreateClient();

            using var content = new MultipartFormDataContent();

            using var stream = file.OpenReadStream();

            using var fileContent = new StreamContent(stream);

            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(
                    file.ContentType);

            content.Add(fileContent, "file", file.FileName);

            return await client.PostAsync(endpoint, content);
        }

        // PUT JSON DATA TO API
        public async Task<HttpResponseMessage> PutAsync<T>(
            string endpoint, T data)
        {
            var client = CreateClient();

            return await client.PutAsJsonAsync(endpoint, data);
        }

        // DELETE DATA FROM API
        public async Task<HttpResponseMessage> DeleteAsync(
            string endpoint)
        {
            var client = CreateClient();

            return await client.DeleteAsync(endpoint);
        }
    }
}
