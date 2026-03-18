using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services.Api;
using DeliveryMonitoring.Services.Orders;
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
        private readonly ICompletedOrdersService _orderService;
        private string CompanyTin =>_authenticationManager.GetSecureCookie(
                CNET_WebConstantes.IdentificationCookie) ?? string.Empty;
        private const string AdminComanyTin = AppConstants.Company.AdminTin;
        public HomeController(
            IHttpContextAccessor httpContextAccessor,
            IApiRequestService apiRequestService,
            AuthenticationManager authenticationManager,
            ICompletedOrdersService orderService)
        {
            _httpContextAccessor = httpContextAccessor;
            _apiRequestService = apiRequestService;
            _authenticationManager = authenticationManager;
            _orderService = orderService;
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
                    Comps = await _apiRequestService.GetCompaniesAsync(),
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
        public async Task<IActionResult> GetChartData()
        {
            var queryParams = new OrderQueryParams
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today
            };

            var response = await _orderService.GetAllOrdersAsync(queryParams);
            var orders = response.Data ?? new List<CompletedOrders>();

            var allTypes = new[]
            {
                new { TableId = AppConstants.TableIds.TakeAway, Label = "Takeaway" },
                new { TableId = AppConstants.TableIds.Delivery, Label = "Delivery" },
                new { TableId = AppConstants.TableIds.DineIn, Label = "Dine-in" },
                new { TableId = AppConstants.TableIds.ScheduledDelivery, Label = "Scheduled Delivery" },
                new { TableId = AppConstants.TableIds.ScheduledPickUp, Label = "Scheduled Takeaway" }
            };

            var chartData = allTypes.Select((type, index) =>
            {
                var typeOrders = orders.Where(o => o.TableId == type.TableId);
                return new
                {
                    tableId = type.TableId,        // camelCase
                    label = type.Label,
                    count = typeOrders.Count(),    // camelCase
                    total = typeOrders.Sum(o => o.TotalAmount), // camelCase
                    index
                };
            }).ToList();

            return Ok(chartData);
        }
        [AllowAnonymous]
        [HttpGet("/serverTime")]
        public IActionResult GetServerTime()
        {
            return Ok(new
            {
                serverUtcNow = DateTime.UtcNow,
                serverLocalNow = DateTime.Now
            });
        }
    }

}
 