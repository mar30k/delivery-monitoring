using DeliveryMonitoring.Models;

namespace DeliveryMonitoring.Services.Orders
{
    public interface ICompletedOrdersService
    {
        Task<HulubejeResponse<List<CompletedOrders>>> GetCompletedOrdersAsync(OrderQueryParams @params);
        Task<HulubejeResponse<List<CompletedOrders>>> GetPendingOrdersAsync(OrderQueryParams @params);
        Task<HulubejeResponse<List<CompletedOrders>>> GetOrdersByTypeAsync( OrderQueryParams @params);
        Task<HulubejeResponse<List<CompletedOrders>>> GetAllOrdersAsync(OrderQueryParams @params);
    }

}
