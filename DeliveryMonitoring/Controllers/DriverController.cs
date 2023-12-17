using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;

namespace DeliveryMonitoring.Controllers
{
    [Authorize]
    public class DriverController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public DriverController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        

        //Driver Index Page - starts here
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
        //Driver Index Page - ends here

        //Used for fetching the all driver's location regularly - starts here
        [HttpGet("/Driver/LiveLocation")]
        public async Task<IActionResult> LiveLocation()
        {
            var _client = _httpClientFactory.CreateClient("Delivery");
            string data = null;

            HttpResponseMessage response = await _client.GetAsync($"{_client.BaseAddress}/drivers");

            if (response.IsSuccessStatusCode)
            {
                data = await response.Content.ReadAsStringAsync();
            }
            return Ok(data);
        }
        //Used for fetching the all driver's location regularly - ends here       


        //Used for filtering Drivers based on their status & company TIN - starts here
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
        //Used for filtering Drivers based on their status & company TIN - ends here

        //Used for filtering Drivers Live Location based on their status & company TIN - starts here
        [HttpGet("/Driver/LiveFilter/{status?}/{companyTin?}")]
        public async Task<IActionResult> LiveFilter(string? status, string companyTin)
        {
            string data = null;
            var _client = _httpClientFactory.CreateClient("Delivery");
            if (string.IsNullOrEmpty(status) && string.IsNullOrEmpty(companyTin))
            {
                return RedirectToAction("Index");
            }

            List<Driver> filteredDrivers = new List<Driver>();

            // Build the endpoint based on the provided filters
            StringBuilder endpoint = new StringBuilder($"{_client.BaseAddress}/drivers?");

            if (status != "null")
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
                data = await response.Content.ReadAsStringAsync();                
            }

            return Ok(data);
        }
        //Used for filtering Drivers based on their status & company TIN - ends here


        //Used for filtering Drivers based on their CompanyTIN - starts here
        [HttpGet("/Driver/FilterCompany/{companyTin}")]
        public async Task<IActionResult> FilterCompany(string companyTin)
        {
            var _client = _httpClientFactory.CreateClient("Delivery");
            
            List<Driver> filteredDrivers = new List<Driver>();

            HttpResponseMessage response = await _client.GetAsync($"{_client.BaseAddress}/drivers?companyTin={companyTin}");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                filteredDrivers = JsonConvert.DeserializeObject<List<Driver>>(data);
            }
            return View(filteredDrivers);
        }
        //Used for filtering Drivers based on their CompanyTIN - ends here

        //Used for fetching the driver's location regularly - starts here
        [HttpGet("/Driver/LiveLocationByCompany/{companyTin}")]
        public async Task<IActionResult> LiveLocationByCompany(string companyTin)
        {
            var _client = _httpClientFactory.CreateClient("Delivery");
            string data = null;

            HttpResponseMessage response = await _client.GetAsync($"{_client.BaseAddress}/drivers?companyTin={companyTin}");

            if (response.IsSuccessStatusCode)
            {
                data = await response.Content.ReadAsStringAsync();
            }
            return Ok(data);
        }
        //Used for fetching the driver's location regularly - ends here

        //Driver Details-- Starts Here
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

                //foreach (var order in orders)
                //{
                //    order.assignedDriverPhoneNumber = "0918539962";
                //    order.customer.latLng.lat = 9.01123;
                //    order.customer.latLng.lng = 38.76264;
                //}
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

                    //if (driver.phoneNumber == "0912918305")
                    //{
                    //    //driver.status = "delivering";
                    //    driver.latLng.lat = 9.0166004;
                    //    driver.latLng.lng = 38.7631881;
                    //}

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
        //Driver Details -- Ends Here

        //Used for fetching the driver's location regularly - starts here
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
        //Used for fetching the driver's location regularly - ends here

        //Driver Update Page - starts here
        [HttpGet("/Driver/Update/{phoneNumber}")]
        public async Task<IActionResult> Update(string phoneNumber)
        {
            var _client = _httpClientFactory.CreateClient("Delivery");
            // Fetch driver data
            UpdateDriverModel driver = null;

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"{_client.BaseAddress}/drivers/{phoneNumber}");

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    driver = JsonConvert.DeserializeObject<UpdateDriverModel>(data);

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
        //Driver Update Page - ends here

        //Used for updating driver's information - starts here
        [HttpPatch("/Driver/Update/{phoneNumber}")]
        public async Task<IActionResult> Update([Bind("firstName,isDisabled,phoneNumber,companyTin")] UpdateDriverModel update, string phoneNumber, [FromBody] UpdateDriverModel updateModel)
        {
            var _client = _httpClientFactory.CreateClient("Delivery");

            try
            {
                // Send PATCH request to update driver details
                var patchContent = new StringContent(JsonConvert.SerializeObject(updateModel), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PatchAsync($"{_client.BaseAddress}/drivers/{phoneNumber}", patchContent);
            }
            catch(HttpRequestException)
            {
                // Log detailed information about the exception
                Console.WriteLine($"HTTP PATCH request failed with exception:");
                return StatusCode(500); // Handle exception with a 500 Internal Server Error
            }

            if (ModelState.IsValid)
            {
                // Pass the updated data to the view
                return View("Index", "Driver");
            }
            return View(update);
        }
        //Used for updating driver's information - ends here

    }
}
