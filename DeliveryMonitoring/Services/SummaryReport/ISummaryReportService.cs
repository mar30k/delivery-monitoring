using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Models;

namespace DeliveryMonitoring.Services.SummaryReport
{
    public interface ISummaryReportService
    {

        Task<IEnumerable<MerchantSummary>> MerchantSummary(OrderQueryParams @params);

        Task<IEnumerable<ConsigneeSummary>> ConsigneeSummary(OrderQueryParams @params);

        Task<IEnumerable<DriverSummary>> DriverSummary(OrderQueryParams @params);

        Task<IEnumerable<SupervisorSummary>> SupervisorSummary(OrderQueryParams @params);
    }
}
