using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;

namespace DeliveryMonitoring.Controllers
{
    public class OrderController : Controller
    {
        private readonly HttpClient _client;

        Uri baseAddress = new Uri("http://196.189.21.67:8084/api");
        public OrderController(HttpClient client)
        {
            _client = client;
            _client.BaseAddress = baseAddress;
            _client.DefaultRequestHeaders.Add("x-api-key", "c666e0e9-fnnm-5804-bbxo-144ad72ae730");
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<Order> orders = new List<Order>();

            HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/orderRequests").Result;

            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                orders = JsonConvert.DeserializeObject<List<Order>>(data);
            }
            return View(orders);

        }

        [HttpGet("/Order/Details/{voucherCode}")]
        public async Task<IActionResult> Details(string voucherCode)
        {
            Order order = null;

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"{_client.BaseAddress}/orderRequests/{voucherCode}");

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    order = JsonConvert.DeserializeObject<Order>(data);
                }

                if (order == null)
                {
                    return NotFound(); // Return a 404 Not Found response if no driver is found.
                }

                return View(order);
            }
            catch (HttpRequestException)
            {
                return StatusCode(500); // Handle exception with a 500 Internal Server Error
            }
        }
    }
}
