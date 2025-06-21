using CNET_ERP_V7.WebConstants;
using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

namespace DeliveryMonitoring.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        //HttpClient Setup starts here
        private readonly IHttpClientFactory _httpClientFactory;
        private IHttpContextAccessor _httpContextAccessor;

        public HomeController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }
        //HttpClient Setup ends here

        //This returns the view for Home/Index
        [Route("/")]
        public async Task<IActionResult> Index()
        {
            var _client = _httpClientFactory.CreateClient("Delivery");
            var _httpClient = _httpClientFactory.CreateClient("ApiBaseUrl");
            var _v7client = _httpClientFactory.CreateClient("CnetApiBaseUrl");

            // Fetch driver data
            List<Driver> drivers = new();
            List<SupervisorsDTO>? superVisors = new();

            HttpResponseMessage driverResponse = await _client.GetAsync(_client.BaseAddress + "/drivers");

            if (driverResponse.IsSuccessStatusCode)
            {
                string driverData = await driverResponse.Content.ReadAsStringAsync();
                drivers = JsonConvert.DeserializeObject<List<Driver>>(driverData) ?? new List<Driver>();
                
            }

            // Fetch order data
            List<OrderDetail> orders = new ();
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];
            if (string.IsNullOrWhiteSpace(companyTin) || string.IsNullOrWhiteSpace(companyTin)) {
                return RedirectToAction("Logout", "Login");
            }

            HttpResponseMessage orderResponse = await _client.GetAsync(_client.BaseAddress + $"/orderRequests?companyTin={companyTin}");

            if (orderResponse.IsSuccessStatusCode)
            {
                string orderData = await orderResponse.Content.ReadAsStringAsync();
                orders = JsonConvert.DeserializeObject<List<OrderDetail>>(orderData) ?? new List<OrderDetail>();
            }

            Companies company = new ();
            var deviceControl = new List<DeviceControl>();

            try
            {
                var startDate = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");
                var endDate = DateTime.Today.ToString("yyyy-MM-dd");

                HttpResponseMessage response = await _client.GetAsync($"{_client.BaseAddress}/companies");
                HttpResponseMessage getsupervisors = await _v7client.GetAsync(_v7client.BaseAddress + $"auth/getsupervisors");


                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    company = JsonConvert.DeserializeObject<Companies>(data) ?? new Companies();
                }
                if (getsupervisors.IsSuccessStatusCode)
                {
                    string data = await getsupervisors.Content.ReadAsStringAsync();
                    superVisors = JsonConvert.DeserializeObject<List<SupervisorsDTO>>(data);
                }

                if (companyTin!= "0076217301" && company?.companyTins?.Contains(companyTin) == true)
                {
                    company.companyTins = new List<string> { companyTin };
                }
                HttpResponseMessage deviceControlResponse = await _httpClient.GetAsync($"deviceControl?StartDate={startDate}&EndDate={endDate}");

                if (deviceControlResponse.IsSuccessStatusCode)
                {
                    string data = await deviceControlResponse.Content.ReadAsStringAsync();
                    deviceControl = JsonConvert.DeserializeObject<List<DeviceControl>>(data) ?? new List<DeviceControl>();
                }
                if (companyTin != "0076217301")
                {
                    deviceControl = deviceControl.Where(x => x.Tin.ToString() == companyTin.Trim()).ToList();
                }
                deviceControl = deviceControl?.Where(x => !x.Note.StartsWith("09")).ToList();
            }
            catch (HttpRequestException)
            {
            }

            // Create HomeViewModel
            var viewModel = new HomeViewModel
            {
                Drivers = drivers,
                Orders = orders,
                Comps = company,
                CompanyTin = companyTin,
                DeviceControl = deviceControl,
                Supervisors = superVisors
            };

            return View(viewModel);
        }
        //This is the end of the code that returns the view for Home/Index Page
    }
}
