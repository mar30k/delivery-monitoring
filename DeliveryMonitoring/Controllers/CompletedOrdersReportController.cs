using Bogus.DataSets;
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
        private string CompanyTin =>
        _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie] ?? "";
        public CompletedOrdersReportController(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
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
            var purposeResponse = await _client.GetAsync("delivery/getpurpose");
            if (!purposeResponse.IsSuccessStatusCode)
            {
                var errorContent = await purposeResponse.Content.ReadAsStringAsync();
                ViewBag.ErrorMessage = $"Failed to retrieve completed orders. Server responded with: {errorContent}";
            }
            var purposeResponseData = await purposeResponse.Content.ReadAsStringAsync();
            var completedResult = await FetchCompletedOrders(CompanyTin ?? "", _client, url);
            var viewModel = new CompletedOrdersViewModel
            {
                Type = t,
                CompletedOrders = completedResult,
                PurposeOptions = JsonConvert.DeserializeObject<Dictionary<int, string>>(purposeResponseData)
            };
            return View(viewModel);
        }

        private static async Task<HulubejeResponse<List<CompletedOrders>>?> FetchCompletedOrders(string companyTin, HttpClient client, string url)
        {
            try
            {
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var responseData = await response.Content.ReadAsStringAsync();
                var completedResult = JsonConvert.DeserializeObject<HulubejeResponse<List<CompletedOrders>>>(responseData) ?? new HulubejeResponse<List<CompletedOrders>>();
                if (!string.IsNullOrWhiteSpace(companyTin) && companyTin != "0076217301")
                {
                    if (completedResult != null)
                        completedResult.Data = completedResult.Data?.Where(order => order.Tin == companyTin).ToList();
                }
                return completedResult;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<CompletedOrdersViewModel> CompletedOrdersViewModel(
            HttpClient client,
            string companyTin,
            DateTime startDate,
            DateTime endDate)
        {
            var deliveryOrdersUrl = "voucher/getcompletedorders";
            var dineInOrdersUrl = "voucher/getordersbytype?type=3203";
            var takeAwayOrdersUrl = "voucher/getordersbytype?type=2076";

            var deliveryTask = FetchCompletedOrders(companyTin, client, deliveryOrdersUrl);
            var dineInTask = FetchCompletedOrders(companyTin, client, dineInOrdersUrl);
            var takeAwayTask = FetchCompletedOrders(companyTin, client, takeAwayOrdersUrl);

            await Task.WhenAll(deliveryTask, dineInTask, takeAwayTask);

            var deliveryResponse = await deliveryTask;
            var dineInResponse = await dineInTask;
            var takeAwayResponse = await takeAwayTask;

            // Helper to safely filter even if response or Data is null
            HulubejeResponse<List<CompletedOrders>> FilterByDate(HulubejeResponse<List<CompletedOrders>>? response)
            {
                if (response == null)
                {
                    return new HulubejeResponse<List<CompletedOrders>>
                    {
                        IsSuccessful = false,
                        Data = new List<CompletedOrders>(),
                        ErrorMessages = new List<string> { "Response was null" }
                    };
                }

                return new HulubejeResponse<List<CompletedOrders>>
                {
                    IsSuccessful = response.IsSuccessful,
                    ErrorMessages = response.ErrorMessages,
                    AdditionalParameters = response.AdditionalParameters,
                    Data = response.Data?
                                .Where(o => o.RequestCreatedAt.Date >= startDate.Date &&
                                            o.RequestCreatedAt.Date <= endDate.Date)
                                .ToList()
                            ?? new List<CompletedOrders>()
                };
            }

            return new CompletedOrdersViewModel
            {
                CompletedOrders = FilterByDate(deliveryResponse),
                DineInOders = FilterByDate(dineInResponse),
                TakeAwayOrders = FilterByDate(takeAwayResponse),
                CompanyTin = companyTin
            };
        }

        [Route("/csummary")]
        public IActionResult ConsigneeSummaryReport()
        {
            
            return View("consigneesummary");
        }

        [HttpGet]
        [Route("/msummary")]
        public IActionResult MerchantSummaryReport()
        {
            return View("merchantsummary");
        }

        // AJAX Data for DataTable
        [HttpPost]
        [Route("/msummary/data")]
        public async Task<IActionResult> MerchantSummaryData(DateTime? startDate, DateTime? endDate)
        {
            var summaries = await BuildSummaryReport(
                c => c.BranchCode,
                (merchant, allOrdersByType) =>
                {
                    var firstItem = merchant.FirstOrDefault();
                    return new MerchantSummary
                    {
                        Tin = firstItem?.Tin,
                        CompanyName = firstItem?.CompanyName,
                        BranchName = firstItem?.BranchName,
                        TotalConsigneeCount = merchant
                            .Select(c => c.PhoneNumber)
                            .Distinct()
                            .Count()
                    };
                },
                startDate,
                endDate
            );

            return Json(new { data = summaries }); // <-- return JSON only
        }
        // AJAX Data for DataTable
        [HttpPost]
        [Route("/csummary/data")]
        public async Task<IActionResult> ConsigneeSummaryData(DateTime? startDate, DateTime? endDate)
        {
            var summaries = await BuildSummaryReport(
                c => c.PhoneNumber ?? string.Empty,
                (consignee, allOrdersByType) =>
                {
                    var firstItem = consignee.FirstOrDefault();
                    return new ConsigneeSummary
                    {
                        PhoneNumber = firstItem?.PhoneNumber,
                        Name = firstItem?.FirstName,
                        TotalMerchantCount = consignee
                            .Select(c => new { c.Tin, c.BranchCode })
                            .Distinct()
                            .Count()
                    };
                },

                startDate,
                endDate
            );

            return Json(new { data = summaries }); // <-- return JSON only
        }
        private async Task<List<TSummary>> BuildSummaryReport<TSummary, TKey>(
             Func<CompletedOrders, TKey> groupKeySelector,
             Func<IGrouping<TKey, CompletedOrders>, CompletedOrdersViewModel, TSummary> createSummary,
             DateTime? startDate = null,
             DateTime? endDate = null)
            where TSummary : Summary
        {
            var fromDate = startDate ?? DateTime.Now.Date;
            var toDate = endDate ?? DateTime.Now.Date;

            var allOrdersByType = await CompletedOrdersViewModel(
                _httpClientFactory.CreateClient("CnetApiBaseUrl"),
                CompanyTin ?? "",
                fromDate,
                toDate);

            var allOrderItems =
                (allOrdersByType?.DineInOders?.Data ?? Enumerable.Empty<CompletedOrders>())
                .Concat(allOrdersByType?.CompletedOrders?.Data ?? Enumerable.Empty<CompletedOrders>())
                .Concat(allOrdersByType?.TakeAwayOrders?.Data ?? Enumerable.Empty<CompletedOrders>())
                .ToList();

            return allOrderItems
                .GroupBy(groupKeySelector)
                .Select(group =>
                {
                    var summary = createSummary(group, allOrdersByType ?? new Models.CompletedOrdersViewModel());
                    if (summary == null) return null;

                    // common aggregates
                    var dineIn = allOrdersByType?.DineInOders?.Data?.Where(c => groupKeySelector(c)!.Equals(group.Key)) ?? Enumerable.Empty<CompletedOrders>();
                    var takeAway = allOrdersByType?.TakeAwayOrders?.Data?.Where(c => groupKeySelector(c)!.Equals(group.Key)) ?? Enumerable.Empty<CompletedOrders>();
                    var delivery = allOrdersByType?.CompletedOrders?.Data?.Where(c => groupKeySelector(c)!.Equals(group.Key)) ?? Enumerable.Empty<CompletedOrders>();

                    summary.DineInAmount = dineIn.Sum(c => c.TotalAmount);
                    summary.TakeawayAmount = takeAway.Sum(c => c.TotalAmount);
                    summary.DeliveryAmount = delivery.Sum(c => c.TotalAmount);

                    summary.TotalDineInOrders = dineIn.Count();
                    summary.TotalTakeAwayOrders = takeAway.Count();
                    summary.TotalDeliveryOrders = delivery.Count();

                    summary.GrandTotal = summary.DineInAmount + summary.TakeawayAmount + summary.DeliveryAmount;

                    return summary;
                })
                .Where(s => s != null)
                .ToList()!;
        }
    }
}
