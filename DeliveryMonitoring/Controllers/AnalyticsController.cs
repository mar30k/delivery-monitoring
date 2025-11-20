using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;

namespace DeliveryMonitoring.Controllers
{
    public class AnalyticsController : Controller
    {
        private IHttpContextAccessor _httpContextAccessor;
        private readonly IApiRequestService _apiRequestService;
        private readonly AuthenticationManager _authenticationManager;
        private const string AdminCompanyTin = AppConstants.Company.AdminTin;
        public AnalyticsController(IHttpContextAccessor httpContextAccessor
            , IApiRequestService apiRequestService,
            AuthenticationManager authenticationManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _apiRequestService = apiRequestService;
            _authenticationManager = authenticationManager;
        }
        [Route("/analytics")]
        public async Task<IActionResult> Index()
        {
            var companyTin = _authenticationManager.GetSecureCookie(
                CNET_WebConstantes.IdentificationCookie);
            if (string.IsNullOrWhiteSpace(companyTin))
                return RedirectToAction("Logout", "Login");

            // Declare all variables outside try
            List<Driver> drivers = new();
            List<OrderDetail> orders = new();
            Companies company = new();
            List<DeviceControl> deviceControl = new();
            List<SupervisorsDTO> superVisors = new();
            try
            {
                var startDate = DateTime.Today.ToString("yyyy-MM-dd");
                // Fetch drivers and orders
                drivers = new List<Driver>();
                orders = await _apiRequestService.GetOrderRequestsAsync();
                company = await _apiRequestService.GetCompaniesAsync();
                superVisors = await _apiRequestService.GetSupervisorsAsync();
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
            if (companyTin != AdminCompanyTin)
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
                Supervisors = superVisors
            };
            return View(viewModel);
        }
    }
}
