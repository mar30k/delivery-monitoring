using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
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
        public async Task<IActionResult> Index()
        {
            var _client = _httpClientFactory.CreateClient("Delivery");

            // Call the first endpoint to get company TINs
            HttpResponseMessage companiesResponse = await _client.GetAsync($"{_client.BaseAddress}/companies");
            string data = await companiesResponse.Content.ReadAsStringAsync();
            var companiesModel = JsonConvert.DeserializeObject<Companies>(data);

            // Call the second endpoint for each company TIN to get detailed information
            var companyDetailsList = new List<Company>();
            foreach (var companyTins in companiesModel.companyTins)
            {
                HttpResponseMessage companyDetailsResponse = await _client.GetAsync($"{_client.BaseAddress}/companies/{companyTins}");
                string data2 = await companyDetailsResponse.Content.ReadAsStringAsync();
                var companyDetailsModel = JsonConvert.DeserializeObject<Company>(data2);
                companyDetailsList.Add(companyDetailsModel);
            }

            // Create the CompanyIndex view model
            var viewModel = new CompanyIndex
            {
                Companies = companiesModel,
                company = companyDetailsList
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
