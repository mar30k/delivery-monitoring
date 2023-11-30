using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static NuGet.Packaging.PackagingConstants;

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
        public async Task<IActionResult> Index(string companyTins)
        {
            var _client = _httpClientFactory.CreateClient("Delivery");

            Companies company1 = null;
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"{_client.BaseAddress}/companies");

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    company1 = JsonConvert.DeserializeObject<Companies>(data);
                }

                if (company1 == null)
                {
                    return NotFound(); // Return a 404 Not Found response if no driver is found.
                }                
            }
            catch (HttpRequestException)
            {
                return StatusCode(500); // Handle exception with a 500 Internal Server Error
            }

            Company company2 = null;
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"{_client.BaseAddress}/companies/{companyTins}");

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    company2 = JsonConvert.DeserializeObject<Company>(data);
                }                               
            }
            catch (HttpRequestException)
            {
                return StatusCode(500); // Handle exception with a 500 Internal Server Error
            }

            // Create HomeViewModel
            var viewModel = new CompanyIndex
            {
                Companies =company1,
                company = company2                
            };

            return View(viewModel);
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

                if (company == null || company.error != null)
                {
                    // Return a view indicating that company details are not found
                    return View("Error");
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
