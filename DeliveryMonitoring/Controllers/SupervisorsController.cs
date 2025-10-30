using CNET_ERP_V7.WebConstants;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services;
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
        public SupervisorsController(
            IHttpContextAccessor httpContextAccessor,
            IApiRequestService apiRequestService)
        {
            _httpContextAccessor = httpContextAccessor;
            _apiRequestService = apiRequestService;
        }
        [Route("/supervisors")]
        public async Task<IActionResult>Index()
        {

            List<OrderDetail>? orders = new();
            List<SupervisorsDTO>? superVisors = new();
            HulubejeResponse<List<CompletedOrders>>? completedOrders = new();
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];
            if (string.IsNullOrWhiteSpace(companyTin) || string.IsNullOrWhiteSpace(companyTin))
            {
                return RedirectToAction("Logout", "Login");
            }

            orders = await _apiRequestService.GetOrderRequestsAsync();
            if ( companyTin== "0076217301" )
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
                Supervisors = companyTin== "0076217301" ?superVisors : new List<SupervisorsDTO>(),
                CompletedOrders = completedOrders?.Data?.Where(o=>o.SupervisorPhoneNumber!=null).ToList() ?? new List<CompletedOrders>()
            };
            return View(orderViewModel);
        }
    }
}
