using System.Threading.Tasks;

namespace DeliveryMonitoring.Services
{
    public interface IApiRequestService
    {
        Task<string> GetCompletedOrdersRawAsync();
        Task<string> GetOrdersByTypeRawAsync(int type);
        Task<string> GetDeliveryPurposeRawAsync();
    }
}