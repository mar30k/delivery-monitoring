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
            var _client = _httpClientFactory.CreateClient("CnetApiBaseUrl");
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];
            try
            {
                var completedResult = await FetchCompletedOrders(_client);

                var purposeResponse = await _client.GetAsync("delivery/getpurpose");
                if (!purposeResponse.IsSuccessStatusCode)
                {
                    var errorContent = await purposeResponse.Content.ReadAsStringAsync();
                    ViewBag.ErrorMessage = $"Failed to retrieve completed orders. Server responded with: {errorContent}";
                }

                
                var purposeResponseData = await purposeResponse.Content.ReadAsStringAsync();
                var purposeResult = JsonConvert.DeserializeObject<Dictionary<int, string>>(purposeResponseData);
                var dineInResponse = await _client.GetAsync("voucher/getordersbytype?type=3203");
                if (!dineInResponse.IsSuccessStatusCode)
                {
                    var errorContent = await dineInResponse.Content.ReadAsStringAsync();
                    ViewBag.ErrorMessage = $"Failed to retrieve completed orders. Server responded with: {errorContent}";
                }

                
                var dineInResponseData = await dineInResponse.Content.ReadAsStringAsync();
                var dineIneResult = JsonConvert.DeserializeObject<HulubejeResponse<List<CompletedOrders>>>(dineInResponseData) ?? new HulubejeResponse<List<CompletedOrders>>();

                var takeAwayResponse = await _client.GetAsync("voucher/getordersbytype?type=2076");
                if (!takeAwayResponse.IsSuccessStatusCode)
                {
                    var errorContent = await takeAwayResponse.Content.ReadAsStringAsync();
                    ViewBag.ErrorMessage = $"Failed to retrieve completed orders. Server responded with: {errorContent}";
                }

                
                var takeAwayResponseData = await takeAwayResponse.Content.ReadAsStringAsync();
                var takeAwayResult = JsonConvert.DeserializeObject<HulubejeResponse<List<CompletedOrders>>>(takeAwayResponseData) ?? new HulubejeResponse<List<CompletedOrders>>();
                if (companyTin != "0076217301")
                {
                    completedResult.Data = completedResult.Data?.Where(order => order.Tin == companyTin).ToList();
                    dineIneResult.Data = dineIneResult.Data?.Where(order => order.Tin == companyTin).ToList();
                    takeAwayResult.Data = takeAwayResult.Data?.Where(order => order.Tin == companyTin).ToList();
                }
                
                var CompletedOrdersViewModel = new CompletedOrdersViewModel
                {
                    CompletedOrders = completedResult,
                    DineInOders = dineIneResult,
                    TakeAwayOrders = takeAwayResult,
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

        [HttpGet("getCompletedOrders")]
        public async Task<IActionResult> GetCompletedOrdersApi()
        {
            var client = _httpClientFactory.CreateClient("CnetApiBaseUrl");
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];

            var result = await FetchCompletedOrders(client);
            if (result == null || result.Data == null)
            {
                return NotFound("Failed to retrieve or parse completed orders.");
            }

            if (companyTin != "0076217301")
            {
                result.Data = result.Data.Where(order => order.Tin == companyTin).ToList();
            }
            foreach (var item in result.Data)
            {
                item.RequestCreatedAtString = item.RequestCreatedAt.ToString("yyyy-MM-dd hh:mm:ss");
            }
            return Ok(result);
        }


        private async Task<HulubejeResponse<List<CompletedOrders>>> FetchCompletedOrders(HttpClient client)
        {
            try
            {
                var response = await client.GetAsync("voucher/getcompletedorders");
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<HulubejeResponse<List<CompletedOrders>>>(responseData);
            }
            catch (Exception)
            {
                return null;
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
        [HttpGet("getDeliveryActivity")]
        public async Task<IActionResult> GetDeliveryActivity(string voucherCode, string companyCode)
        {
            var client = _httpClientFactory.CreateClient("CnetApiBaseUrl");

            try
            {
                var response = await client.GetAsync($"driveractivity/get?companyCode={companyCode}&voucherCode={voucherCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return BadRequest(errorContent);
                }

                var responseData = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<HulubejeResponse<Activities>>(responseData);
                Response.Headers["Cache-Control"] = "public, max-age=10"; // cache for 5 minutes
                Response.Headers["Vary"] = "Accept-Encoding";
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message[0]);
            }
        }
    }
}
