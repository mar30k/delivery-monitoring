using CNET_ERP_V7.WebConstants;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;

namespace DeliveryMonitoring.Controllers
{
    public class AnalyticsController : Controller
    {
        private IHttpContextAccessor _httpContextAccessor;
        private readonly IApiRequestService _apiRequestService;
        public AnalyticsController(IHttpContextAccessor httpContextAccessor, IApiRequestService apiRequestService)
        {
            _httpContextAccessor = httpContextAccessor;
            _apiRequestService = apiRequestService;
        }
        [Route("/Analytics")]
        public async Task<IActionResult> Index()
        {
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];
            if (string.IsNullOrWhiteSpace(companyTin))
                return RedirectToAction("Logout", "Login");

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
                drivers = await _apiRequestService.GetAvailableDriversAsync();
                orders = await _apiRequestService.GetOrderRequestsAsync();
                company = await _apiRequestService.GetCompaniesAsync();
                superVisors = await _apiRequestService.GetSupervisorsAsync();
                deviceControl = await _apiRequestService.GetDeviceControlAsync(startDate);
                takeAwayOrders = await _apiRequestService.GetOrdersByTypeAsync(2076);
                completedOrders = await _apiRequestService.GetCompletedOrdersAsync();
                dineInOders = await _apiRequestService.GetOrdersByTypeAsync(3203);
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
            var result = latestByTinAndBranch?
                .Where(s => string.IsNullOrEmpty(s.Note) || !s.Note.StartsWith("09"))
                .ToList();
            var viewModel = new HomeViewModel
            {
                Drivers = drivers,
                Orders = orders,
                Comps = company,
                CompanyTin = companyTin,
                DeviceControl = result,
                Supervisors = superVisors,
                CompletedOrders = completedOrders,
                DineInOrders = dineInOders,
                TakeAwayOrders = takeAwayOrders
            };
            return View(viewModel);
        }
    }
}
