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
            return View();
        }
        [HttpGet("/gettimeLineOrder")]
        public async Task<IActionResult> GetCompletedOrdersWithTimeline([FromQuery] OrderQueryParams @params)
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

            foreach (var order in orders)
            {
                foreach (var activity in order.Activities?.Activity ?? new List<ActivityResponse>())
                {
                    // Try to find the key by value
                    var matchingPair = DeliveryActivities.FirstOrDefault(kv => kv.Value == activity.Name);

                    // If found, use the key; otherwise, fallback to original name
                    activity.ActivityName = !string.IsNullOrEmpty(matchingPair.Key)
                        ? matchingPair.Key
                        : activity.Name;
                }
            }
            var hashInput = JsonSerializer.Serialize(orders.Select(o => new { o.VoucherCode, o.Activities?.Activity?.Count }));
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(hashInput));
            var currentHash = Convert.ToBase64String(hashBytes);
            Response.Headers.Add("X-Data-Hash", currentHash);
            return PartialView("_OrderTimelinePartial", orders);
        }
        public static Dictionary<string, string> DeliveryActivities = new()
        {
            {"sent", "Order Placed"},
            {"prepared", "Your order Invoice is printed"},
            {"received", "Order received and is being prepared"},
            {"accepted", "Order Delivery accepted by the Driver"},
            {"seen", "Order Delivery accepted by the Supervisor"},
            {"declined", "Order Delivery declined by the Driver"},
            {"assigned", "Order Delivery assigned to a Driver"},
            {"drivernotfound", "No Driver found for your Delivery"},
            {"completed", "Order Delivery completed"},
            {"sos", "Delivery issue reported."},
            {"ontheway", "Your order is picked up & driver is on the way"},
            {"arrived", "Driver has arrived at the destination"},
            {"arrivedatbranch", "Driver has arrived at the pickup location"},
            {"done", "Kitchen has finished cooking your order"},
        };
    }
}
