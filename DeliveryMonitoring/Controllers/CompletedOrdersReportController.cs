using Bogus.DataSets;
using CNET_ERP_V7.WebConstants;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DeliveryMonitoring.Controllers
{
    public class CompletedOrdersReportController : Controller
    {
        private IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IApiRequestService _apiRequest;
        private string CompanyTin =>
        _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie] ?? "";
        public CompletedOrdersReportController(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory, IApiRequestService apiRequest)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _apiRequest = apiRequest;
        }
        [Route("/report")]
        public async Task<IActionResult> Index(string t = "")
        {
            var _client = _httpClientFactory.CreateClient("CnetApiBaseUrl");
            string url = "voucher/getcompletedorders";
            if (t.ToLower() == "dinein")
            {
                url = "voucher/getordersbytype?type=3203";
            }
            else if (t.ToLower() == "takeaway")
            {
                url = "voucher/getordersbytype?type=2076";
            }
            var completedResult = await _apiRequest.GetAsync<HulubejeResponse<List<CompletedOrders>>>(url);
            if (!string.IsNullOrWhiteSpace(CompanyTin) && CompanyTin != "0076217301")
            {
                if (completedResult != null)
                    completedResult.Data = completedResult.Data?.Where(order => order.Tin == CompanyTin).ToList();
            }
            var viewModel = new CompletedOrdersViewModel
            {
                Type = t,
                CompletedOrders = completedResult,
                PurposeOptions = await _apiRequest.GetAsync<Dictionary<int, string>>("delivery/getpurpose"),
            };
            return View(viewModel);
        }
    }
}
