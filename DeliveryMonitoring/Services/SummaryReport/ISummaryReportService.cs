using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Models;

namespace DeliveryMonitoring.Services.SummaryReport
{
    public interface ISummaryReportService
    {

        Task<IEnumerable<MerchantSummary>> BuildMerchantSummary(OrderQueryParams @params);

        Task<IEnumerable<ConsigneeSummary>> BuildConsigneeSummary(OrderQueryParams @params);

        Task<IEnumerable<DriverSummary>> BuildDriverSummary(OrderQueryParams @params);

        Task<IEnumerable<SupervisorSummary>> BuildSupervisorSummary(OrderQueryParams @params);
    }
}
