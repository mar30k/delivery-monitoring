using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Constants.Enums;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services.Api;
using Microsoft.AspNetCore.Mvc;
using static DeliveryMonitoring.Constants.AppConstants;

namespace DeliveryMonitoring.Controllers
{
    [Route("/analytics")]
    public class AnalyticsController : Controller
    {
        private readonly IApiRequestService _apiRequestService;
        private readonly AuthenticationManager _authenticationManager;
        public AnalyticsController(
             IApiRequestService apiRequestService,
            AuthenticationManager authenticationManager)
        {
            _apiRequestService = apiRequestService;
            _authenticationManager = authenticationManager;
        }
        
        public async Task<IActionResult> Index(string companyTin)
        {
            var company = await _apiRequestService.GetCompaniesAsync();
            var viewModel = new HomeViewModel
            {
                Comps = company,
                CompanyTin = companyTin,
            };
            return View(viewModel);
        }
        [Route("/getCompletedOrdersDashboard")]
        public async Task<IActionResult> GetCompletedOrders()
        {
            var orders = await _apiRequestService.GetCompletedOrdersAsync();
            return Ok(orders);
        }

        [Route("getorders")]
        public async Task<List<OrderDetail>?> GetOrders()
        {
            return await _apiRequestService.GetOrderRequestsAsync(tin: "0076217301");
        }

        [HttpGet("driver")]
        public async Task<IActionResult> LiveLocation()
        {
            return Ok(await _apiRequestService.GetAvailableDriversAsync()
                          ?? new List<Driver>());
        }

        [HttpGet("getDeviceControl")]
        public async Task<List<DeviceControl>?> GetDeviceControlByDate(string date)
        {
            date ??= DateTime.Now.ToString("yyyy-MM-dd");
            var deviceControl = await _apiRequestService.GetDeviceControlAsync(date);
            var latestByTinAndBranch = deviceControl?
                .Where(d => d.TimeStamp.HasValue) // Ensure TimeStamp is not null
                .GroupBy(d => new { d.Tin, d.BranchName, d.DeviceName }) // Group by Tin , BranchName and DeviceName
                .Select(g => g.OrderByDescending(d => d.TimeStamp).First()) // Get the one with latest TimeStamp
                .ToList();

            // Filter out items with Note starting with "09"
            var result = latestByTinAndBranch?
                .Where(s => string.IsNullOrEmpty(s.Note) || !s.Note.StartsWith("09"))
                .ToList();
            return result;
        }

        [HttpGet("getAvailableSupervisors")]
        public async Task<IActionResult> GetAvailableSupervisors()
        {
            try
            {
                var supervisors = await _apiRequestService.GetSupervisorsAsync();
                return Ok(supervisors ?? new List<SupervisorsDTO>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Exception: {ex.Message}");
            }
        }

        [HttpGet("getChartData")]
        public async Task<IActionResult> GetChartData()
        {
            var dineInTask = _apiRequestService.GetOrdersByTypeAsync((int)DeliveryOrderType.InHouseDining);
            var takeAwayTask = _apiRequestService.GetOrdersByTypeAsync((int)DeliveryOrderType.PickUpAtBranch);
            var scheduledDeliveryTask = _apiRequestService.GetOrdersByTypeAsync((int)DeliveryOrderType.ScheduledDeliveryToLocation);
            var scheduledPickUpTask = _apiRequestService.GetOrdersByTypeAsync((int)DeliveryOrderType.ScheduledPickUp);
            var deliveryTask = _apiRequestService.GetCompletedOrdersAsync();

            await Task.WhenAll(dineInTask, takeAwayTask, scheduledDeliveryTask, scheduledPickUpTask, deliveryTask);

            var dineInOrders = (await dineInTask).Data ?? new List<CompletedOrders>();
            var takeAwayOrders = (await takeAwayTask).Data ?? new List<CompletedOrders>();
            var scheduledDeliveryOrders = (await scheduledDeliveryTask).Data ?? new List<CompletedOrders>();
            var scheduledPickUpOrders = (await scheduledPickUpTask).Data ?? new List<CompletedOrders>();
            var deliveryOrders = (await deliveryTask).Data ?? new List<CompletedOrders>();

            var chartData = new[]
            {
                new
                {
                    tableId = AppConstants.TableIds.TakeAway,
                    label = "Takeaway",
                    count = takeAwayOrders.Count,
                    total = takeAwayOrders.Sum(o => o.TotalAmount),
                    index = 0
                },
                new
                {
                    tableId = AppConstants.TableIds.Delivery,
                    label = "Delivery",
                    count = deliveryOrders.Count,
                    total = deliveryOrders.Sum(o => o.TotalAmount),
                    index = 1
                },
                new
                {
                    tableId = AppConstants.TableIds.DineIn,
                    label = "Dine-in",
                    count = dineInOrders.Count,
                    total = dineInOrders.Sum(o => o.TotalAmount),
                    index = 2
                },
                new
                {
                    tableId = AppConstants.TableIds.ScheduledDelivery,
                    label = "Scheduled Delivery",
                    count = scheduledDeliveryOrders.Count,
                    total = scheduledDeliveryOrders.Sum(o => o.TotalAmount),
                    index = 3
                },
                new
                {
                    tableId = AppConstants.TableIds.ScheduledPickUp,
                    label = "Scheduled Takeaway",
                    count = scheduledPickUpOrders.Count,
                    total = scheduledPickUpOrders.Sum(o => o.TotalAmount),
                    index = 4
                }
            };

            return Ok(chartData);
        }
    }
}
