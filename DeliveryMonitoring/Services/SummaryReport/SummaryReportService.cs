using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Controllers;
using DeliveryMonitoring.Helpers;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services.Api;
using DeliveryMonitoring.Services.Orders;

namespace DeliveryMonitoring.Services.SummaryReport
{
    public class SummaryReportService : ISummaryReportService
    {
        private readonly IApiRequestService _api;
        private readonly ICompletedOrdersService _ordersService;
        
        public SummaryReportService(
            IApiRequestService api,
            ICompletedOrdersService ordersService)
        {
            _api = api;
            _ordersService = ordersService;
        }

        public async Task<IEnumerable<MerchantSummary>> MerchantSummary(OrderQueryParams p)
        {
            var orders = await _ordersService.GetAllOrdersAsync(p);
            return SummaryBuilders.BuildMerchantSummary(orders.Data ?? new List<CompletedOrders>());
        }

        public async Task<IEnumerable<ConsigneeSummary>> ConsigneeSummary(OrderQueryParams p)
        {
            var orders = await _ordersService.GetAllOrdersAsync(p);
            return SummaryBuilders.BuildConsigneeSummary(orders.Data ?? new List<CompletedOrders>());
        }

        public async Task<IEnumerable<DriverSummary>> DriverSummary(OrderQueryParams p)
        {
            var orders = await _ordersService.GetCompletedOrdersAsync(p);
            var drivers = await _api.GetAvailableDriversAsync(
                OrderHelpers.IsTodayIncluded(p) || p.IsClear);

            return SummaryBuilders.BuildDriverSummary(orders.Data ?? new List<CompletedOrders>(), drivers);
        }

        public async Task<IEnumerable<SupervisorSummary>> SupervisorSummary(OrderQueryParams p)
        {
            var orders = await _ordersService.GetCompletedOrdersAsync(p);
            return SummaryBuilders.BuildSupervisorSummary(orders.Data ?? new List<CompletedOrders>());
        }
    }
}
