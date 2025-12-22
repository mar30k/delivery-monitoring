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
        private const string DineInTableId = AppConstants.TableIds.DineIn;
        private const string TakeAwayTableId = AppConstants.TableIds.TakeAway;
        private const string DeliveryTableId = AppConstants.TableIds.Delivery;
        private const string ScheduledDeliveryTableId = AppConstants.TableIds.ScheduledDelivery;
        private const string ScheduledTakeawayTableId = AppConstants.TableIds.ScheduledPickUp;
        private const string AdminCompanyTin = AppConstants.Company.AdminTin;
        private readonly IApiRequestService _apiRequestService;
        private string CompanyTin => _authenticationManager.GetSecureCookie(CNET_WebConstantes.IdentificationCookie) ?? string.Empty;

        public CompletedOrdersService(IApiRequestService apiRequestService,AuthenticationManager authenticationManager)
        {
            _apiRequestService = apiRequestService;
            _authenticationManager = authenticationManager;
        }

        public async Task<HulubejeResponse<List<CompletedOrders>>> GetCompletedOrdersAsync(OrderQueryParams @params)
        {
            bool skipCache = OrderHelpers.IsTodayIncluded(@params) || @params.IsClear;

            var result = await _apiRequestService.GetCompletedOrdersAsync(skipCache);

            if (result?.Data == null)
                return new HulubejeResponse<List<CompletedOrders>> { Data = new List<CompletedOrders>(), IsSuccessful = false };

            var filtered = OrderHelpers.FilterOrders(result.Data, @params, CompanyTin, AdminCompanyTin);
            filtered.ForEach(OrderHelpers.PrepareDisplayValues);

            return new HulubejeResponse<List<CompletedOrders>>
            {
                Data = filtered,
                IsSuccessful = true
            };
        }

        public async Task<List<CompletedOrders>> GetOrdersByTypeAsync(OrderQueryParams @params)
        {
            bool skipCache = OrderHelpers.IsTodayIncluded(@params) || @params.IsClear;

            var ordersData = await _apiRequestService.GetOrdersByTypeAsync(@params.Type, skipCache);
            var orders = ordersData?.Data ?? new List<CompletedOrders>();

            foreach (var order in orders)
            {
                OrderHelpers.PrepareDisplayValues(order);
                order.TableId = ((DeliveryOrderTypes)@params.Type) switch
                {
                    DeliveryOrderTypes.PickUpAtBranch => TakeAwayTableId,
                    DeliveryOrderTypes.InHouseDining => DineInTableId,
                    DeliveryOrderTypes.DeliveryToLocation => DeliveryTableId,
                    DeliveryOrderTypes.ScheduledDeliveryToLocation => ScheduledDeliveryTableId,
                    DeliveryOrderTypes.ScheduledPickUp => ScheduledTakeawayTableId,
                    _ => DeliveryTableId // fallback
                };
            }

            return OrderHelpers.FilterOrders(orders, @params, CompanyTin, AdminCompanyTin);
        }

        public async Task<List<CompletedOrders>> GetAllOrdersAsync( OrderQueryParams @params)
        {
            bool skipCache = OrderHelpers.IsTodayIncluded(@params) || @params.IsClear;

            OrderQueryParams WithType(int type) => new()
            {
                Type = type,
                SType = @params.SType,
                StartDate = @params.StartDate,
                EndDate = @params.EndDate,
                IsClear = @params.IsClear
            };

            var dineInTask = GetOrdersByTypeAsync(WithType((int)DeliveryOrderTypes.InHouseDining));
            var takeAwayTask = GetOrdersByTypeAsync(WithType((int)DeliveryOrderTypes.PickUpAtBranch));
            var scheduledDeliveryTask = GetOrdersByTypeAsync(WithType((int)DeliveryOrderTypes.ScheduledDeliveryToLocation));
            var scheduledPickUpTask = GetOrdersByTypeAsync(WithType((int)DeliveryOrderTypes.ScheduledPickUp));
            var deliveryTask = _apiRequestService.GetCompletedOrdersAsync(skipCache);
            var deliveryOrders = await deliveryTask;

            foreach (var item in deliveryOrders.Data ?? new List<CompletedOrders>())
            {
                OrderHelpers.PrepareDisplayValues(item);
                item.TableId = DeliveryTableId;
            }

            await Task.WhenAll(dineInTask, takeAwayTask, scheduledDeliveryTask, scheduledPickUpTask);

            var allOrders = (deliveryOrders.Data ?? new List<CompletedOrders>())
                            .Concat(await dineInTask)
                            .Concat(await takeAwayTask)
                            .Concat(await scheduledDeliveryTask)
                            .Concat(await scheduledPickUpTask)
                            .ToList();

            return OrderHelpers.FilterOrders(allOrders, @params, CompanyTin, AdminCompanyTin);
        }

    }

}
