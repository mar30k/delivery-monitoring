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
        private readonly HttpClient _client;

        Uri baseAddress = new Uri("http://196.189.21.67:8084/api");

        public HomeController(HttpClient client)
        {
            _client = client;
            _client.BaseAddress = baseAddress;
            _client.DefaultRequestHeaders.Add("x-api-key", "c666e0e9-fnnm-5804-bbxo-144ad72ae730");
        }
        //HttpClient Setup ends here

        //This returns the view for Home/Index Page
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
        //This is the end of the code that returns the view for Home/Index Page

        //This is the view for Home/Privacy Page
        public IActionResult Privacy()
        {
            return View();
        }
        //This is the end of the code for the view of Home/Privacy Page
    }
}
