using DeliveryMonitoring.Models;

namespace DeliveryMonitoring.Services.Orders
{
    public interface ICompletedOrdersService
    {
        Task<HulubejeResponse<List<CompletedOrders>>> GetCompletedOrdersAsync(DateTime? startDate, DateTime? endDate, bool isClear);
        Task<List<CompletedOrders>> GetOrdersByTypeAsync(int type, DateTime? startDate, DateTime? endDate, bool isClear);
        Task<List<CompletedOrders>> GetAllOrdersAsync(DateTime? startDate, DateTime? endDate, bool isClear);
    }

}
