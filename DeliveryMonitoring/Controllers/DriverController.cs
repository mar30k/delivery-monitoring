using CNET_ERP_V7.WebConstants;
using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;

namespace DeliveryMonitoring.Controllers
{
    [Authorize]
    [Route("Driver")]
    public class DriverController : Controller
    {
        private IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;
        public DriverController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        //Driver Index Page - starts here
        [HttpGet]
        [Route("/drivers")]
        public async Task<IActionResult> Index()
        {
            var _client = _httpClientFactory.CreateClient("Delivery");
            List<Driver> drivers = new();
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];
            string uri = companyTin == "0076217301" ? "/drivers" : $"/drivers?companyTin={companyTin}";

            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + uri);
            ViewBag.CompanyTin = companyTin;
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                drivers = JsonConvert.DeserializeObject<List<Driver>>(data) ?? new List<Driver>();
            }
            return View(drivers);         
        }
        //Driver Index Page - ends here

        //Used for fetching the all driver's location regularly - starts here
        [HttpGet("/Driver/LiveLocation")]
        public async Task<IActionResult> LiveLocation()
        {
            var _client = _httpClientFactory.CreateClient("Delivery");
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];
            string uri = companyTin == "0076217301" ? "/drivers" : $"/drivers?companyTin={companyTin}";
            var data = new List<Driver>();
            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + uri);

            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                data = JsonConvert.DeserializeObject<List<Driver>>(responseString);
                foreach (var item in data ?? new List<Driver>())
                {
                    item.lastUpdatedAtIso = item?.updatedAt?.ToString("yyyy-MM-dd HH:mm:ss");
                }

            }
            return Ok(data);
        }
        //Used for fetching the all driver's location regularly - ends here       


        //Used for filtering Drivers based on their status & company TIN - starts here
        [HttpGet]
        public async Task<IActionResult> Filter(string status, string companyTin)
        {
            var cookieCompanyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];

            var _client = _httpClientFactory.CreateClient("Delivery");
            if (string.IsNullOrEmpty(status) && string.IsNullOrEmpty(companyTin))
            {
                return RedirectToAction("Index");
            }

            List<Driver> filteredDrivers = new();

            // Build the endpoint based on the provided filters
            StringBuilder endpoint = new($"{_client.BaseAddress}/drivers?");
            if (!string.IsNullOrEmpty(status))
            {
                endpoint.Append($"status={status}");
            }
            if(cookieCompanyTin != "0076217301" && companyTin==null)
            {
                if (endpoint.Length > 0 && status != null)
                {
                    endpoint.Append('&');
                }
                endpoint.Append($"companyTin={cookieCompanyTin}");
            }

            if (!string.IsNullOrEmpty(companyTin))
            {
                if (endpoint.Length > 0 && status != null)
                {
                    endpoint.Append('&');
                }
                endpoint.Append($"companyTin={companyTin}");
            }

            HttpResponseMessage response = await _client.GetAsync(endpoint.ToString());

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                filteredDrivers = JsonConvert.DeserializeObject<List<Driver>>(data) ?? new List<Driver>();
            }

            return View(filteredDrivers);
        }
        //Used for filtering Drivers based on their status & company TIN - ends here

        //Used for filtering Drivers Live Location based on their status & company TIN - starts here
        [HttpGet("/Driver/LiveFilter/{status?}/{companyTin?}")]
        public async Task<IActionResult> LiveFilter(string? status, string companyTin)
         {
            string? data = null;
            var _client = _httpClientFactory.CreateClient("Delivery");
            if (string.IsNullOrEmpty(status) && string.IsNullOrEmpty(companyTin))
            {
                return RedirectToAction("Index");
            }

            // Build the endpoint based on the provided filters
            var endpoint = new StringBuilder($"{_client.BaseAddress}/drivers?");

            if (status != null && status!="all")
            {
                endpoint.Append($"status={status}");
            }

            if (!string.IsNullOrEmpty(companyTin))
            {
                if (endpoint.Length > 0 && status!=null)
                {
                    endpoint.Append('&');
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
        [HttpGet("/Drivers/FilterCompany/{companyTin}")]
        public async Task<IActionResult> FilterCompany(string? companyTin)
        {
            var _client = _httpClientFactory.CreateClient("Delivery");
            
            List<Driver> filteredDrivers = new ();

            HttpResponseMessage response = await _client.GetAsync($"{_client.BaseAddress}/drivers?companyTin={companyTin}");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                filteredDrivers = JsonConvert.DeserializeObject<List<Driver>>(data) ?? new List<Driver>();
            }
            return View(filteredDrivers);
        }
        //Used for filtering Drivers based on their CompanyTIN - ends here

        //Used for fetching the driver's location regularly - starts here
        [HttpGet("/Driver/LiveLocationByCompany/{companyTin}")]
        public async Task<IActionResult> LiveLocationByCompany(string companyTin)
        {
            var _client = _httpClientFactory.CreateClient("Delivery");
            string? data = null;

            HttpResponseMessage response = await _client.GetAsync($"{_client.BaseAddress}/drivers?companyTin={companyTin}");

            if (response.IsSuccessStatusCode)
            {
                data = await response.Content.ReadAsStringAsync();
            }
            return Ok(data);
        }
        //Used for fetching the driver's location regularly - ends here

        //Driver Details-- Starts Here
        [HttpGet("/driverdetail/{phoneNumber}")]
        public async Task<IActionResult> Details(string phoneNumber)
        {
            var _client = _httpClientFactory.CreateClient("Delivery");
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];
            if (string.IsNullOrWhiteSpace(companyTin) || string.IsNullOrWhiteSpace(companyTin))
            {
                return RedirectToAction("Logout", "Login");
            }
            // Fetch order data
            List<OrderDetail> orders = new ();

            

            // Fetch driver data
            Driver? driver = new ();

            try
            {
                HttpResponseMessage orderResponse = await _client.GetAsync(_client.BaseAddress + $"/orderRequests?companyTin={companyTin}");

                if (orderResponse.IsSuccessStatusCode)
                {
                    string orderData = await orderResponse.Content.ReadAsStringAsync();
                    orders = JsonConvert.DeserializeObject<List<OrderDetail>>(orderData) ?? new List<OrderDetail>();

                }
                HttpResponseMessage response = await _client.GetAsync($"{_client.BaseAddress}/drivers/{phoneNumber}");

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    driver = JsonConvert.DeserializeObject<Driver>(data) ?? new Driver();

                    if(companyTin!= "0076217301" && companyTin!= driver.companyTin)
                    {
                        return NotFound();
                    }

                    if (driver == null)
                    {
                        return NotFound(); // Return a 404 Not Found response if no driver is found.
                    }
                }
            }

            catch (HttpRequestException)
            {
            }

            ViewData["Orders"] = orders;
            return View(driver);
        }
        //Driver Details -- Ends Here

        //Used for fetching the driver's location regularly - starts here
        [HttpGet("/Driver/LiveLocation/{phoneNumber}")]
        public async Task<IActionResult> LiveLocation(string phoneNumber)
        {
            var _client = _httpClientFactory.CreateClient("Delivery");
            string? data = null;

            HttpResponseMessage response = await _client.GetAsync($"{_client.BaseAddress}/drivers/{phoneNumber}");
           
            if (response.IsSuccessStatusCode)
            {
                data = await response.Content.ReadAsStringAsync();
            }
            return Ok(data);
        }
        //Used for fetching the driver's location regularly - ends here

        //Driver Update Page - starts here
        [HttpGet("/updatedriverinfo/{phoneNumber}")]
        public async Task<IActionResult> Update(string phoneNumber)
        {
            var _client = _httpClientFactory.CreateClient("Delivery");
            // Fetch driver data
            UpdateDriverModel? driver = new();

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
        [HttpGet("getDrivers")]
        public async Task<IActionResult> GetAvailableDrivers()
        {
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];
            string uri = companyTin == "0076217301" ? "/drivers" : $"/drivers?companyTin={companyTin}";
            var _client = _httpClientFactory.CreateClient("Delivery");
            List<Driver> drivers = new();

            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress +  uri);

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                drivers = JsonConvert.DeserializeObject<List<Driver>>(data) ?? new List<Driver>();
            }
            return Ok(drivers);
        }

        //Used for updating driver's information - ends here
        [HttpGet("/Review/{phoneNumber}")]
        public async Task<IActionResult> DriverReview(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 2)
                return BadRequest("Invalid phone number.");
            int page = 1;
            string trimmedPhone = phoneNumber.Substring(1); // Remove first character
            var allReviews = new DriverReview();
            List<Reviews> allReviewItems = new();

            try
            {
                while (true)
                {
                    var pageData = await FetchDriverReviewsAsync(page, trimmedPhone);

                    if (pageData == null || pageData.Reviews == null || !pageData.Reviews.Any())
                        break;

                    if (page == 1)
                    {
                        allReviews.Count = pageData.Count;
                        allReviews.Rating = pageData.Rating;
                    }

                    allReviewItems.AddRange(pageData.Reviews);
                    page++;
                }

                allReviews.Reviews = allReviewItems;

                if (!allReviewItems.Any())
                {
                    ViewBag.Error = "No reviews found for this driver.";
                    return View("review", null);
                }

                return View("review", allReviews);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading reviews. Please try again later.";
                return View("review", null);
            }
        }

        [HttpGet("/Driver/fetchReview")]
        public async Task<IActionResult> FetchReview([FromQuery] string phoneNumber, [FromQuery] int page)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 2)
                return BadRequest("Invalid phone number.");

            string trimmedPhone = phoneNumber.Substring(1); // Remove first character
            var result = await FetchDriverReviewsAsync(page, trimmedPhone);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }


        private async Task<DriverReview?> FetchDriverReviewsAsync(int page, string phoneNumber)
        {
            var client = _httpClientFactory.CreateClient("CnetApiBaseUrl");

            var requestPayload = new
            {
                article = phoneNumber,
                page
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestPayload), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("review/get", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    // TODO: log errorContent
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<HulubejeResponse<DriverReview>>(json);

                return apiResponse?.Data;
            }
            catch (Exception ex)
            {
                // TODO: log ex
                return null;
            }
        }
    }
}
