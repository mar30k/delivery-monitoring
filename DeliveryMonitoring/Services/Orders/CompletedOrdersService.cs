using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Controllers;
using DeliveryMonitoring.Helpers;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services.Api;
using DeliveryMonitoring.Services.Cache;

namespace DeliveryMonitoring.Services.Orders
{
    
    public class CompletedOrdersService : ICompletedOrdersService
    {
        private readonly AuthenticationManager _authenticationManager;
        private const string DineInTableId = "dineIn";
        private const string TakeAwayTableId = "takeAway";
        private const string DeliveryTableId = "delivery";
        private const string AdminCompanyTin = "0076217301";
        private readonly IApiRequestService _apiRequestService;
        private string CompanyTin => _authenticationManager.GetSecureCookie(CNET_WebConstantes.IdentificationCookie) ?? string.Empty;

        public CompletedOrdersService(IApiRequestService apiRequestService,AuthenticationManager authenticationManager)
        {
            _apiRequestService = apiRequestService;
            _authenticationManager = authenticationManager;
        }

        public async Task<HulubejeResponse<List<CompletedOrders>>> GetCompletedOrdersAsync(DateTime? startDate, DateTime? endDate, bool isClear)
        {
            bool skipCache = OrderHelpers.IsTodayIncluded(startDate, endDate) || isClear;

            var result = await _apiRequestService.GetCompletedOrdersAsync(skipCache);

            if (result?.Data == null)
                return new HulubejeResponse<List<CompletedOrders>> { Data = new List<CompletedOrders>(), IsSuccessful = false };

            var filtered = OrderHelpers.FilterOrders(result.Data, startDate, endDate, isClear, CompanyTin, AdminCompanyTin);
            filtered.ForEach(OrderHelpers.FormatRequestDate);

            return new HulubejeResponse<List<CompletedOrders>>
            {
                Data = filtered,
                IsSuccessful = true
            };
        }

        public async Task<List<CompletedOrders>> GetOrdersByTypeAsync(int type, DateTime? startDate, DateTime? endDate, bool isClear)
        {
            bool skipCache = OrderHelpers.IsTodayIncluded(startDate, endDate) || isClear;

            var ordersData = await _apiRequestService.GetOrdersByTypeAsync(type, skipCache);
            var orders = ordersData?.Data ?? new List<CompletedOrders>();

            foreach (var order in orders)
            {
                OrderHelpers.FormatRequestDate(order);
                OrderHelpers.ParseSupervisor(order);
                order.TableId = type == (int)DeliveryOrderTypes.PickUpAtBranch ? TakeAwayTableId : DineInTableId;
            }

            return OrderHelpers.FilterOrders(orders, startDate, endDate, isClear, CompanyTin, AdminCompanyTin);
        }

        public async Task<List<CompletedOrders>> GetAllOrdersAsync(DateTime? startDate, DateTime? endDate, bool isClear)
        {
            bool skipCache = OrderHelpers.IsTodayIncluded(startDate, endDate) || isClear;

            var dineInTask = GetOrdersByTypeAsync((int)DeliveryOrderTypes.InHouseDining, startDate, endDate, isClear);
            var takeAwayTask = GetOrdersByTypeAsync((int)DeliveryOrderTypes.PickUpAtBranch, startDate, endDate, isClear);

            var deliveryTask = _apiRequestService.GetCompletedOrdersAsync(skipCache);
            var deliveryOrders = await deliveryTask;

            foreach (var item in deliveryOrders.Data ?? new List<CompletedOrders>())
            {
                OrderHelpers.FormatRequestDate(item);
                item.TableId = DeliveryTableId;
            }

            await Task.WhenAll(dineInTask, takeAwayTask);

            var allOrders = (deliveryOrders.Data ?? new List<CompletedOrders>())
                            .Concat(await dineInTask)
                            .Concat(await takeAwayTask)
                            .ToList();

            return OrderHelpers.FilterOrders(allOrders, startDate, endDate, isClear, CompanyTin, AdminCompanyTin);
        }

    }

}
