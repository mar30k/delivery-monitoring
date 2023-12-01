using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

namespace DeliveryMonitoring.Controllers
{
    
    public class HomeController : Controller
    {
        //HttpClient Setup starts here
        private readonly IHttpClientFactory _httpClientFactory;
        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        //HttpClient Setup ends here

        //This returns the view for Home/Index Page
        public async Task<IActionResult> Index()
        {
            var _client = _httpClientFactory.CreateClient("Delivery");
            // Fetch driver data
            List<Driver> drivers = new List<Driver>();

            HttpResponseMessage driverResponse = await _client.GetAsync(_client.BaseAddress + "/drivers");

            if (driverResponse.IsSuccessStatusCode)
            {
                string driverData = await driverResponse.Content.ReadAsStringAsync();
                drivers = JsonConvert.DeserializeObject<List<Driver>>(driverData);
                
            }

            // Fetch order data
            List<Order> orders = new List<Order>();

            HttpResponseMessage orderResponse = await _client.GetAsync(_client.BaseAddress + "/orderRequests");

            if (orderResponse.IsSuccessStatusCode)
            {
                string orderData = await orderResponse.Content.ReadAsStringAsync();
                orders = JsonConvert.DeserializeObject<List<Order>>(orderData);
            }

            Companies company = null;

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"{_client.BaseAddress}/companies");

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    company = JsonConvert.DeserializeObject<Companies>(data);
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
        //This is the end of the code that returns the view for Home/Index Page
    }
}
