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
            var deliveryordersTask = _ordersService.GetDeliveryOrdersAsync(p);
            var driversTask = _api.GetAvailableDriversAsync(
                OrderHelpers.IsTodayIncluded(p) || p.IsClear);

            await Task.WhenAll(deliveryordersTask, driversTask);
            var deliveryorders = await deliveryordersTask;
            var drivers = await driversTask;

            return SummaryBuilders.BuildDriverSummary(deliveryorders.Data ?? new List<CompletedOrders>(), drivers);
        }

        public async Task<IEnumerable<SupervisorSummary>> SupervisorSummary(OrderQueryParams p)
        {
            var deliveryOrders = await _ordersService.GetDeliveryOrdersAsync(p);
            return SummaryBuilders.BuildSupervisorSummary(deliveryOrders.Data ?? new List<CompletedOrders>());
        }
    }
}
