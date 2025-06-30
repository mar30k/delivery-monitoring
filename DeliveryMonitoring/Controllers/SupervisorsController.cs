using CNET_ERP_V7.WebConstants;
using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DeliveryMonitoring.Controllers
{
    [Authorize]
    public class SupervisorsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SupervisorsController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }
        [Route("/supervisors")]
        public async Task<IActionResult>Index()
        {
            var _client = _httpClientFactory.CreateClient("Delivery");
            var _V7client = _httpClientFactory.CreateClient("CnetApiBaseUrl");
            List<OrderDetail>? orders = new();
            List<SupervisorsDTO>? superVisors = new();
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];
            if (string.IsNullOrWhiteSpace(companyTin) || string.IsNullOrWhiteSpace(companyTin))
            {
                return RedirectToAction("Logout", "Login");
            }

            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + $"/orderRequests?companyTin={companyTin}");
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                orders = JsonConvert.DeserializeObject<List<OrderDetail>>(data);
            }
            if( companyTin== "0076217301" )
            {
                HttpResponseMessage getsupervisors = await _V7client.GetAsync(_V7client.BaseAddress + "auth/getsupervisors");


                if (getsupervisors.IsSuccessStatusCode)
                {
                    string data = await getsupervisors.Content.ReadAsStringAsync();
                    superVisors = JsonConvert.DeserializeObject<List<SupervisorsDTO>>(data);
                }
            }
            
            var orderViewModel = new OrderViewModel
            {
                OrderDetail = orders,
                Supervisors = companyTin== "0076217301" ?superVisors : new List<SupervisorsDTO>()
            };
            return View(orderViewModel);
        }
    }
}
