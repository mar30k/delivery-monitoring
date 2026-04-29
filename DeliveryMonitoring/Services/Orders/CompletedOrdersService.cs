using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Constants.Enums;
using DeliveryMonitoring.Controllers;
using DeliveryMonitoring.Helpers;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services.Api;
using DeliveryMonitoring.Services.Cache;
using System.Linq;
using static NuGet.Packaging.PackagingConstants;

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

        public CompletedOrdersService(IApiRequestService apiRequestService, AuthenticationManager authenticationManager)
        {
            _apiRequestService = apiRequestService;
            _authenticationManager = authenticationManager;
        }

        public async Task<HulubejeResponse<List<CompletedOrders>>> GetCompletedOrdersAsync(OrderQueryParams @params)
        {
            try
            {
                bool skipCache = OrderHelpers.IsTodayIncluded(@params) || @params.IsClear;

                var orders = (await _apiRequestService.GetCompletedOrdersAsync(skipCache))?.Data ?? new List<CompletedOrders>();

                if (!orders.Any())
                {
                    return HulubejeResponse<List<CompletedOrders>>.Fail(new List<string> { "No completed orders found." });
                }

                var filtered = OrderHelpers.FilterOrders(orders, @params, CompanyTin, AdminCompanyTin);
                filtered.ForEach(OrderHelpers.PrepareDisplayValues);

                return HulubejeResponse<List<CompletedOrders>>.Success(filtered);

            }
            catch (Exception ex)
            {
                return HulubejeResponse<List<CompletedOrders>>.Fail(new List<string> { $"Failed to fetch completed orders: {ex.Message}" });
            }
        }


        public async Task<HulubejeResponse<List<CompletedOrders>>> GetOrdersByTypeAsync(OrderQueryParams @params)
        {
            try
            {
                bool skipCache = OrderHelpers.IsTodayIncluded(@params) || @params.IsClear;

                var ordersData = await _apiRequestService.GetOrdersByTypeAsync(@params.Type, skipCache);

                if (ordersData == null || !ordersData.IsSuccessful)
                {
                    return HulubejeResponse<List<CompletedOrders>>.Fail(new List<string> { "Failed to fetch orders by type." });
                }

                var orders = ordersData.Data ?? new List<CompletedOrders>();

                foreach (var order in orders)
                {
                    OrderHelpers.PrepareDisplayValues(order);
                    order.TableId = ((DeliveryOrderType)@params.Type) switch
                    {
                        DeliveryOrderType.PickUpAtBranch => TakeAwayTableId,
                        DeliveryOrderType.InHouseDining => DineInTableId,
                        DeliveryOrderType.DeliveryToLocation => DeliveryTableId,
                        DeliveryOrderType.ScheduledDeliveryToLocation => ScheduledDeliveryTableId,
                        DeliveryOrderType.ScheduledPickUp => ScheduledTakeawayTableId,
                        _ => "N/A"
                    };
                }

                var filteredOrders = OrderHelpers.FilterOrders(orders, @params, CompanyTin, AdminCompanyTin);

                return HulubejeResponse<List<CompletedOrders>>.Success(filteredOrders);
            }
            catch (Exception ex)
            {
                return HulubejeResponse<List<CompletedOrders>>.Fail(new List<string> { $"Failed to fetch orders by type: {ex.Message}" });
            }
        }

        public async Task<HulubejeResponse<List<CompletedOrders>>> GetAllOrdersAsync(OrderQueryParams @params)
        {
            try
            {
                bool skipCache = OrderHelpers.IsTodayIncluded(@params) || @params.IsClear;


                var dineInTask = GetOrdersByTypeAsync(CopyWithType(@params, (int)DeliveryOrderType.InHouseDining));
                var takeAwayTask = GetOrdersByTypeAsync(CopyWithType(@params, (int)DeliveryOrderType.PickUpAtBranch));
                var scheduledDeliveryTask = GetOrdersByTypeAsync(CopyWithType(@params, (int)DeliveryOrderType.ScheduledDeliveryToLocation));
                var scheduledPickUpTask = GetOrdersByTypeAsync(CopyWithType(@params, (int)DeliveryOrderType.ScheduledPickUp));
                var deliveryTask = _apiRequestService.GetCompletedOrdersAsync(skipCache);

                await Task.WhenAll(dineInTask, takeAwayTask, scheduledDeliveryTask, scheduledPickUpTask, deliveryTask);

                var deliveryOrders = (await deliveryTask).Data ?? new List<CompletedOrders>();
                foreach (var item in deliveryOrders ?? new List<CompletedOrders>())
                {
                    OrderHelpers.PrepareDisplayValues(item);
                    item.TableId = DeliveryTableId;
                }


                var allOrders = (deliveryOrders ?? new List<CompletedOrders>())
                                .Concat((await dineInTask).Data ?? new List<CompletedOrders>())
                                .Concat((await takeAwayTask).Data ?? new List<CompletedOrders>())
                                .Concat((await scheduledDeliveryTask).Data ?? new List<CompletedOrders>())
                                .Concat((await scheduledPickUpTask).Data ?? new List<CompletedOrders>())
                                .ToList();

                var filteredOrders = OrderHelpers.FilterOrders(allOrders, @params, CompanyTin, AdminCompanyTin);
                return HulubejeResponse<List<CompletedOrders>>.Success(filteredOrders);
            }
            catch (Exception ex)
            {
                return HulubejeResponse<List<CompletedOrders>>.Fail(new List<string> { $"Failed to fetch all orders: {ex.Message}" });
            }
        }

        public async Task<HulubejeResponse<List<CompletedOrders>>> GetPendingOrdersAsync(OrderQueryParams @params)
        {
            try
            {
                bool skipCache = OrderHelpers.IsTodayIncluded(@params) || @params.IsClear;

                var orders = (await _apiRequestService.GetPendingOrdersAsync(skipCache))?.Data ?? new List<CompletedOrders>();

                if (!orders.Any())
                {
                    return HulubejeResponse<List<CompletedOrders>>.Fail(orders, "No pending orders found.");
                }

                var activeOrders = await _apiRequestService.GetOrderRequestsAsync();

                var activeOrdersByVoucher = activeOrders
                    .Where(a => !string.IsNullOrEmpty(a.VoucherCode))
                    .ToDictionary(a => a.VoucherCode!);

                foreach (var pendingOrder in orders)
                {
                    if (!string.IsNullOrEmpty(pendingOrder.VoucherCode) &&
                        activeOrdersByVoucher.TryGetValue(pendingOrder.VoucherCode, out var activeOrder))
                    {
                        pendingOrder.Status = activeOrder.Status;
                    }
                }

                var filtered = OrderHelpers.FilterOrders(orders, @params, CompanyTin, AdminCompanyTin);
                filtered.ForEach(OrderHelpers.PrepareDisplayValues);
                return HulubejeResponse<List<CompletedOrders>>.Success(filtered);
            }
            catch (Exception ex)
            {
                return HulubejeResponse<List<CompletedOrders>>.Fail(new List<string> { $"Failed to fetch pending orders: {ex.Message}" });
                
            }
        }

        public async Task<HulubejeResponse<List<CompletedOrders>>> GetDeliveryOrdersAsync(OrderQueryParams @params)
        {

            var deliveryTask = GetCompletedOrdersAsync(@params);
            var scheduledTask = GetOrdersByTypeAsync(CopyWithType(@params, (int)DeliveryOrderType.ScheduledDeliveryToLocation));

            await Task.WhenAll(deliveryTask, scheduledTask);

            var delivery =await deliveryTask;
            var scheduled = await scheduledTask;

            var combined = (delivery.Data ?? Enumerable.Empty<CompletedOrders>())
                .Concat(scheduled.Data ?? Enumerable.Empty<CompletedOrders>())
                .ToList();

            return HulubejeResponse<List<CompletedOrders>>.Success(combined);

        }
        private static OrderQueryParams CopyWithType(OrderQueryParams source, int type) => new()
        {
            Type = type,
            SummaryType = source.SummaryType,
            StartDate = source.StartDate,
            EndDate = source.EndDate,
            IsClear = source.IsClear
        };

        public async Task<HulubejeResponse<List<OrderDto>>> GetDeliveryOrders(OrderQueryParams @params)
        {
            try
            {
                bool skipCache = OrderHelpers.IsTodayIncluded(@params) || @params.IsClear;

                var orders = (await _apiRequestService.GetDeliveryOrdersAsync(skipCache)) ?? new List<OrderDto>();

                if (!orders.Any())
                {
                    return HulubejeResponse<List<OrderDto>>.Fail(new List<string> { "No completed orders found." });
                }

                var filtered = OrderHelpers.FilterDeliveryOrders(orders, @params, CompanyTin, AdminCompanyTin, status: "completed");

                return HulubejeResponse<List<OrderDto>>.Success(filtered);

            }
            catch (Exception ex)
            {
                return HulubejeResponse<List<OrderDto>>.Fail(new List<string> { $"Failed to fetch completed orders: {ex.Message}" });
            }
        }
    }
}
