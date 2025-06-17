using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;

namespace DeliveryMonitoring.Controllers
{
    public class AnalyticsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public AnalyticsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        [Route("/Analytics")]
        public async Task<IActionResult> Index()
        {
            var _client = _httpClientFactory.CreateClient("Delivery");
            // Fetch driver data
            List<Driver> drivers = new();

            HttpResponseMessage driverResponse = await _client.GetAsync(_client.BaseAddress + "/drivers");

            if (driverResponse.IsSuccessStatusCode)
            {
                string driverData = await driverResponse.Content.ReadAsStringAsync();
                drivers = JsonConvert.DeserializeObject<List<Driver>>(driverData) ?? new List<Driver>();

            }

            // Fetch order data
            List<OrderDetail> orders = new();

            HttpResponseMessage orderResponse = await _client.GetAsync(_client.BaseAddress + "/orderRequests");

            if (orderResponse.IsSuccessStatusCode)
            {
                string orderData = await orderResponse.Content.ReadAsStringAsync();
                orders = JsonConvert.DeserializeObject<List<OrderDetail>>(orderData) ?? new List<OrderDetail>();
            }

            Companies company = new();

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"{_client.BaseAddress}/companies");

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    company = JsonConvert.DeserializeObject<Companies>(data) ?? new Companies();
                }

                if (company == null)
                {
                    return NotFound(); // Return a 404 Not Found response if no driver is found.
                }
            }
            catch (HttpRequestException)
            {
                return StatusCode(500); // Handle exception with a 500 Internal Server Error
            }

            // Create HomeViewModel
            var viewModel = new HomeViewModel
            {
                Drivers = drivers,
                Orders = orders,
                Comps = company
            };

            return View(viewModel);
        }
    }
}
