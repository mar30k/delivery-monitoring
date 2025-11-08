using DeliveryMonitoring.Constants;

namespace DeliveryMonitoring.Services.SummaryReport
{
    public interface ISummaryReportService
    {
        Task<IEnumerable<object>> GetSummaryDataAsync(
            SummaryType type,
            DateTime? startDate,
            DateTime? endDate,
            bool isClear);
    }
}
