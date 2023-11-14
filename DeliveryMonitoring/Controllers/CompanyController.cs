using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DeliveryMonitoring.Controllers
{
    public class CompanyController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public CompanyController(IHttpClientFactory httpClientFactory)
        {
           _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var _client = _httpClientFactory.CreateClient("Delivery");
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

                return View(company);
            }
            catch (HttpRequestException)
            {
                return StatusCode(500); // Handle exception with a 500 Internal Server Error
            }
        }

        
        [HttpGet("/Company/Details/{companyTins}")]
        public async Task<IActionResult> Details(string companyTins)
        {
            var _client = _httpClientFactory.CreateClient("Delivery");
            Company company = null;

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"{_client.BaseAddress}/companies/{companyTins}");

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    company = JsonConvert.DeserializeObject<Company>(data);
                }

                if (company == null)
                {
                    return NotFound(); // Return a 404 Not Found response if no driver is found.
                }

                return View(company);
            }
            catch (HttpRequestException)
            {
                return StatusCode(500); // Handle exception with a 500 Internal Server Error
            }
        }
        
    }
}
