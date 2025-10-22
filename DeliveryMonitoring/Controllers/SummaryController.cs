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
                case "driver":
                    var availableDrivers = await GetAvailableDrivers();
                    if (CompanyTin != "0076217301")
                        availableDrivers = availableDrivers.Where(d => d.CompanyTin != null && d.CompanyTin == CompanyTin).ToList();
                    var ordersResult = await FetchCompletedOrders(CompanyTin, _httpClientFactory.CreateClient("CnetApiBaseUrl"), "voucher/getcompletedorders");

                    if (ordersResult?.Data == null || availableDrivers == null)
                        return Json(new { data = new List<object>() });

                    if (!isClear && startDate.HasValue && endDate.HasValue)
                    {
                        var start = startDate.Value.Date;
                        var end = endDate.Value.Date;
                        ordersResult.Data = ordersResult.Data.Where(o =>
                            o.RequestCreatedAt.Date >= start &&
                            o.RequestCreatedAt.Date <= end
                        ).ToList();
                    }
                    // Get driver phone numbers from available drivers for filtering
                    var availableDriverPhones = availableDrivers
                        .Where(d => !string.IsNullOrWhiteSpace(d.PhoneNumber))
                        .Select(d => d.PhoneNumber)
                        .ToHashSet();

                    // Group by driver phone number but only include available drivers
                    var driverSummary = ordersResult.Data
                        .Where(d => !string.IsNullOrWhiteSpace(d.DriverPhoneNumber) &&
                                   availableDriverPhones.Contains(d.DriverPhoneNumber))
                        .GroupBy(d => d.DriverPhoneNumber)
                        .Select(group =>
                        {
                            var first = group.FirstOrDefault();
                            var driver = availableDrivers.FirstOrDefault(d => d.PhoneNumber == first?.DriverPhoneNumber);
                            var validRatings = group
                                .Where(o => o.Rating > 0)
                                .Select(o => o.Rating)
                                .ToList();
                            var averageRating = validRatings.Any() ? Math.Round(validRatings.Average(), 2)  : 0;
                            var totalTimeDeviation = group.Sum(o => o.Eta) - group.Sum(o => o.Duration);
                            return new
                            {
                                first?.DriverPhoneNumber,
                                Name = driver?.FirstName ?? "N/A",
                                TotalDistance = group.Sum(o => o.Distance),
                                Tip = group.Sum(o => o.Tip),
                                TotalDeliveryOrders = group.Count(),
                                DeliveryAmount = group.Sum(o => o.TotalAmount),
                                TotalConsigneeCount = group
                                    .Select(o => o.PhoneNumber)
                                    .Distinct()
                                    .Count(),
                                TotalMerchantCount = group
                                    .Select(o => new { o.Tin, o.BranchCode, o.CompanyCode })
                                    .Distinct()
                                    .Count(),
                                averageRating,
                                totalTimeDeviation
                            };
                        })
                        .ToList();

                    return Json(new { data = driverSummary });
                case "supervisor":
                    if (CompanyTin != "0076217301")
                        return Json(new { data = new List<object>() });
                    var orderResult = await FetchCompletedOrders(CompanyTin, _httpClientFactory.CreateClient("CnetApiBaseUrl"), "voucher/getcompletedorders");

                    if (orderResult?.Data == null )
                        return Json(new { data = new List<object>() });
                    if(!isClear && startDate.HasValue && endDate.HasValue)
                    {
                        var start = startDate.Value.Date;
                        var end = endDate.Value.Date;
                        orderResult.Data = orderResult.Data
                            .Where(o=> o.RequestCreatedAt.Date >= start &&
                            o.RequestCreatedAt.Date <= end
                        ).ToList();
                    }

                    var categoryColors = new Dictionary<string, string>
                    {
                        { "Good", "green" },
                        { "Very Critical", "red" },
                        { "Restaurant Related", "purple" },
                        { "Vehicle Related", "darkred" },
                        { "Customer Related", "orange" },
                        { "System Error", "blue" },
                        { "Other", "gray" }
                    };

                    var purposeCategories = new Dictionary<string, string>
                    {
                        // Good
                        { "Successful Delivery", "Good" },
                        { "Successful Pickup", "Good" },
                        { "Successful Dining", "Good" },

                        // Customer Related
                        { "Ordered By Mistake", "Customer Related" },
                        { "Incorrect Delivery Address", "Customer Related" },
                        { "Incorrectly Marked As Delivered", "Customer Related" },
                        { "Address Out Of Range", "Customer Related" },
                        { "Wrong Order Placed", "Customer Related" },
                        { "Customer Unreachable", "Customer Related" },
                        

                        // Restaurant Related
                        { "Item Out Of Stock", "Restaurant Related" },
                        { "Long Preparation Time", "Restaurant Related" },
                        { "Order Declined By Restaurant", "Restaurant Related" },
                        { "Restaurant Closed", "Restaurant Related" },
                        { "Special Request Not Possible", "Restaurant Related" },

                        // Vehicle Related
                        { "Traffic Or Road Blockage", "Vehicle Related" },
                        { "Weather Conditions", "Vehicle Related" },
                        { "Vehicle Accident", "Vehicle Related" },
                        { "Vehicle Malfunction", "Vehicle Related" },
                        { "Vehicle Out of Charge or Fuel", "Vehicle Related" },
                        { "Personal Emergency", "Customer Related" },

                        // System Error
                        { "Duplicate Order", "System Error" }, // If system caused duplicates
                       

                        // Very Critical
                        { "Robbery", "Very Critical" },
                        { "Delayed Delivery", "Very Critical" }
                    };

                    var supervisorSummary = orderResult.Data
                        .Where(o => !string.IsNullOrWhiteSpace(o.SupervisorPhoneNumber))
                        .GroupBy(o => o.SupervisorPhoneNumber)
                        .Select(group =>
                        {
                            var first = group.FirstOrDefault();
                            var totalConsigneeCount = group.DistinctBy(o => o.PhoneNumber).Count();
                            var totalMerchantCount = group.DistinctBy(o => new { o.Tin, o.BranchCode, o.CompanyCode }).Count();

                            var purposeCounts = group
                                .Where(o => !string.IsNullOrEmpty(o.Purpose))?
                                .GroupBy(o => o.Purpose!)
                                .ToDictionary(g => g.Key!, g => g.Count());

                            

                            string purposeSummary = "";

                            if (purposeCounts != null)
                            {
                                var htmlList = purposeCounts.Select(pc =>
                                {
                                    string category = purposeCategories.TryGetValue(pc.Key, out var cat) ? cat : "Other";
                                    string color = categoryColors[category];

                                    return new { Section = category, Html = $"<span style='color:{color}; font-weight:600;'>{pc.Key}: {pc.Value}</span>" };
                                }).ToList();

                                purposeSummary = string.Join("<br>",
                                    htmlList.GroupBy(x => x.Section)
                                            .Select(g => string.Join(", ", g.Select(x => x.Html)))
                                );
                            }


                            return new
                            {
                                first?.SupervisorName,
                                first?.SupervisorPhoneNumber,
                                TotalDeliveryOrders = group.Count(),
                                TotalOrderDeclinedByRestaurant = group.Where(o => o.Purpose == "Order Declined By Restaurant").Count(),
                                purposeSummary,
                                DeliveryAmount = Math.Round(group.Sum(o=> o.TotalAmount), 2),
                                totalConsigneeCount,
                                totalMerchantCount
                            };
                        }).ToList();
                    return Json(new { data = supervisorSummary });
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
        public async Task<List<Driver>> GetAvailableDrivers()
        {
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];
            string uri = companyTin == "0076217301" ? "/drivers" : $"/drivers?companyTin={companyTin}";
            var _client = _httpClientFactory.CreateClient("Delivery");
            List<Driver> drivers = new();

            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + uri);

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                drivers = JsonConvert.DeserializeObject<List<Driver>>(data) ?? new List<Driver>();
            }
            return drivers;
        }
    }
}
