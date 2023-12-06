using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using static NuGet.Packaging.PackagingConstants;

namespace DeliveryMonitoring.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        //HttpClient Setup starts here
        private readonly IHttpClientFactory _httpClientFactory;
        public OrderController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        //HttpClient Setup ends here

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var _client = _httpClientFactory.CreateClient("Delivery");
            List<Order> orders = new List<Order>();

            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + "/orderRequests");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                orders = JsonConvert.DeserializeObject<List<Order>>(data);
            }
            return View(orders);

        }

        [HttpGet("/Order/Details/{voucherCode}")]
        public async Task<IActionResult> Details(string voucherCode)
        {
            var _client = _httpClientFactory.CreateClient("Delivery");
            OrderDetail order = null;

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"{_client.BaseAddress}/orderRequests/{voucherCode}");

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    order = JsonConvert.DeserializeObject<OrderDetail>(data);
                    //order.assignedDriverPhoneNumber = "0924438476";
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
