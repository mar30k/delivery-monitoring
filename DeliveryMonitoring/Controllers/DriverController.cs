using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace DeliveryMonitoring.Controllers
{
    public class DriverController : Controller
    {
        private readonly HttpClient _client;

        Uri baseAddress = new Uri("uri_path");
        public DriverController(HttpClient client)
        {
            _client = client;
            _client.BaseAddress = baseAddress;
            _client.DefaultRequestHeaders.Add("key", "api_key");
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<Driver> drivers = new List<Driver>();

            HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/drivers").Result;

            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                drivers = JsonConvert.DeserializeObject<List<Driver>>(data);
            }
            return View(drivers);

        }

        [HttpGet]
        public IActionResult Filter(string status, string companyTin)
        {
            if (string.IsNullOrEmpty(status) && string.IsNullOrEmpty(companyTin))
            {
                return RedirectToAction("Index");
            }

            List<Driver> filteredDrivers = new List<Driver>();

            // Build the endpoint based on the provided filters
            StringBuilder endpoint = new StringBuilder($"{_client.BaseAddress}/drivers?");

            if (!string.IsNullOrEmpty(status))
            {
                endpoint.Append($"status={status}");
            }

            if (!string.IsNullOrEmpty(companyTin))
            {
                if (endpoint.Length > 0)
                {
                    endpoint.Append("&");
                }
                endpoint.Append($"companyTin={companyTin}");
            }

            HttpResponseMessage response = _client.GetAsync(endpoint.ToString()).Result;

            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                filteredDrivers = JsonConvert.DeserializeObject<List<Driver>>(data);
            }

            return View(filteredDrivers);
        }

        [HttpGet("/Driver/Details/{phoneNumber}")]
        public async Task<IActionResult> Details(string phoneNumber)
        {
            Driver driver = null;

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"{_client.BaseAddress}/drivers/{phoneNumber}");

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    driver = JsonConvert.DeserializeObject<Driver>(data);
                }

                if (driver == null)
                {
                    return NotFound(); // Return a 404 Not Found response if no driver is found.
                }

                return View(driver);
            }
            catch (HttpRequestException)
            {
                return StatusCode(500); // Handle exception with a 500 Internal Server Error
            }
        }

        
    }
}
