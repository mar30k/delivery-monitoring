using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services.Api;
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
        private IHttpContextAccessor _httpContextAccessor;
        private readonly IApiRequestService _apiRequestService;
        private readonly AuthenticationManager _authenticationManager;
        private string CompanyTin =>_authenticationManager.GetSecureCookie(
                CNET_WebConstantes.IdentificationCookie) ?? string.Empty;
        private const string AdminComanyTin = AppConstants.Company.AdminTin;
        public HomeController(
            IHttpContextAccessor httpContextAccessor,
            IApiRequestService apiRequestService,
            AuthenticationManager authenticationManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _apiRequestService = apiRequestService;
            _authenticationManager = authenticationManager;
        }
        [Route("/")]
        public async Task<IActionResult> Index()
        {
            
            if (string.IsNullOrWhiteSpace(CompanyTin))
                return RedirectToAction("Logout", "Login");

            try
            {
                var viewModel = new HomeViewModel
                {
                    Drivers = new List<Driver>(),
                    Orders = await _apiRequestService.GetOrderRequestsAsync(),
                    Comps = await _apiRequestService.GetCompaniesAsync(),
                    Supervisors = await _apiRequestService.GetSupervisorsAsync(),
                    CompanyTin = CompanyTin
                };

                return View(viewModel);
            }
            catch (HttpRequestException)
            {
                return View(new HomeViewModel { CompanyTin = CompanyTin });
            }
        }

        [HttpGet("/GetChartData")]
        public async Task<IActionResult> GetChartData([FromQuery] ReportByOrderType type)
        {
            var response = type == ReportByOrderType.Delivery
                ? await _apiRequestService.GetCompletedOrdersAsync()
                : await _apiRequestService.GetOrdersByTypeAsync(
                    type == ReportByOrderType.Takeaway
                        ? (int)DeliveryOrderTypes.PickUpAtBranch
                        : (int)DeliveryOrderTypes.InHouseDining
                );

            if (response?.Data == null || !response.Data.Any())
                return Ok(new { count = 0, total = 0m });

            var filteredData = (CompanyTin == AdminComanyTin)
                ? response.Data
                : response.Data.Where(r => r.Tin == CompanyTin);

            var todayOrders = filteredData.Where(x => x.RequestCreatedAt.Date == DateTime.Today).ToList();

            return Ok(new
            {
                count = todayOrders.Count,
                total = todayOrders.Sum(x => x.TotalAmount)
            });
        }
    }

}
