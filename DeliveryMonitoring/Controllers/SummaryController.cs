using DeliveryMonitoring.Constants;
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
        public IActionResult Index(SummaryReportType type = SummaryReportType.Consignee )
        {
            var config = TableConfigFactory.CreateSummary(type); // encapsulate config in one place
            return View(config);
        }

        [HttpGet("/summary/data")]
        public async Task<IActionResult> SummaryData(SummaryType type, DateTime? startDate, DateTime? endDate, bool isClear)
        {
            try
            {
                var result = await _summaryReportService.GetSummaryDataAsync(
                    type, startDate, endDate, isClear);

                return Json(new { data = result});
            }
            catch (Exception ex)
            {
                return Json(new { data = new List<object>(), error = $"Failed to generate summary data.{ex.Message}" });
            }
        }
    }
}
