using CNET_ERP_V7.WebConstants;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services;
using DeliveryMonitoring.Helpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;

namespace DeliveryMonitoring.Controllers
{
    public class SummaryController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IApiRequestService _apiRequest;
        private string CompanyTin =>
        _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie] ?? "";
        public SummaryController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IApiRequestService apiRequest)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _apiRequest = apiRequest;
        }
        [HttpGet("/summary")]
        public IActionResult Index(string t = "consignee")
        {
            var config = SummaryConfigFactory.Create(t); // encapsulate config in one place
            return View(config);
        }

        [HttpPost("/summary/data")]
        public async Task<IActionResult> SummaryData(string type, DateTime? startDate, DateTime? endDate, bool isClear)
        {
            switch (type?.ToLower())
            {
                case "merchant":
                    var merchantSummaries = await BuildSummaryReport(
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
                        startDate, endDate, isClear
                    );
                    return Json(new { data = merchantSummaries });

                case "consignee":
                default:
                    var consigneeSummaries = await BuildSummaryReport(
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
                        startDate, endDate, isClear
                    );
                    return Json(new { data = consigneeSummaries });
            }
        }


        public async Task<CompletedOrdersViewModel> CompletedOrdersViewModel(
            HttpClient client,
            string companyTin,
            DateTime startDate,
            DateTime endDate,
            bool isClear = false)
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
                var filteredData = response.Data ?? new List<CompletedOrders>();
                if (!isClear)
                {
                    filteredData = filteredData
                                .Where(o => o.RequestCreatedAt.Date >= startDate.Date &&
                                            o.RequestCreatedAt.Date <= endDate.Date)
                                .ToList()
                            ?? new List<CompletedOrders>();
                }
                return new HulubejeResponse<List<CompletedOrders>>
                {
                    IsSuccessful = response.IsSuccessful,
                    ErrorMessages = response.ErrorMessages,
                    AdditionalParameters = response.AdditionalParameters,
                    Data = filteredData
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

        private async Task<List<TSummary>> BuildSummaryReport<TSummary, TKey>(
             Func<CompletedOrders, TKey> groupKeySelector,
             Func<IGrouping<TKey, CompletedOrders>, CompletedOrdersViewModel, TSummary> createSummary,
             DateTime? startDate = null,
             DateTime? endDate = null,
             bool isClear = false)
            where TSummary : Summary
        {
            var fromDate = startDate ?? DateTime.Now.Date;
            var toDate = endDate ?? DateTime.Now.Date;

            var allOrdersByType = await CompletedOrdersViewModel(
                _httpClientFactory.CreateClient("CnetApiBaseUrl"),
                CompanyTin ?? "",
                fromDate,
                toDate,
                isClear);

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
    }
}
