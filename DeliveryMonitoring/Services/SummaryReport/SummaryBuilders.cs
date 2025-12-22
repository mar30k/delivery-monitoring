using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Models;
using System.Linq;

namespace DeliveryMonitoring.Services.SummaryReport
{
    internal static class SummaryBuilders
    {
        private const string DineInTableId = AppConstants.TableIds.DineIn;
        private const string TakeAwayTableId = AppConstants.TableIds.TakeAway;
        private const string DeliveryTableId = AppConstants.TableIds.Delivery;
        private const string ScheduledDeliveryTableId = AppConstants.TableIds.ScheduledDelivery;
        private const string ScheduledTakeawayTableId = AppConstants.TableIds.ScheduledPickUp;
        public static IEnumerable<MerchantSummary> BuildMerchantSummary(
            IEnumerable<CompletedOrders> orders)
        {
            return orders
                .GroupBy(o => o.BranchCode)
                .Select(g =>
                {
                    var list = g.ToList();
                    var first = list[0];
                    return new MerchantSummary
                    {
                        Tin = first.Tin,
                        CompanyName = first.CompanyName,
                        BranchName = first.BranchName,

                        TotalConsigneeCount = list
                            .Select(x => x.PhoneNumber)
                            .Where(p => !string.IsNullOrWhiteSpace(p))
                            .Distinct()
                            .Count(),

                        TotalDeliveryOrders = list.Count(x => x.TableId == DeliveryTableId),
                        TotalDineInOrders = list.Count(x => x.TableId == DineInTableId),
                        TotalTakeAwayOrders = list.Count(x => x.TableId == TakeAwayTableId),
                        TotalScheduledDeliveryOrders = list.Count(x => x.TableId == ScheduledDeliveryTableId),
                        TotalScheduledTakeawayOrders = list.Count(x => x.TableId == ScheduledTakeawayTableId),

                        DineInAmount = list
                            .Where(x => x.TableId == DineInTableId)
                            .Sum(x => x.TotalAmount),

                        TakeawayAmount = list
                            .Where(x => x.TableId == TakeAwayTableId)
                            .Sum(x => x.TotalAmount),

                        DeliveryAmount = list
                            .Where(x => x.TableId == DeliveryTableId)
                            .Sum(x => x.TotalAmount),

                        ScheduledDeliveryAmount = list
                            .Where(x => x.TableId == ScheduledDeliveryTableId)
                            .Sum(x => x.TotalAmount),

                        ScheduledTakeawayAmount = list
                            .Where(x => x.TableId == ScheduledTakeawayTableId)
                            .Sum(x => x.TotalAmount),

                        GrandTotal = list.Sum(x => x.TotalAmount)
                    };
                });
        }

        public static IEnumerable<ConsigneeSummary> BuildConsigneeSummary(
            IEnumerable<CompletedOrders> orders)
        {
            return orders
                .Where(o => !string.IsNullOrWhiteSpace(o.PhoneNumber))
                .GroupBy(o => o.PhoneNumber!)
                .Select(g =>
                {
                    var list = g.ToList();
                    var first = list[0];
                    return new ConsigneeSummary
                    {
                        PhoneNumber = first.PhoneNumber,
                        Name = first.FirstName,

                        TotalMerchantCount = list
                            .Select(x => new { x.Tin, x.BranchCode })
                            .Distinct()
                            .Count(),

                        TotalDeliveryOrders = list.Count(x => x.TableId == DeliveryTableId),
                        TotalDineInOrders = list.Count(x => x.TableId == DineInTableId),
                        TotalTakeAwayOrders = list.Count(x => x.TableId == TakeAwayTableId),
                        TotalScheduledDeliveryOrders = list.Count(x => x.TableId == ScheduledDeliveryTableId),
                        TotalScheduledTakeawayOrders = list.Count(x => x.TableId == ScheduledTakeawayTableId),

                        DineInAmount = list
                            .Where(x => x.TableId == DineInTableId)
                            .Sum(x => x.TotalAmount),

                        TakeawayAmount = list
                            .Where(x => x.TableId == TakeAwayTableId)
                            .Sum(x => x.TotalAmount),

                        DeliveryAmount = list
                            .Where(x => x.TableId == DeliveryTableId)
                            .Sum(x => x.TotalAmount),

                        ScheduledDeliveryAmount = list
                            .Where(x => x.TableId == ScheduledDeliveryTableId)
                            .Sum(x => x.TotalAmount),

                        ScheduledTakeawayAmount = list
                            .Where(x => x.TableId == ScheduledTakeawayTableId)
                            .Sum(x => x.TotalAmount),

                        GrandTotal = list.Sum(x => x.TotalAmount)
                    };
                });
        }

        public static IEnumerable<DriverSummary> BuildDriverSummary(
            IEnumerable<CompletedOrders> orders,
            IEnumerable<Driver> drivers)
        {
            var driverLookup = drivers
                .Where(d => !string.IsNullOrWhiteSpace(d.PhoneNumber))
                .GroupBy(d => d.PhoneNumber!)
                .ToDictionary(g => g.Key, g => g.First());


            return orders
                .Where(o => !string.IsNullOrWhiteSpace(o.DriverPhoneNumber) && driverLookup.ContainsKey(o.DriverPhoneNumber))
                .GroupBy(o => o.DriverPhoneNumber!)
                .Select(g =>
                {
                    var list = g.ToList();
                    var first = list[0];

                    driverLookup.TryGetValue(first.DriverPhoneNumber!, out var driver);

                    var ratings = list
                        .Where(o => o.Rating > 0)
                        .Select(o => o.Rating)
                        .ToList();

                    var topDay = list
                        .GroupBy(o => o.RequestCreatedAt.Date)
                        .OrderByDescending(x => x.Count())
                        .FirstOrDefault();

                    return new DriverSummary
                    {
                        DriverPhoneNumber = first.DriverPhoneNumber,
                        Name = driver?.FirstName ?? "N/A",

                        TotalDeliveryOrders = list.Count,
                        DeliveryAmount = list.Sum(o => o.TotalAmount),
                        TotalDistance = list.Sum(o => o.Distance),

                        TotalEtaDifference = list.Sum(o => o.EtaDifference),
                        TimelyDeliveriesCount = list.Count(o => o.EtaDifference >= 0),
                        LateDeliveriesCount = list.Count(o => o.EtaDifference < 0),

                        Tip = list.Sum(o => o.Tip),

                        AverageRating = ratings.Any()
                            ? Math.Round(ratings.Average(), 2)
                            : 0,

                        MostOrdersDate = topDay?.Key.ToString("ddd MM dd, yyyy"),
                        MostOrdersCount = topDay?.Count() ?? 0,

                        TotalConsigneeCount = list
                            .Select(x => x.PhoneNumber)
                            .Where(p => !string.IsNullOrWhiteSpace(p))
                            .Distinct()
                            .Count(),

                        TotalMerchantCount = list
                            .Select(x => new { x.Tin, x.BranchCode })
                            .Distinct()
                            .Count()
                    };
                });
        }

        public static IEnumerable<SupervisorSummary> BuildSupervisorSummary(
            IEnumerable<CompletedOrders> orders)
        {
            var purposes = OrderCategoryMappings.PurposeCategories;
            var colors = OrderCategoryMappings.CategoryColors;

            return orders
                .Where(o => !string.IsNullOrWhiteSpace(o.SupervisorPhoneNumber))
                .GroupBy(o => o.SupervisorPhoneNumber!)
                .Select(g =>
                {
                     var list = g.ToList();
                    var first = list[0];

                    return new SupervisorSummary
                    {
                        SupervisorName = first.SupervisorName,
                        SupervisorPhoneNumber = first.SupervisorPhoneNumber,

                        TotalDeliveryOrders = list.Count,
                        DeliveryAmount = Math.Round(list.Sum(o => o.TotalAmount), 2),

                        PurposeSummary = list
                            .Where(o => !string.IsNullOrWhiteSpace(o.Purpose))
                            .GroupBy(o => o.Purpose!)
                            .Select(p =>
                            {
                                var category = purposes.TryGetValue(p.Key, out var c)
                                    ? c
                                    : "Other";

                                return new PurposeItem
                                {
                                    Purpose = p.Key,
                                    Count = p.Count(),
                                    Color = colors[category]
                                };
                            })
                            .ToList(),

                        TotalConsigneeCount = list
                            .Select(x => x.PhoneNumber)
                            .Where(p => !string.IsNullOrWhiteSpace(p))
                            .Distinct()
                            .Count(),

                        TotalMerchantCount = list
                            .Select(x => new { x.Tin, x.BranchCode })
                            .Distinct()
                            .Count()
                    };
                });
        }
    }
}
