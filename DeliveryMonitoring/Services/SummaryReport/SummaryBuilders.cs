using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Models;
using System.Linq;

namespace DeliveryMonitoring.Services.SummaryReport
{
    internal static class SummaryBuilders
    {
        public static IEnumerable<MerchantSummary> BuildMerchantSummary(
            IEnumerable<CompletedOrders> orders)
        {
            return orders
                .GroupBy(o => o.BranchCode)
                .Select(g =>
                {
                    var first = g.First();
                    return new MerchantSummary
                    {
                        Tin = first.Tin,
                        CompanyName = first.CompanyName,
                        BranchName = first.BranchName,
                        TotalConsigneeCount = g.Select(x => x.PhoneNumber).Distinct().Count(),
                        DeliveryAmount = g.Sum(x => x.TotalAmount),
                        TotalDeliveryOrders = g.Count()
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
                    var first = g.First();
                    return new ConsigneeSummary
                    {
                        PhoneNumber = first.PhoneNumber,
                        Name = first.FirstName,
                        TotalMerchantCount = g
                            .Select(x => new { x.Tin, x.BranchCode })
                            .Distinct()
                            .Count()
                    };
                });
        }

        public static IEnumerable<DriverSummary> BuildDriverSummary(
            IEnumerable<CompletedOrders> orders,
            IEnumerable<Driver> drivers)
        {
            var driverLookup = drivers
                .Where(d => !string.IsNullOrWhiteSpace(d.PhoneNumber))
                .ToDictionary(d => d.PhoneNumber!);

            return orders
                .Where(o => !string.IsNullOrWhiteSpace(o.DriverPhoneNumber) && driverLookup.ContainsKey(o.DriverPhoneNumber))
                .GroupBy(o => o.DriverPhoneNumber!)
                .Select(g =>
                {
                    var first = g.First();
                    driverLookup.TryGetValue(first.DriverPhoneNumber!, out var driver);

                    var ratings = g.Where(o => o.Rating > 0).Select(o => o.Rating);
                    var topDay = g.GroupBy(o => o.RequestCreatedAt.Date)
                                  .OrderByDescending(x => x.Count())
                                  .FirstOrDefault();

                    return new DriverSummary
                    {
                        DriverPhoneNumber = first.DriverPhoneNumber,
                        Name = driver?.FirstName ?? "N/A",
                        TotalDeliveryOrders = g.Count(),
                        DeliveryAmount = g.Sum(o => o.TotalAmount),
                        TotalDistance = g.Sum(o => o.Distance),
                        TotalEtaDifference  = g.Sum(o => o.Eta - o.Duration),
                        TimelyDeliveriesCount = g.Count(o => o.EtaDifference >= 0),
                        LateDeliveriesCount = g.Count(o => o.EtaDifference < 0),
                        Tip = g.Sum(o => o.Tip),
                        AverageRating = ratings.Any()
                            ? Math.Round(ratings.Average(), 2)
                            : 0,
                        MostOrdersDate = topDay?.Key.ToString("ddd MM dd, yyyy"),
                        MostOrdersCount = topDay?.Count() ?? 0
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
                    var first = g.First();

                    return new SupervisorSummary
                    {
                        SupervisorName = first.SupervisorName,
                        SupervisorPhoneNumber = first.SupervisorPhoneNumber,
                        TotalDeliveryOrders = g.Count(),
                        DeliveryAmount = Math.Round(g.Sum(o => o.TotalAmount), 2),
                        PurposeSummary = g
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
                            .ToList()
                    };
                });
        }
    }
}
