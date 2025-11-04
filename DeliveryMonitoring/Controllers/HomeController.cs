using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services;
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
                    Drivers = await _apiRequestService.GetAvailableDriversAsync(),
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
        public async Task<IActionResult> GetChartData([FromQuery] string type)
        {
            var today = DateTime.Today;
            if (type is null)
                return BadRequest("Invalid type parameter. Use takeaway, delivery, or dinein.");

            var response = type?.ToLower()== "delivery" ? await _apiRequestService.GetCompletedOrdersAsync() 
                : await _apiRequestService.GetOrdersByTypeAsync(type?.ToLower() == "takeaway" ? (int)DeliveryOrderTypes.PickUpAtBranch : (int)DeliveryOrderTypes.InHouseDining);

            var count = response.Data?.Count(x => x.RequestCreatedAt.Date == today) ?? 0;
            var total = response.Data?.Where(x => x.RequestCreatedAt.Date == today).Sum(x => x.TotalAmount) ?? 0;

            return Ok(new { count, total });
        }
    }

}
