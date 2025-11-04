using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Helpers;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;

namespace DeliveryMonitoring.Controllers
{
    [Authorize]
    public class SummaryController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IApiRequestService _apiRequestService;
        private readonly AuthenticationManager _authenticationManager;
        private string CompanyTin => _authenticationManager.GetSecureCookie(CNET_WebConstantes.IdentificationCookie) ?? string.Empty;
        private const string AdminCompanyTin = "0076217301";
        public SummaryController( IHttpContextAccessor httpContextAccessor, IApiRequestService apiRequest, AuthenticationManager authenticationManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _apiRequestService = apiRequest;
            _authenticationManager = authenticationManager;
        }
        [HttpGet("/summary")]
        public IActionResult Index(SummaryReportType t )
        {
            var config = TableConfigFactory.CreateSummary(t); // encapsulate config in one place
            return View(config);
        }

        [HttpGet("/summary/data")]
        public async Task<IActionResult> SummaryData(SummaryType type, DateTime? startDate, DateTime? endDate, bool isClear)
        {
            switch (type)
            {
                case SummaryType.Merchant:
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
                case SummaryType.Driver:
                    var availableDrivers = await _apiRequestService.GetAvailableDriversAsync();
                    var ordersResult = FilterOrdersByCompany(await _apiRequestService.GetCompletedOrdersAsync(), CompanyTin);

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
                            var totalTimeDeviation = Math.Round(group.Sum(o => o.Eta) - group.Sum(o => o.Duration), 2);
                            // Find the date with the most orders for this driver
                            var topOrderDate = group
                                .GroupBy(o => o.RequestCreatedAt.Date)
                                .Select(g => new { Date = g.Key, Count = g.Count() })
                                .OrderByDescending(x => x.Count)
                                .ThenBy(x => x.Date) // optional: earliest if tied
                                .FirstOrDefault();

                            return new DriverSummary
                            {
                                DriverPhoneNumber = first?.DriverPhoneNumber,
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
                                AverageRating = averageRating,
                                TotalTimeDeviation = totalTimeDeviation,
                                // 🔹 New fields:
                                MostOrdersDate = topOrderDate?.Date.ToString("ddd MM dd, yyyy"),
                                MostOrdersCount = topOrderDate?.Count ?? 0
                            };
                        })
                        .ToList();

                    return Json(new { data = driverSummary });
                case SummaryType.Supervisor:
                    if (CompanyTin != AdminCompanyTin)
                        return Json(new { data = new List<object>() });
                    var orderResult = FilterOrdersByCompany(await _apiRequestService.GetCompletedOrdersAsync(), CompanyTin);

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

                    var purposeCategories = OrderCategoryMappings.PurposeCategories;
                    var categoryColors = OrderCategoryMappings.CategoryColors;

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


                            return new SupervisorSummary
                            {
                                SupervisorName = first?.SupervisorName,
                                SupervisorPhoneNumber = first?.SupervisorPhoneNumber,
                                TotalDeliveryOrders = group.Count(),
                                PurposeSummary = string.IsNullOrEmpty(purposeSummary) ? "N/A" : purposeSummary,
                                DeliveryAmount = Math.Round(group.Sum(o=> o.TotalAmount), 2),
                                TotalConsigneeCount =  totalConsigneeCount,
                                TotalMerchantCount = totalMerchantCount
                            };
                        }).ToList();
                    return Json(new { data = supervisorSummary });
                case SummaryType.Consignee:
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


        public async Task<CompletedOrdersViewModel> CompletedOrdersViewModelAsync(
            string companyTin,
            DateTime startDate,
            DateTime endDate,
            bool isClear = false)
        {
            // Fetch and filter by company
            var deliveryTask = _apiRequestService.GetCompletedOrdersAsync();
            var dineInTask = _apiRequestService.GetOrdersByTypeAsync((int)DeliveryOrderTypes.InHouseDining);
            var takeAwayTask = _apiRequestService.GetOrdersByTypeAsync((int)DeliveryOrderTypes.PickUpAtBranch);

            await Task.WhenAll(deliveryTask, dineInTask, takeAwayTask);

            var deliveryResponse = FilterOrdersByCompany(await deliveryTask, companyTin);
            var dineInResponse = FilterOrdersByCompany(await dineInTask, companyTin);
            var takeAwayResponse = FilterOrdersByCompany(await takeAwayTask, companyTin);

            // Local helper for date filtering
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
                        .ToList();
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

            var allOrdersByType = await CompletedOrdersViewModelAsync(
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

        private static HulubejeResponse<List<CompletedOrders>>? FilterOrdersByCompany(
            HulubejeResponse<List<CompletedOrders>>? completedResult,
            string companyTin)
        {
            try
            {
                if (completedResult == null)
                    return null;

                // Apply company filter if the TIN is provided and not the default one
                if (!string.IsNullOrWhiteSpace(companyTin) && companyTin != AdminCompanyTin)
                {
                    completedResult.Data = completedResult.Data?
                        .Where(order => order.Tin == companyTin)
                        .ToList();
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
