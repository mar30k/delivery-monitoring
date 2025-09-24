using CNET_ERP_V7.WebConstants;
using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DeliveryMonitoring.Controllers
{
    public class CompletedOrdersReportController : Controller
    {
        private IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;
        public CompletedOrdersReportController(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }
        [Route("/report")]
        public async Task<IActionResult> Index(string t = "")
        {
            var _client = _httpClientFactory.CreateClient("CnetApiBaseUrl");
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];
            string url = "voucher/getcompletedorders";
            if (t.ToLower() == "dinein")
            {
                url = "voucher/getordersbytype?type=3203";
            }
            else if (t.ToLower() == "takeaway")
            {
                url = "voucher/getordersbytype?type=2076";
            }
            var purposeResponse = await _client.GetAsync("delivery/getpurpose");
            if (!purposeResponse.IsSuccessStatusCode)
            {
                var errorContent = await purposeResponse.Content.ReadAsStringAsync();
                ViewBag.ErrorMessage = $"Failed to retrieve completed orders. Server responded with: {errorContent}";
            }
            var purposeResponseData = await purposeResponse.Content.ReadAsStringAsync();
            var completedResult = await FetchCompletedOrders(_client, url);
            if (companyTin != "0076217301")
            {
                if (completedResult != null)
                    completedResult.Data = completedResult.Data?.Where(order => order.Tin == companyTin).ToList();
            }
            var viewModel = new CompletedOrdersViewModel
            {
                Type = t,
                CompletedOrders = completedResult,
                PurposeOptions = JsonConvert.DeserializeObject<Dictionary<int, string>>(purposeResponseData)
            };
            return View(viewModel);
        }

        private static async Task<HulubejeResponse<List<CompletedOrders>>?> FetchCompletedOrders(HttpClient client, string url)
        {
            try
            {
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<HulubejeResponse<List<CompletedOrders>>>(responseData) ?? new HulubejeResponse<List<CompletedOrders>>();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
