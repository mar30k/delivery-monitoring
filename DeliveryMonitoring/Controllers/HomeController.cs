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
            var client = _httpClientFactory.CreateClient("Delivery");
            var httpClient = _httpClientFactory.CreateClient("ApiBaseUrl");
            var v7Client = _httpClientFactory.CreateClient("CnetApiBaseUrl");

            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];
            if (string.IsNullOrWhiteSpace(companyTin))
                return RedirectToAction("Logout", "Login");

            string driverUri = companyTin == "0076217301" ? "/drivers" : $"/drivers?companyTin={companyTin}";

            // Declare all variables outside try
            List<Driver> drivers = new();
            List<OrderDetail> orders = new();
            Companies company = new();
            List<DeviceControl> deviceControl = new();
            List<SupervisorsDTO> superVisors = new();
            HulubejeResponse<List<CompletedOrders>> completedOrders = new();
            HulubejeResponse<List<CompletedOrders>> dineInOders = new();
            HulubejeResponse<List<CompletedOrders>> takeAwayOrders = new();

            try
            {
                var startDate = DateTime.Today.ToString("yyyy-MM-dd");
                // Fetch drivers and orders
                drivers = await FetchData<List<Driver>>(client, driverUri) ?? new List<Driver>();
                orders = await FetchData<List<OrderDetail>>(client, $"/orderRequests?companyTin={companyTin}") ?? new List<OrderDetail>();
                company = await FetchData<Companies>(client, $"/companies") ?? new Companies();
                superVisors = await FetchData<List<SupervisorsDTO>>(v7Client, $"auth/getsupervisors") ?? new List<SupervisorsDTO>();
                takeAwayOrders = await FetchData<HulubejeResponse<List<CompletedOrders>>>(v7Client, $"voucher/getordersbytype?type=2076") ?? new HulubejeResponse<List<CompletedOrders>>();
                completedOrders = await FetchData<HulubejeResponse<List<CompletedOrders>>>(v7Client, $"voucher/getcompletedorders") ?? new HulubejeResponse<List<CompletedOrders>>();
                dineInOders = await FetchData<HulubejeResponse<List<CompletedOrders>>>(v7Client, $"voucher/getordersbytype?type=3203") ?? new HulubejeResponse<List<CompletedOrders>>();
                deviceControl = (await FetchData<List<DeviceControl>>(httpClient, $"deviceControl?StartDate={startDate}&EndDate={startDate}") ?? new List<DeviceControl>()).ToList().Where(s => !s.Note.StartsWith("09")).ToList();
            }
            catch (HttpRequestException)
            {
                // Optionally log the error
            }
            var latestByTinAndBranch = deviceControl?
                .Where(d => d.TimeStamp.HasValue) // Ensure TimeStamp is not null
                .GroupBy(d => new { d.Tin, d.BranchName, d.DeviceName }) // Group by Tin , BranchName and DeviceName
                .Select(g => g.OrderByDescending(d => d.TimeStamp).First()) // Get the one with latest TimeStamp
                .ToList();
            if (companyTin != "0076217301")
            {
                latestByTinAndBranch = latestByTinAndBranch?
                    .Where(x => x?.Tin?.ToString() == companyTin?.Trim())
                    .ToList();
            }
            var viewModel = new HomeViewModel
            {
                Drivers = drivers,
                Orders = orders,
                Comps = company,
                CompanyTin = companyTin,
                DeviceControl = latestByTinAndBranch,
                Supervisors = superVisors,
                TakeAwayOrders = takeAwayOrders,
                CompletedOrders = completedOrders,
                DineInOrders = dineInOders
            };
            
            return View(viewModel);
        }

        private async Task<T?> FetchData<T>(HttpClient client, string uri)
        {
            HttpResponseMessage response = await client.GetAsync(client.BaseAddress + uri);
            if (!response.IsSuccessStatusCode) return default;

            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
