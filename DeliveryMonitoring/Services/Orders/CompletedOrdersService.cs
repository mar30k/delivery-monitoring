using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Controllers;
using DeliveryMonitoring.Helpers;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services.Api;
using DeliveryMonitoring.Services.Cache;
using System.Linq;

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
                    return new HulubejeResponse<List<CompletedOrders>>
                    {
                        Data = orders,
                        IsSuccessful = false,
                        ErrorMessages = new List<string> { "No completed orders found." }
                    };
                }

                var filtered = OrderHelpers.FilterOrders(orders, @params, CompanyTin, AdminCompanyTin);
                filtered.ForEach(OrderHelpers.PrepareDisplayValues);

                return new HulubejeResponse<List<CompletedOrders>>
                {
                    Data = filtered,
                    IsSuccessful = true
                };
            }
            catch (Exception ex)
            {
                return new HulubejeResponse<List<CompletedOrders>>
                {
                    Data = new List<CompletedOrders>(),
                    IsSuccessful = false,
                    ErrorMessages = new List<string> { $"Failed to fetch completed orders: {ex.Message}" }
                };
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
                    return new HulubejeResponse<List<CompletedOrders>>
                    {
                        Data = new List<CompletedOrders>(),
                        IsSuccessful = false,
                        ErrorMessages = ordersData?.ErrorMessages ?? new List<string> { "Failed to retrieve orders." }
                    };
                }

                var orders = ordersData.Data ?? new List<CompletedOrders>();

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
                        _ => DeliveryTableId
                    };
                }

                var filteredOrders = OrderHelpers.FilterOrders(orders, @params, CompanyTin, AdminCompanyTin);

                return new HulubejeResponse<List<CompletedOrders>>
                {
                    Data = filteredOrders,
                    IsSuccessful = true
                };
            }
            catch (Exception ex)
            {
                return new HulubejeResponse<List<CompletedOrders>>
                {
                    Data = new List<CompletedOrders>(),
                    IsSuccessful = false,
                    ErrorMessages = new List<string> { $"Error retrieving orders by type: {ex.Message}" }
                };
            }
        }

        public async Task<HulubejeResponse<List<CompletedOrders>>> GetAllOrdersAsync(OrderQueryParams @params)
        {
            try
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

                await Task.WhenAll(dineInTask, takeAwayTask, scheduledDeliveryTask, scheduledPickUpTask, deliveryTask);

                var deliveryOrders = deliveryTask.Result?.Data ?? new List<CompletedOrders>();
                foreach (var item in deliveryOrders ?? new List<CompletedOrders>())
                {
                    OrderHelpers.PrepareDisplayValues(item);
                    item.TableId = DeliveryTableId;
                }


                var allOrders = (deliveryOrders ?? new List<CompletedOrders>())
                                .Concat(dineInTask.Result.Data ?? new List<CompletedOrders>())
                                .Concat(takeAwayTask.Result.Data ?? new List<CompletedOrders>())
                                .Concat(scheduledDeliveryTask.Result.Data ?? new List<CompletedOrders>())
                                .Concat(scheduledPickUpTask.Result.Data ?? new List<CompletedOrders>())
                                .ToList();

                return new HulubejeResponse<List<CompletedOrders>>
                {
                    Data = OrderHelpers.FilterOrders(allOrders, @params, CompanyTin, AdminCompanyTin),
                    IsSuccessful = true
                };
            }
            catch (Exception ex)
            {
                return new HulubejeResponse<List<CompletedOrders>>
                {
                    Data = new List<CompletedOrders>(),
                    IsSuccessful = false,
                    ErrorMessages = new List<string> { $"Error retrieving all orders: {ex.Message}" }
                };
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
                    return new HulubejeResponse<List<CompletedOrders>>
                    {
                        Data = orders,
                        IsSuccessful = false,
                        ErrorMessages = new List<string> { "No pending orders found." }
                    };
                }

                var filtered = OrderHelpers.FilterOrders(orders, @params, CompanyTin, AdminCompanyTin);
                filtered.ForEach(OrderHelpers.PrepareDisplayValues);
                return new HulubejeResponse<List<CompletedOrders>>
                {
                    Data = filtered,
                    IsSuccessful = true
                };
            }
            catch (Exception ex)
            {
                return new HulubejeResponse<List<CompletedOrders>>
                {
                    Data = new List<CompletedOrders>(),
                    IsSuccessful = false,
                    ErrorMessages = new List<string> { $"Failed to fetch pending orders: {ex.Message}" }
                };
            }
        }
    }
}
