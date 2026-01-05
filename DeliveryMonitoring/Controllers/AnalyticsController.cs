using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;

namespace DeliveryMonitoring.Controllers
{
    [Authorize]
    public class AnalyticsController : Controller
    {
        private readonly IApiRequestService _apiRequestService;
        private readonly AuthenticationManager _authenticationManager;
        public AnalyticsController(
             IApiRequestService apiRequestService,
            AuthenticationManager authenticationManager)
        {
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

            var company = await _apiRequestService.GetCompaniesAsync();
            var superVisors = await _apiRequestService.GetSupervisorsAsync();
            var viewModel = new HomeViewModel
            {
                Drivers = new List<Driver>(),
                Orders = new List<OrderDetail>(),
                Comps = company,
                CompanyTin = companyTin,
                DeviceControl = new List<DeviceControl>(),
                Supervisors = new List<SupervisorsDTO>()
            };
            return View(viewModel);
        }
    }
}
