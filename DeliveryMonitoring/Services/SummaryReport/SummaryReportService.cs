using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Controllers;
using DeliveryMonitoring.Helpers;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services.Api;

namespace DeliveryMonitoring.Services.SummaryReport
{
    public class SummaryReportService : ISummaryReportService
    {
        private readonly IApiRequestService _api;
        private readonly AuthenticationManager _auth;

        private string CompanyTin =>
            _auth.GetSecureCookie(CNET_WebConstantes.IdentificationCookie) ?? string.Empty;

        public SummaryReportService(
            IApiRequestService api,
            AuthenticationManager auth)
        {
            _api = api;
            _auth = auth;
        }

        public async Task<IEnumerable<MerchantSummary>> MerchantSummary(OrderQueryParams p)
        {
            var orders = await OrderQueryHelper.GetAllOrdersAsync(_api, CompanyTin, p);
            return SummaryBuilders.BuildMerchantSummary(orders);
        }

        public async Task<IEnumerable<ConsigneeSummary>> ConsigneeSummary(OrderQueryParams p)
        {
            var orders = await OrderQueryHelper.GetAllOrdersAsync(_api, CompanyTin, p);
            return SummaryBuilders.BuildConsigneeSummary(orders);
        }

        public async Task<IEnumerable<DriverSummary>> DriverSummary(OrderQueryParams p)
        {
            var orders = await OrderQueryHelper.GetCompletedOrdersAsync(_api, CompanyTin, p);
            var drivers = await _api.GetAvailableDriversAsync(
                OrderHelpers.IsTodayIncluded(p) || p.IsClear);

            return SummaryBuilders.BuildDriverSummary(orders, drivers);
        }

        public async Task<IEnumerable<SupervisorSummary>> SupervisorSummary(OrderQueryParams p)
        {
            var orders = await OrderQueryHelper.GetCompletedOrdersAsync(_api, CompanyTin, p);
            return SummaryBuilders.BuildSupervisorSummary(orders);
        }
    }
}
