using CNET_ERP_V7.WebConstants;
using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using static NuGet.Packaging.PackagingConstants;

namespace DeliveryMonitoring.Controllers
{
    [Authorize]
    public class CompanyController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;
        public CompanyController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
           _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        [Route("Companies")]
        public async Task<IActionResult> Index()
        {
            var _client = _httpClientFactory.CreateClient("Delivery");
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];
            if (string.IsNullOrWhiteSpace(companyTin) || string.IsNullOrWhiteSpace(companyTin))
            {
                return RedirectToAction("Logout", "Login");
            }
                // Call the first endpoint to get company TINs
            HttpResponseMessage companiesResponse = await _client.GetAsync($"{_client.BaseAddress}/companies");
            string data = await companiesResponse.Content.ReadAsStringAsync();
            var companiesModel = JsonConvert.DeserializeObject<Companies>(data);

            // Call the second endpoint for each company TIN to get detailed information
            var companyDetailsList = new List<Company>();
            if(companyTin != "0076217301")
            {
                HttpResponseMessage companyDetailsResponse = await _client.GetAsync($"{_client.BaseAddress}/companies/{companyTin}");
                if (companyDetailsResponse.IsSuccessStatusCode)
                {
                    string data2 = await companyDetailsResponse.Content.ReadAsStringAsync();
                    var companyDetailsModel = JsonConvert.DeserializeObject<Company>(data2);
                    companyDetailsList.Add(companyDetailsModel);
                }

                if (companiesModel?.companyTins?.Contains(companyTin) == true)
                {
                    companiesModel.companyTins = new List<string> { companyTin };
                }

                return View(new CompanyIndex
                {
                    Companies = companiesModel ?? new Companies(),
                    company = companyDetailsList
                });
            }
            foreach (var companyTins in companiesModel?.companyTins ?? new List<string>())
            {
                HttpResponseMessage companyDetailsResponse = await _client.GetAsync($"{_client.BaseAddress}/companies/{companyTins}");
                if (companyDetailsResponse.IsSuccessStatusCode)
                {
                    string data2 = await companyDetailsResponse.Content.ReadAsStringAsync();
                    var companyDetailsModel = JsonConvert.DeserializeObject<Company>(data2);
                    companyDetailsList.Add(companyDetailsModel);
                }
            }

            // Create the CompanyIndex view model
            var viewModel = new CompanyIndex
            {
                Companies = companiesModel ?? new Companies(),
                company = companyDetailsList
            };

            return View(viewModel);
        }

        
        [HttpGet("/Company/{companyTins}")]
        public async Task<IActionResult> Details(string companyTins)
        {
            var _client = _httpClientFactory.CreateClient("Delivery");
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];
            if (string.IsNullOrWhiteSpace(companyTin) || string.IsNullOrWhiteSpace(companyTin))
            {
                return RedirectToAction("Logout", "Login");
            }
            else if (companyTin != "0076217301") { return RedirectToAction("index", "home"); }
            Company? company = null;

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"{_client.BaseAddress}/companies/{companyTins}");

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    company = !string.IsNullOrEmpty(data) ? JsonConvert.DeserializeObject<Company>(data) : null;
                }

                if (company == null )
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
