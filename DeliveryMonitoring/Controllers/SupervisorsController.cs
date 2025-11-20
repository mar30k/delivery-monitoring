using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DeliveryMonitoring.Controllers
{
    [Authorize]
    public class SupervisorsController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IApiRequestService _apiRequestService;
        private readonly AuthenticationManager _authenticationManager;
        private const string AdminCompanyTin = AppConstants.Company.AdminTin;
        public SupervisorsController(
            IHttpContextAccessor httpContextAccessor,
            IApiRequestService apiRequestService,
            AuthenticationManager authenticationManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _apiRequestService = apiRequestService;
            _authenticationManager = authenticationManager;
        }
        [Route("/supervisors")]
        public async Task<IActionResult>Index()
        {

            List<OrderDetail>? orders = new();
            List<SupervisorsDTO>? superVisors = new();
            HulubejeResponse<List<CompletedOrders>>? completedOrders = new();
            var companyTin = _authenticationManager.GetSecureCookie(
                CNET_WebConstantes.IdentificationCookie);
            if (string.IsNullOrWhiteSpace(companyTin) || string.IsNullOrWhiteSpace(companyTin))
            {
                return RedirectToAction("Logout", "Login");
            }

            orders = await _apiRequestService.GetOrderRequestsAsync();
            if ( companyTin== AdminCompanyTin )
            {
                superVisors = await _apiRequestService.GetSupervisorsAsync();
                completedOrders = await _apiRequestService.GetCompletedOrdersAsync();

                foreach (var supervisor in superVisors ?? new List<SupervisorsDTO>())
                {
                    supervisor.TotalSupervisedOrders = completedOrders?.Data?.Count(x => x.SupervisorPhoneNumber == supervisor.UserName) ?? 0;
                }
            }
            
            var orderViewModel = new OrderViewModel
            {
                OrderDetail = orders,
                Supervisors = companyTin== AdminCompanyTin ?superVisors : new List<SupervisorsDTO>(),
                CompletedOrders = completedOrders?.Data?.Where(o=>o.SupervisorPhoneNumber!=null).ToList() ?? new List<CompletedOrders>()
            };
            return View(orderViewModel);
        }
    }
}
