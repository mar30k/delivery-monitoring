using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

namespace DeliveryMonitoring.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _client;

        Uri baseAddress = new Uri("uri_path");

        public HomeController(HttpClient client)
        {
            _client = client;
            _client.BaseAddress = baseAddress;
            _client.DefaultRequestHeaders.Add("key", "api_key");
        }

        public IActionResult Index()
        {
            // Fetch driver data
            List<Driver> drivers = new List<Driver>();

            HttpResponseMessage driverResponse = _client.GetAsync(_client.BaseAddress + "/drivers").Result;

            if (driverResponse.IsSuccessStatusCode)
            {
                string driverData = driverResponse.Content.ReadAsStringAsync().Result;
                drivers = JsonConvert.DeserializeObject<List<Driver>>(driverData);
            }

            // Fetch order data
            List<Order> orders = new List<Order>();

            HttpResponseMessage orderResponse = _client.GetAsync(_client.BaseAddress + "/orderRequests").Result;

            if (orderResponse.IsSuccessStatusCode)
            {
                string orderData = orderResponse.Content.ReadAsStringAsync().Result;
                orders = JsonConvert.DeserializeObject<List<Order>>(orderData);
            }

            // Create HomeViewModel
            var viewModel = new HomeViewModel
            {
                Drivers = drivers,
                Orders = orders
            };

            return View(viewModel);
        }
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
