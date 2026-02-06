using DeliveryMonitoring.Constants.Enums;
using DeliveryMonitoring.Helpers;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services.SummaryReport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;

namespace DeliveryMonitoring.Controllers
{
    [Authorize]
    public class SummaryController : Controller
    {
        private readonly ISummaryReportService _summaryReportService;
        public SummaryController(
            ISummaryReportService summaryReportService)
        {
            _summaryReportService = summaryReportService;
        }
        [HttpGet("/summary/{type?}")]
        public IActionResult Index(SummaryType type = SummaryType.Consignee )
        {
            var config = TableConfigFactory.CreateSummary(type); // encapsulate config in one place
            return View(config);
        }

        [HttpGet("/summary/data")]
        public async Task<IActionResult> SummaryData(OrderQueryParams queryParams)
        {
            try
            {
                var data = queryParams.SummaryType switch
                {
                    SummaryType.Merchant =>
                        (object) await _summaryReportService.MerchantSummary(queryParams),

                    SummaryType.Driver =>
                        (object) await _summaryReportService.DriverSummary(queryParams),

                    SummaryType.Supervisor =>
                        (object) await _summaryReportService.SupervisorSummary(queryParams),

                    _ =>
                       (object) await _summaryReportService.ConsigneeSummary(queryParams),
                };
                return Json(new { data });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    data = new List<object>(),
                    error = $"Failed to generate summary data. {ex.Message}"
                });
            }
        }
    }
}
