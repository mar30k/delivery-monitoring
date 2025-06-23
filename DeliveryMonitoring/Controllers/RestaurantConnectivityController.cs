using CNET_ERP_V7.WebConstants;
using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;

namespace DeliveryMonitoring.Controllers
{
    [Authorize]
    public class RestaurantConnectivityController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RestaurantConnectivityController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }
        [Route("/deviceControl")]
        public async Task<IActionResult >Index( string date)
        {
            var client = _httpClientFactory.CreateClient("ApiBaseUrl");
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];
            try
            {
                var response = await client.GetAsync($"deviceControl?StartDate={date}&EndDate={date}");

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return View(null);
                }

                var responseData = await response.Content.ReadAsStringAsync();
                var deviceControl = JsonConvert.DeserializeObject<List<DeviceControl>>(responseData);
                if (companyTin != "0076217301")
                {
                    deviceControl = deviceControl?.Where(x => x?.Tin?.ToString() == companyTin?.Trim()).ToList();
                }
                return View(deviceControl);
            }
            catch (Exception ex)
            {
                return View(null);
            }
        }
    }
}
