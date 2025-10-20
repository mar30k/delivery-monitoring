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
            var v7Client = _httpClientFactory.CreateClient("CnetApiBaseUrl");

            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];
            if (string.IsNullOrWhiteSpace(companyTin))
                return RedirectToAction("Logout", "Login");

            string driverUri = companyTin == "0076217301" ? "/drivers" : $"/drivers?companyTin={companyTin}";

            // Declare all variables outside try
            List<Driver> drivers = new();
            List<OrderDetail> orders = new();
            Companies company = new();
            List<SupervisorsDTO> superVisors = new();
            
            try
            {
                var startDate = DateTime.Today.ToString("yyyy-MM-dd");
                // Fetch drivers and orders
                drivers = await FetchData<List<Driver>>(client, driverUri) ?? new List<Driver>();
                orders = await FetchData<List<OrderDetail>>(client, $"/orderRequests?companyTin={companyTin}") ?? new List<OrderDetail>();
                company = await FetchData<Companies>(client, $"/companies") ?? new Companies();
                superVisors = await FetchData<List<SupervisorsDTO>>(v7Client, $"auth/getsupervisors") ?? new List<SupervisorsDTO>();
            }
            catch (HttpRequestException)
            {
                // Optionally log the error
            }
            var viewModel = new HomeViewModel
            {
                Drivers = drivers,
                Orders = orders,
                Comps = company,
                CompanyTin = companyTin,
                Supervisors = superVisors,
            };

            return View(viewModel);
        }

        private static async Task<T?> FetchData<T>(HttpClient client, string uri)
        {
            try
            {
                var response = await client.GetAsync(client.BaseAddress + uri);
                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"[FetchData] Request to '{uri}' failed with status: {response.StatusCode}");
                    return default;
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[FetchData] Exception while fetching '{uri}': {ex.Message}");
                return default;
            }
        }


        [HttpGet("/GetChartData")]
        public async Task<IActionResult> GetChartData([FromQuery] string type)
        {
            var v7Client = _httpClientFactory.CreateClient("CnetApiBaseUrl");
            var today = DateTime.Today;

            string? uri = type?.ToLower() switch
            {
                "takeaway" => "voucher/getordersbytype?type=2076",
                "delivery" => "voucher/getcompletedorders",
                "dinein" => "voucher/getordersbytype?type=3203",
                _ => null
            };

            if (uri is null)
                return BadRequest("Invalid type parameter. Use takeaway, delivery, or dinein.");

            var response = await FetchData<HulubejeResponse<List<CompletedOrders>>>(v7Client, uri)
                          ?? new HulubejeResponse<List<CompletedOrders>>();

            var count = response.Data?.Count(x => x.RequestCreatedAt.Date == today) ?? 0;
            var total = response.Data?.Where(x => x.RequestCreatedAt.Date == today).Sum(x => x.TotalAmount) ?? 0;

            return Ok(new { count, total });
        }
    }

}
