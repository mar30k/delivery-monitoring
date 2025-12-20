using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Helpers;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services.Api;
using MediaBrowser.Model.Services;

namespace DeliveryMonitoring.Services.SummaryReport
{
    internal static class OrderQueryHelper
    {
        private const string AdminTin = AppConstants.Company.AdminTin;

        public static async Task<List<CompletedOrders>> GetAllOrdersAsync(
            IApiRequestService api,
            string companyTin,
            OrderQueryParams p)
        {
            bool skipCache = OrderHelpers.IsTodayIncluded(p) || p.IsClear;

            var deliveryTask = api.GetCompletedOrdersAsync(skipCache);
            var dineInTask = api.GetOrdersByTypeAsync((int)DeliveryOrderTypes.InHouseDining, skipCache);
            var takeAwayTask = api.GetOrdersByTypeAsync((int)DeliveryOrderTypes.PickUpAtBranch, skipCache);

            await Task.WhenAll(deliveryTask, dineInTask, takeAwayTask);

            var allTasks = new [] { deliveryTask.Result, dineInTask.Result, takeAwayTask.Result };
            var merged = allTasks
                .Where(r => r?.Data != null)
                .SelectMany(r => r.Data!)
                .ToList();
            return ApplyFilters(merged, companyTin, p);
        }

        public static async Task<List<CompletedOrders>> GetCompletedOrdersAsync(
            IApiRequestService api,
            string companyTin,
            OrderQueryParams p)
        {
            bool skipCache = OrderHelpers.IsTodayIncluded(p) || p.IsClear;
            var response = await api.GetCompletedOrdersAsync(skipCache);

            return ApplyFilters(response?.Data ?? new List<CompletedOrders>(), companyTin, p);
        }
        
        private static List<CompletedOrders> ApplyFilters(
            List<CompletedOrders> data,
            string companyTin,
            OrderQueryParams p)
        {
            if (!string.IsNullOrWhiteSpace(companyTin) && companyTin != AdminTin)
                data = data.Where(o => o.Tin == companyTin).ToList();

            if (!p.IsClear)
            {
                if (p.StartDate.HasValue)
                    data = data.Where(o => o.RequestCreatedAt.Date >= p.StartDate.Value.Date).ToList();

                if (p.EndDate.HasValue)
                    data = data.Where(o => o.RequestCreatedAt.Date <= p.EndDate.Value.Date).ToList();
            }

            return data;
        }
    }
}
