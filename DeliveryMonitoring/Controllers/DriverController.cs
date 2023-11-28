using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;

namespace DeliveryMonitoring.Controllers
{
    public class DriverController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public DriverController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var _client = _httpClientFactory.CreateClient("Delivery");
            List<Driver> drivers = new List<Driver>();

            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + "/drivers");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                drivers = JsonConvert.DeserializeObject<List<Driver>>(data);
            }
            return View(drivers);

        }

        [HttpGet]
        public async Task<IActionResult> Filter(string status, string companyTin)
        {
            var _client = _httpClientFactory.CreateClient("Delivery");
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

            HttpResponseMessage response = await _client.GetAsync(endpoint.ToString());

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                filteredDrivers = JsonConvert.DeserializeObject<List<Driver>>(data);
            }

            return View(filteredDrivers);
        }


        //Details Page Endpoint Consumption -- Starts Here
        [HttpGet("/Driver/Details/{phoneNumber}")]
        public async Task<IActionResult> Details(string phoneNumber)
        {
            var _client = _httpClientFactory.CreateClient("Delivery");


            // Fetch order data
            List<Order> orders = new List<Order>();

            HttpResponseMessage orderResponse = await _client.GetAsync(_client.BaseAddress + "/orderRequests");

            if (orderResponse.IsSuccessStatusCode)
            {
                string orderData = await orderResponse.Content.ReadAsStringAsync();
                orders = JsonConvert.DeserializeObject<List<Order>>(orderData);
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                
                //foreach (var order in orders)
                //{
                   //order.assignedDriverPhoneNumber = "0939977886";
                   //order.customer.latLng.lat = 9.01123;
                   //order.customer.latLng.lng = 38.76264;
                //}
                
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            }

            // Fetch driver data
            Driver driver = null;

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"{_client.BaseAddress}/drivers/{phoneNumber}");

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    driver = JsonConvert.DeserializeObject<Driver>(data);
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    /*
                    if (driver.phoneNumber == "0939977886")
                    {
                        driver.status = "delivering";
                        driver.latLng.lat = 9.01664;
                        driver.latLng.lng = 38.76288;
                        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    }
                    */

                    if (driver == null)
                    {
                        return NotFound(); // Return a 404 Not Found response if no driver is found.
                    }
                }
            }

            catch (HttpRequestException)
            {
                return StatusCode(500); // Handle exception with a 500 Internal Server Error
            }
            
            // Create HomeViewModel
            var viewModel = new DriverDetailsViewModel
            {
                Drivers = driver,
                Orders = orders
            };

            return View(viewModel);
        }
        //Details Page Endpoint Consumption -- Ends Here

        /////////////////////////////////////////////////////
        [HttpGet("/Driver/LiveLocation/{phoneNumber}")]
        public async Task<IActionResult> LiveLocation(string phoneNumber)
        {
            var _client = _httpClientFactory.CreateClient("Delivery");
            string data = null;

            HttpResponseMessage response = await _client.GetAsync($"{_client.BaseAddress}/drivers/{phoneNumber}");

            if (response.IsSuccessStatusCode)
            {
                data = await response.Content.ReadAsStringAsync();
            }

            return Ok(data);

        }
        ////////////////////////////////////////////////////////////
        
        [HttpGet("/Driver/Update/{phoneNumber}")]
        public async Task<IActionResult> Update(string phoneNumber)
        {
            var _client = _httpClientFactory.CreateClient("Delivery");
            // Fetch driver data
            Driver driver = null;

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"{_client.BaseAddress}/drivers/{phoneNumber}");

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    driver = JsonConvert.DeserializeObject<Driver>(data);

                    if (driver == null)
                    {
                        return NotFound(); // Return a 404 Not Found response if no driver is found.
                    }
                }
            }

            catch (HttpRequestException)
            {
                return StatusCode(500); // Handle exception with a 500 Internal Server Error
            }

            return View(driver);
        }

        [HttpPatch("/Driver/UpdateDriver/{phoneNumber}")]
        public async Task<IActionResult> UpdateDriver(string phoneNumber, [FromBody] UpdateDriverModel updateModel)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine($"invalid modelstate");
                return BadRequest(ModelState);
            }

            var _client = _httpClientFactory.CreateClient("Delivery");

            try
            {
                // Send PATCH request to update driver details
                var patchContent = new StringContent(JsonConvert.SerializeObject(updateModel), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PatchAsync($"{_client.BaseAddress}/drivers/{phoneNumber}", patchContent);

                if (response.IsSuccessStatusCode)
                {
                    // Redirect to details page or show success message
                    return RedirectToAction("Index");
                }
                else
                {
                    // Log detailed information about the response
                    Console.WriteLine($"HTTP PATCH failed with status code: {response.StatusCode}");
                    Console.WriteLine($"Response content: {await response.Content.ReadAsStringAsync()}");
                    return View("Error");
                }
            }
            catch (HttpRequestException)
            {
                // Log detailed information about the exception
                Console.WriteLine($"HTTP PATCH request failed with exception:");
                return StatusCode(500); // Handle exception with a 500 Internal Server Error
            }
        }
    }
}
