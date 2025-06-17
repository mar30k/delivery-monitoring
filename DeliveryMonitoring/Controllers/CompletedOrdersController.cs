using CNET_ERP_V7.WebConstants;
using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace DeliveryMonitoring.Controllers
{
    [Authorize]
    [Route("/CompletedOrders")]
    public class CompletedOrdersController : Controller
    {
        private IHttpClientFactory _httpClientFactory;
        private IHttpContextAccessor _httpContextAccessor;
        public CompletedOrdersController(IHttpClientFactory httpClientFactory , IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("CnetApiBaseUrl");
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];
            try
            {
                var response = await client.GetAsync("voucher/getcompletedorders");
                var purposeResponse = await client.GetAsync("delivery/getpurpose");
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ViewBag.ErrorMessage = $"Failed to retrieve completed orders. Server responded with: {errorContent}";
                    return View(null); // Or an empty list, if the view expects a model
                }
                if (!purposeResponse.IsSuccessStatusCode)
                {
                    var errorContent = await purposeResponse.Content.ReadAsStringAsync();
                    ViewBag.ErrorMessage = $"Failed to retrieve completed orders. Server responded with: {errorContent}";
                }

                var responseData = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<HulubejeResponse<List<CompletedOrders>>>(responseData);
                if (companyTin != "0076217301" && result != null && result.Data != null && result.Data.Count > 0)
                {
                    result.Data = result.Data.Where(order => order.Tin == companyTin).ToList();
                }
                var purposeResponseData = await purposeResponse.Content.ReadAsStringAsync();
                var purposeResult = JsonConvert.DeserializeObject<Dictionary<int, string>>(purposeResponseData);
                if (result == null)
                {
                    ViewBag.ErrorMessage = "Failed to parse completed orders response.";
                    return View(null);
                }
                var CompletedOrdersViewModel = new CompletedOrdersViewModel
                {
                    CompletedOrders = result,
                    PurposeOptions = purposeResult
                };
                return View(CompletedOrdersViewModel); 
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Unable to connect to the service. Please try again later.";
                return View(null);
            }
        }
        [HttpPost("savenote")]
        public async Task<IActionResult> SaveOrderReview([FromBody] CompletedOrders request)
        {
            var client = _httpClientFactory.CreateClient("CnetApiBaseUrl");

            try
            {
                var response = await client.GetAsync($"delivery/savenote?voucherCode={request.VoucherCode}&note={request.Note}&purpose={request.Purpose}" );

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return BadRequest(errorContent);
                }

                var responseData = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Boolean>(responseData);

                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message[0]);
            }
        }
    }
}
