using DeliveryMonitoring.Models;

namespace DeliveryMonitoring.Services.Orders
{
    public interface ICompletedOrdersService
    {
        Task<HulubejeResponse<List<CompletedOrders>>> GetCompletedOrdersAsync(OrderQueryParams @params);
        Task<List<CompletedOrders>> GetOrdersByTypeAsync( OrderQueryParams @params);
        Task<List<CompletedOrders>> GetAllOrdersAsync(OrderQueryParams @params);
    }

}
