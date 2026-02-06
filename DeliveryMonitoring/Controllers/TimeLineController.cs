using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Tweetinvi.Core.Events;
using static NuGet.Packaging.PackagingConstants;

namespace DeliveryMonitoring.Controllers
{
    [Authorize]
    [Route("/timeLine")]
    public class TimeLineController : Controller
    {
        private readonly IApiRequestService _apiRequestService;
        public TimeLineController(IApiRequestService apiRequestService)
        {
            _apiRequestService = apiRequestService;
        }
        public IActionResult Index()
        {
            return View(new List<CompletedOrders>());
        }
        [HttpGet("/gettimeLineOrder")]
        public async Task<IActionResult> GetCompletedOrdersWithTimeline([FromQuery] OrderQueryParams @params, [FromQuery] string filter = "all")
        {
            var today = DateTime.Today;

            DateTime sDate = @params.StartDate ?? today;
            DateTime eDate = @params.EndDate ?? today;

            bool skipCache = !@params.StartDate.HasValue ||
                 !@params.EndDate.HasValue ||
                 (sDate.Date <= today && eDate.Date >= today);


            var response = await _apiRequestService
                .GetCompletedordersWithTimeineAsync(
                    sDate.ToString("yyyy-MM-dd"),
                    eDate.ToString("yyyy-MM-dd"),
                    skipCache);

            var orders = response.Data?.ToList() ?? new List<CompletedOrders>();


            var hashInput = JsonSerializer.Serialize(orders.Select(o => new { o.VoucherCode, o.Activities?.Activity?.Count }));
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(hashInput));
            var currentHash = Convert.ToBase64String(hashBytes);
            Response.Headers.Add("X-Data-Hash", currentHash);
            ViewData["CurrentFilter"] = filter;
            return PartialView("_OrderTimelinePartial", orders);
        }
    }
}
