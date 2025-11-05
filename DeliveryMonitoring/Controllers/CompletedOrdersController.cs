using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace DeliveryMonitoring.Controllers
{
    /// <summary>
    /// Handles all operations related to completed orders,
    /// including listing, filtering, order details, and saving reviews.
    /// </summary>
    [Authorize]
    [Route("/CompletedOrders")]
    public class CompletedOrdersController : Controller
    {
        #region Fields
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IApiRequestService _apiRequestService;
        private readonly AuthenticationManager _authenticationManager;

        private const string DineInTableId = "dineIn";
        private const string TakeAwayTableId = "takeAway";
        private const string DeliveryTableId = "delivery";
        private string CompanyTin => _authenticationManager.GetSecureCookie(CNET_WebConstantes.IdentificationCookie) ?? string.Empty;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="CompletedOrdersController"/> class.
        /// </summary>
        /// <param name="apiRequestService">API service for backend operations.</param>
        /// <param name="httpContextAccessor">HTTP context accessor for cookie retrieval.</param>
        /// <param name="authenticationManager">Authentication manager for user-related operations.</param>
        public CompletedOrdersController(
            IApiRequestService apiRequestService,
            IHttpContextAccessor httpContextAccessor,
            AuthenticationManager authenticationManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _authenticationManager = authenticationManager;
            _apiRequestService = apiRequestService;
        }
        #endregion

        #region Views
        /// <summary>
        /// Displays the Completed Orders page with purpose options and company info.
        /// </summary>
        /// <returns>The CompletedOrders view.</returns>
        public async Task<IActionResult> Index()
        {
            var CompletedOrdersViewModel = new CompletedOrdersViewModel
            {
                PurposeOptions = new Dictionary<int, string>(), // default empty dictionary
                CompanyTin = CompanyTin
            };

            try
            {
                var purposeResponseData = await _apiRequestService.GetDeliveryPurposeAsync();
                var purposeResult = JsonConvert.DeserializeObject<Dictionary<int, string>>(purposeResponseData);
                CompletedOrdersViewModel.PurposeOptions = purposeResult ?? new Dictionary<int, string>();
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Unable to connect to the service. Please try again later.";
            }
            catch (JsonException)
            {
                ViewBag.ErrorMessage = "Invalid JSON received from the service.";
            }

            return View(CompletedOrdersViewModel);
        }

        /// <summary>
        /// Displays the details for a completed order by voucher.
        /// </summary>
        /// <param name="voucher">The voucher code for the order.</param>
        /// <param name="type">Optional type for filtering by order type.</param>
        /// <returns>The OrderDetail view.</returns>
        [Route("/orderdetail")]
        public async Task<IActionResult> CompletedOrderDetail(string voucher, string type = "")
        {
            DeliveryOrderTypes orderType = type switch
            {
                "takeAwayTable" => DeliveryOrderTypes.PickUpAtBranch,
                "dineInTable" => DeliveryOrderTypes.InHouseDining,
                _ => DeliveryOrderTypes.DeliveryToLocation
            };

            var result = orderType == DeliveryOrderTypes.DeliveryToLocation
                ? await _apiRequestService.GetCompletedOrdersAsync()
                : await _apiRequestService.GetOrdersByTypeAsync((int)orderType);

            if (result == null)
            {
                TempData["Message"] = $"Unable to fetch details of Order: {voucher}.";
                return RedirectToAction("Index");
            }

            var order = result.Data?.FirstOrDefault(o => o.VoucherCode == voucher);
            if (CompanyTin != "0076217301" && CompanyTin != order?.Tin)
            {
                TempData["Message"] = $"You do not have the necessary permissions to view Order: {voucher}.";
                return RedirectToAction("Index");
            }

            var voucherDetail = await _apiRequestService.Gethistorydetail(voucher, order?.CompanyCode.ToString() ?? "");
            var supervisors = await _apiRequestService.GetSupervisorsAsync();
            var supervisor = supervisors?.FirstOrDefault(s => s.UserName == order?.SupervisorPhoneNumber);

            if (order != null)
            {
                order.SupervisorName = $"{supervisor?.FirstName} {supervisor?.SecondName}";
            }

            var driverActivity = await _apiRequestService.GetDriverActivityAsync(order?.CompanyCode.ToString() ?? "", voucher);

            var viewModel = new OrderDetail
            {
                CustomerFirstName = order?.FirstName,
                BranchName = order?.BranchName,
                SupervisedBy = order?.SupervisorPhoneNumber,
                SupervisorName = order?.SupervisorName,
                AssignedDriverPhoneNumber = order?.DriverPhoneNumber,
                LineItemsDetail = voucherDetail.Data,
                Activities = driverActivity?.Data,
                VoucherCode = voucher
            };

            return View(viewModel);
        }
        #endregion

        #region APIs
        /// <summary>
        /// Fetches completed orders filtered by company and optionally by date range.
        /// </summary>
        [HttpGet("/getCompletedOrders")]
        public async Task<IActionResult> GetCompletedOrdersApi([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] bool isClear = false)
        {
            var result = await _apiRequestService.GetCompletedOrdersAsync();
            if (result?.Data == null)
                return NotFound("Failed to retrieve or parse completed orders.");

            result.Data = FilterOrders(result.Data, startDate, endDate, isClear);

            foreach (var item in result.Data)
                FormatRequestDate(item);

            return Ok(result);
        }

        /// <summary>
        /// Fetches completed orders by type and filters by company and date range.
        /// </summary>
        [HttpGet("/getordersbytype")]
        public async Task<IActionResult> GetOrdersByType([FromQuery] int type, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] bool isClear = false)
        {
            var getOrdersByTypeData = await _apiRequestService.GetOrdersByTypeAsync(type);
            if (getOrdersByTypeData?.Data == null)
                return NotFound("Failed to retrieve or parse completed orders.");

            getOrdersByTypeData.Data = FilterOrders(getOrdersByTypeData.Data, startDate, endDate, isClear);

            foreach (var item in getOrdersByTypeData.Data)
            {
                ParseSupervisor(item);
                FormatRequestDate(item);
            }

            return Ok(getOrdersByTypeData);
        }

        [HttpGet("/getAllOrders")]
        public async Task<IActionResult> GetAll([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] bool isClear = false)
        {
            var dineInTask = GetProcessedOrdersByType((int)DeliveryOrderTypes.InHouseDining);
            var takeAwayTask = GetProcessedOrdersByType((int)DeliveryOrderTypes.PickUpAtBranch);
            var deliveryTask = _apiRequestService.GetCompletedOrdersAsync();

            await Task.WhenAll(dineInTask, takeAwayTask, deliveryTask);

            var dineInOrders = await dineInTask;
            var takeAwayOrders = await takeAwayTask;
            var deliveryOrders = await deliveryTask;
            
            foreach (var item in deliveryOrders.Data ?? new List<CompletedOrders>())
            {
                FormatRequestDate(item);
                item.TableId = DeliveryTableId;
            }

            var allOrdersData = (deliveryOrders.Data ?? new List<CompletedOrders>())
                                .Concat(dineInOrders)
                                .Concat(takeAwayOrders)
                                .ToList();

            allOrdersData = FilterOrders(allOrdersData, startDate, endDate, isClear);

            return Ok(new HulubejeResponse<List<CompletedOrders>>
            {
                Data = allOrdersData,
                IsSuccessful = true
            });
        }

        private async Task<List<CompletedOrders>> GetProcessedOrdersByType(int type)
        {
            var ordersData = await _apiRequestService.GetOrdersByTypeAsync(type);
            var orders = ordersData?.Data ?? new List<CompletedOrders>();
            foreach (var order in orders)
            {
                FormatRequestDate(order);
                ParseSupervisor(order);
                order.TableId = type == (int)DeliveryOrderTypes.PickUpAtBranch ? TakeAwayTableId : DineInTableId;
            }
            return orders;
        }

        private List<CompletedOrders> FilterOrders(List<CompletedOrders> orders, DateTime? startDate, DateTime? endDate, bool isClear)
        {
            if (CompanyTin != "0076217301")
                orders = orders.Where(o => o.Tin == CompanyTin).ToList();

            if (!isClear && startDate.HasValue && endDate.HasValue)
                orders = orders.Where(o => o.RequestCreatedAt.Date >= startDate.Value.Date &&
                                           o.RequestCreatedAt.Date <= endDate.Value.Date).ToList();

            return orders;
        }

        private static void ParseSupervisor(CompletedOrders order)
        {
            string supervisorName = "N/A";
            if (!string.IsNullOrEmpty(order.Note) && order.Note.StartsWith("{"))
            {
                var match = Regex.Match(order.Note, @"^\{(.*?)\}");
                if (match.Success)
                {
                    supervisorName = match.Groups[1].Value;
                    order.Note = order.Note[match.Length..].TrimStart();
                }
            }
            order.SupervisorName = supervisorName;
        }

        private static void FormatRequestDate(CompletedOrders order)
        {
            order.RequestCreatedAtString = order.RequestCreatedAt.ToString("yyyy-MM-dd hh:mm tt");
        }
        
        /// <summary>
        /// Saves a review or note for a completed order.
        /// </summary>
        [HttpPost("savenote")]
        public async Task<IActionResult> SaveOrderReview([FromBody] CompletedOrders request)
        {
            if (request == null)
                return BadRequest("Please fill all the required fields.");

            try
            {
                if (!request.IsDelivery)
                {
                    var user = _authenticationManager.GetUserFromCookie();
                    if (user != null)
                    {
                        var supervisors = await _apiRequestService.GetSupervisorsAsync();
                        var supervisor = supervisors.FirstOrDefault(s => s.UserName == user.UserName);
                        if (supervisor != null)
                            request.Note = $"{{{supervisor.FirstName} {supervisor.SecondName}}} {request.Note}";
                        else
                            return BadRequest("Unable to find the supervisor. Please try again!");
                    }
                }

                var result = await _apiRequestService.SaveDeliveryNote(request.VoucherCode ?? "", request.Note ?? "", request.Purpose ?? "");
                if (!result.IsSuccessful)
                    return BadRequest();

                return Ok(result.IsSuccessful);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message[0]);
            }
        }
        /// <summary>
        /// Retrieves delivery activity for a specific voucher.
        /// </summary>
        [HttpGet("getDeliveryActivity")]
        public async Task<IActionResult> GetDeliveryActivity(string voucherCode, string companyCode)
        {
            try
            {
                var response = await _apiRequestService.GetDriverActivityAsync(companyCode, voucherCode);
                if (response == null)
                    return NotFound("Failed to retrieve delivery activity.");

                Response.Headers["Cache-Control"] = "public, max-age=10";
                Response.Headers["Vary"] = "Accept-Encoding";

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message[0]);
            }
        }
        #endregion
    }
}