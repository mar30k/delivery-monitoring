using Bogus.DataSets;
using CNET_ERP_V7.WebConstants;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

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
            string jsonResponse;
            string purposesJson;

            // Choose correct endpoint based on order type
            if (t.ToLower() == "dinein")
            {
                jsonResponse = await _apiRequest.GetOrdersByTypeRawAsync(3203);
            }
            else if (t.ToLower() == "takeaway")
            {
                jsonResponse = await _apiRequest.GetOrdersByTypeRawAsync(2076);
            }
            else
            {
                jsonResponse = await _apiRequest.GetCompletedOrdersRawAsync();
            }

            // 🟢 Fetch purposes (raw JSON)
            purposesJson = await _apiRequest.GetDeliveryPurposeRawAsync();

            // 🟢 Deserialize API responses
            var completedResult = JsonConvert.DeserializeObject<HulubejeResponse<List<CompletedOrders>>>(jsonResponse);
            var purposeOptions = JsonConvert.DeserializeObject<Dictionary<int, string>>(purposesJson);

            // 🟢 Apply filtering by CompanyTin
            if (!string.IsNullOrWhiteSpace(CompanyTin) && CompanyTin != "0076217301")
            {
                if (completedResult != null)
                    completedResult.Data = completedResult.Data?.Where(order => order.Tin == CompanyTin).ToList();
            }

            // 🟢 Handle dine-in or takeaway supervisor note extraction
            var isNonDeliveryType = t.ToLower() == "dinein" || t.ToLower() == "takeaway";
            if (isNonDeliveryType)
            {
                foreach (var item in completedResult?.Data ?? new List<CompletedOrders>())
                {
                    string supervisorName = "N/A";

                    if (!string.IsNullOrEmpty(item.Note) && item.Note.StartsWith("{"))
                    {
                        var match = Regex.Match(item.Note, @"^\{(.*?)\}");
                        if (match.Success)
                        {
                            supervisorName = match.Groups[1].Value;
                            item.Note = item.Note.Substring(match.Length).TrimStart();
                        }
                    }

                    item.SupervisorName = supervisorName;
                }
            }

            // 🟢 Build view model
            var viewModel = new CompletedOrdersViewModel
            {
                Type = t,
                CompletedOrders = completedResult,
                PurposeOptions = purposeOptions ?? new Dictionary<int, string>()
            };

            return View(viewModel);
        }
    }
}
