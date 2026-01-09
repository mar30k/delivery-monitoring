using DeliveryMonitoring.Models;

namespace DeliveryMonitoring.Services.Orders
{   
    /// <summary>
    /// Provides methods for retrieving, filtering, and preparing order data
    /// (completed, pending, type-specific, and aggregated) for display.
    /// </summary>
    public interface ICompletedOrdersService
    {
        /// <summary>
        /// Retrieves completed orders, applies filtering and display preparation,
        /// and returns an error response if no completed orders are found.
        /// </summary>
        Task<HulubejeResponse<List<CompletedOrders>>> GetCompletedOrdersAsync(OrderQueryParams @params);
        /// <summary>
        /// Retrieves pending orders, applies filtering and display preparation,
        /// and returns an error response if no pending orders are found.
        /// </summary>
        Task<HulubejeResponse<List<CompletedOrders>>> GetPendingOrdersAsync(OrderQueryParams @params);
        /// <summary>
        /// Retrieves orders by delivery type, assigns table identifiers,
        /// prepares display values, and applies filtering based on query parameters.
        /// </summary>
        Task<HulubejeResponse<List<CompletedOrders>>> GetOrdersByTypeAsync( OrderQueryParams @params);
        /// <summary>
        /// Retrieves all order types in parallel, aggregates them into a single list,
        /// prepares display values, and applies filtering based on query parameters.
        /// </summary>
        Task<HulubejeResponse<List<CompletedOrders>>> GetAllOrdersAsync(OrderQueryParams @params);
        /// <summary>
        /// Retrieves all order types in parallel, aggregates them into a single list,
        /// prepares display values, and applies filtering based on query parameters.
        /// </summary>
        Task<HulubejeResponse<List<CompletedOrders>>> GetDeliveryOrdersAsync(OrderQueryParams @params);
    }

}
