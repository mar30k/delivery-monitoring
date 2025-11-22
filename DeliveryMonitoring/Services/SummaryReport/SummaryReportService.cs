using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Controllers;
using DeliveryMonitoring.Helpers;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services.Api;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeliveryMonitoring.Services.SummaryReport
{
    public class SummaryReportService : ISummaryReportService
    {
        private readonly IApiRequestService _apiRequestService;
        private readonly AuthenticationManager _authenticationManager;
        private string CompanyTin => _authenticationManager.GetSecureCookie(CNET_WebConstantes.IdentificationCookie) ?? string.Empty;
        private const string AdminCompanyTin = AppConstants.Company.AdminTin;
        public SummaryReportService(IApiRequestService apiRequestService, AuthenticationManager authenticationManager)
        {
            _apiRequestService = apiRequestService;
            _authenticationManager = authenticationManager;
        }

        public async Task<IEnumerable<object>> GetSummaryDataAsync(
            SummaryType type, DateTime? startDate, DateTime? endDate, bool isClear)
        {
            return type switch
            {
                SummaryType.Merchant => await BuildMerchantSummary(startDate, endDate, isClear),
                SummaryType.Driver => await BuildDriverSummary(startDate, endDate, isClear),
                SummaryType.Supervisor => await BuildSupervisorSummary(startDate, endDate, isClear),
                _ => await BuildConsigneeSummary(startDate, endDate, isClear),
            };
        }

        // --- Summary builders ---

        private async Task<IEnumerable<MerchantSummary>> BuildMerchantSummary(DateTime? start, DateTime? end, bool isClear)
        {
            return await BuildSummaryReport(
                c => c.BranchCode,
                (merchant, allOrders) =>
                {
                    var first = merchant.FirstOrDefault();
                    return new MerchantSummary
                    {
                        Tin = first?.Tin,
                        CompanyName = first?.CompanyName,
                        BranchName = first?.BranchName,
                        TotalConsigneeCount = merchant.Select(c => c.PhoneNumber).Distinct().Count()
                    };
                },
                start, end, isClear
            );
        }

        private async Task<IEnumerable<ConsigneeSummary>> BuildConsigneeSummary(DateTime? startDate, DateTime? endDate, bool isClear)
        {
            return await BuildSummaryReport(
                c => c.PhoneNumber ?? string.Empty,
                (consignee, allOrders) =>
                {
                    var first = consignee.FirstOrDefault();
                    return new ConsigneeSummary
                    {
                        PhoneNumber = first?.PhoneNumber,
                        Name = first?.FirstName,
                        TotalMerchantCount = consignee.Select(c => new { c.Tin, c.BranchCode }).Distinct().Count()
                    };
                },
                startDate, endDate, isClear
            );
        }

        private async Task<IEnumerable<DriverSummary>> BuildDriverSummary(DateTime? startDate, DateTime? endDate, bool isClear)
        {
            bool skipCache = OrderHelpers.IsTodayIncluded(startDate, endDate) || isClear;

            var availableDrivers = await _apiRequestService.GetAvailableDriversAsync(skipCache);

            var rawOrders = await _apiRequestService.GetCompletedOrdersAsync(skipCache);
            var orders = FilterOrdersByCompany(Clone(rawOrders), CompanyTin);

            if (orders?.Data == null || availableDrivers == null)
                return new List<DriverSummary>();

            orders.Data = FilterByDateRange(orders.Data, startDate, endDate, isClear);

            var availablePhones = availableDrivers
                .Where(d => !string.IsNullOrWhiteSpace(d.PhoneNumber))
                .Select(d => d.PhoneNumber)
                .ToHashSet();

            var driverSummary = orders.Data
                .Where(o => !string.IsNullOrWhiteSpace(o.DriverPhoneNumber) &&
                           availablePhones.Contains(o.DriverPhoneNumber))
                .GroupBy(o => o.DriverPhoneNumber)
                .Select(group =>
                {
                    var first = group.FirstOrDefault();
                    var driver = availableDrivers.FirstOrDefault(d => d.PhoneNumber == first?.DriverPhoneNumber);

                    var ratings = group.Where(o => o.Rating > 0).Select(o => o.Rating).ToList();
                    var avgRating = ratings.Any() ? Math.Round(ratings.Average(), 2) : 0;

                    var topDate = group.GroupBy(o => o.RequestCreatedAt.Date)
                        .Select(g => new { Date = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .FirstOrDefault();

                    return new DriverSummary
                    {
                        DriverPhoneNumber = first?.DriverPhoneNumber,
                        Name = driver?.FirstName ?? "N/A",
                        TotalDistance = group.Sum(o => o.Distance),
                        Tip = group.Sum(o => o.Tip),
                        TotalDeliveryOrders = group.Count(),
                        DeliveryAmount = group.Sum(o => o.TotalAmount),
                        TotalConsigneeCount = group.Select(o => o.PhoneNumber).Distinct().Count(),
                        TotalMerchantCount = group.Select(o => new { o.Tin, o.BranchCode }).Distinct().Count(),
                        AverageRating = avgRating,
                        TotalTimeDeviation = Math.Round(group.Sum(o => o.Eta) - group.Sum(o => o.Duration), 2),
                        MostOrdersDate = topDate?.Date.ToString("ddd MM dd, yyyy"),
                        MostOrdersCount = topDate?.Count ?? 0
                    };
                }).ToList();

            return driverSummary;
        }

        private async Task<IEnumerable<SupervisorSummary>> BuildSupervisorSummary(DateTime? start, DateTime? end, bool isClear)
        {
            bool skipCache = OrderHelpers.IsTodayIncluded(start, end) || isClear;

            var rawOrders = await _apiRequestService.GetCompletedOrdersAsync(skipCache);
            var orders = FilterOrdersByCompany(Clone(rawOrders), CompanyTin);


            if (orders?.Data == null) return new  List<SupervisorSummary>();
            orders.Data = FilterByDateRange(orders.Data, start, end, isClear);

            var purposeCategories = OrderCategoryMappings.PurposeCategories;
            var categoryColors = OrderCategoryMappings.CategoryColors;

            return orders.Data
                .Where(o => !string.IsNullOrWhiteSpace(o.SupervisorPhoneNumber))
                .GroupBy(o => o.SupervisorPhoneNumber)
                .Select(group =>
                {
                    var first = group.FirstOrDefault();
                    var totalConsigneeCount = group.Select(o => o.PhoneNumber).Distinct().Count();
                    var totalMerchantCount = group.Select(o => new { o.Tin, o.BranchCode }).Distinct().Count();

                    var purposeCounts = group.Where(o => !string.IsNullOrEmpty(o.Purpose))
                        .GroupBy(o => o.Purpose!)
                        .ToDictionary(g => g.Key!, g => g.Count());

                    
                    return new SupervisorSummary
                    {
                        SupervisorName = first?.SupervisorName,
                        SupervisorPhoneNumber = first?.SupervisorPhoneNumber,
                        TotalDeliveryOrders = group.Count(),
                        PurposeSummary = purposeCounts?.Select(pc =>
                        {
                            string category = purposeCategories.TryGetValue(pc.Key, out var cat) ? cat : "Other";
                            string color = categoryColors[category];
                            return new PurposeItem
                            {
                                Purpose = pc.Key,
                                Count = pc.Value,
                                Color = color
                            };
                        }).ToList() ?? new List<PurposeItem>(),
                        DeliveryAmount = Math.Round(group.Sum(o => o.TotalAmount), 2),
                        TotalConsigneeCount = totalConsigneeCount,
                        TotalMerchantCount = totalMerchantCount
                    };
                }).ToList();
        }

        // --- Helpers ---

        private static List<CompletedOrders> FilterByDateRange(
            IEnumerable<CompletedOrders> orders, DateTime? start, DateTime? end, bool isClear)
        {
            if (isClear || !start.HasValue || !end.HasValue)
                return orders.ToList();

            return orders.Where(o => o.RequestCreatedAt.Date >= start.Value.Date &&
                                     o.RequestCreatedAt.Date <= end.Value.Date).ToList();
        }

        private static HulubejeResponse<List<CompletedOrders>>? FilterOrdersByCompany(
            HulubejeResponse<List<CompletedOrders>>? completedResult, string companyTin)
        {
            try
            {
                if (completedResult == null) return null;

                if (!string.IsNullOrWhiteSpace(companyTin) && companyTin != AdminCompanyTin)
                {
                    completedResult.Data = completedResult.Data?
                        .Where(order => order.Tin == companyTin)
                        .ToList();
                }
                return completedResult;
            }
            catch
            {
                return null;
            }
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

            var allOrders = await CompletedOrdersViewModelAsync(CompanyTin ?? "", fromDate, toDate, isClear);

            var allOrderItems =
                (allOrders.DineInOders?.Data ?? new List<CompletedOrders>())
                .Concat(allOrders.CompletedOrders?.Data ?? new List<CompletedOrders>())
                .Concat(allOrders.TakeAwayOrders?.Data ?? new List<CompletedOrders>())
                .ToList();

            return allOrderItems
                .GroupBy(groupKeySelector)
                .Select(group =>
                {
                    var summary = createSummary(group, allOrders);
                    if (summary == null) return null;

                    var dineIn = allOrders.DineInOders?.Data?.Where(c => groupKeySelector(c)!.Equals(group.Key));
                    var takeAway = allOrders.TakeAwayOrders?.Data?.Where(c => groupKeySelector(c)!.Equals(group.Key));
                    var delivery = allOrders.CompletedOrders?.Data?.Where(c => groupKeySelector(c)!.Equals(group.Key));

                    summary.DineInAmount = dineIn?.Sum(c => c.TotalAmount) ?? 0;
                    summary.TakeawayAmount = takeAway?.Sum(c => c.TotalAmount) ?? 0;
                    summary.DeliveryAmount = delivery?.Sum(c => c.TotalAmount) ?? 0;
                    summary.TotalDineInOrders = dineIn?.Count() ?? 0;
                    summary.TotalTakeAwayOrders = takeAway?.Count() ?? 0;
                    summary.TotalDeliveryOrders = delivery?.Count() ?? 0;
                    summary.GrandTotal = summary.DineInAmount + summary.TakeawayAmount + summary.DeliveryAmount;

                    return summary;
                })
                .Where(s => s != null)
                .ToList()!;
        }

        private async Task<CompletedOrdersViewModel> CompletedOrdersViewModelAsync(
            string companyTin, DateTime startDate, DateTime endDate, bool isClear = false)
        {
            bool skipCache = OrderHelpers.IsTodayIncluded(startDate, endDate) || isClear;
            var deliveryTask = _apiRequestService.GetCompletedOrdersAsync(skipCache);
            var dineInTask = _apiRequestService.GetOrdersByTypeAsync((int)DeliveryOrderTypes.InHouseDining, skipCache);
            var takeAwayTask = _apiRequestService.GetOrdersByTypeAsync((int)DeliveryOrderTypes.PickUpAtBranch, skipCache);

            var delivery = FilterOrdersByCompany(Clone(await deliveryTask), companyTin);
            var dineIn = FilterOrdersByCompany(Clone(await dineInTask), companyTin);
            var takeAway = FilterOrdersByCompany(Clone(await takeAwayTask), companyTin);

            HulubejeResponse<List<CompletedOrders>> FilterByDate(HulubejeResponse<List<CompletedOrders>>? resp)
            {
                if (resp == null) return new() { Data = new List<CompletedOrders>() };
                var data = resp.Data ??new List<CompletedOrders>();
                if (!isClear)
                    data = data.Where(o => o.RequestCreatedAt.Date >= startDate.Date && o.RequestCreatedAt.Date <= endDate.Date).ToList();
                resp.Data = data;
                return resp;
            }

            return new CompletedOrdersViewModel
            {
                CompletedOrders = FilterByDate(delivery),
                DineInOders = FilterByDate(dineIn),
                TakeAwayOrders = FilterByDate(takeAway),
                CompanyTin = companyTin
            };
        }
        private static T Clone<T>(T source)
        {
            return JsonConvert.DeserializeObject<T>(
                JsonConvert.SerializeObject(source)
            )!;
        }
    }
}
