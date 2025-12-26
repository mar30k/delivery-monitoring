using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Filters;
using DeliveryMonitoring.Helpers;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services.Api;
using DeliveryMonitoring.Services.Cache;
using DeliveryMonitoring.Services.Orders;
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
        private readonly ICompletedOrdersService _ordersService;
        private const string DineInTableId = AppConstants.TableIds.DineIn;
        private const string TakeAwayTableId = AppConstants.TableIds.TakeAway;
        private const string DeliveryTableId = AppConstants.TableIds.Delivery;
        private const string ScheduledDeliveryTableId = AppConstants.TableIds.ScheduledDelivery; 
        private const string ScheduledTakeawayTableId = AppConstants.TableIds.ScheduledPickUp;
        private const string AdminCompanyTin = AppConstants.Company.AdminTin;
        private string CompanyTin => _authenticationManager.GetSecureCookie(CNET_WebConstantes.IdentificationCookie) ?? string.Empty;
        #endregion

        #region Constructor
        public CompletedOrdersController(
            ICompletedOrdersService ordersService,
            IApiRequestService apiRequestService,
            IHttpContextAccessor httpContextAccessor,
            AuthenticationManager authenticationManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _authenticationManager = authenticationManager;
            _apiRequestService = apiRequestService;
            _ordersService = ordersService;
        }
        #endregion

        #region Views
        public async Task<IActionResult> Index()
        {
            var viewModel = new CompletedOrdersViewModel
            {
                CompanyTin = CompanyTin,
                OrderTables = TableConfigFactory.CreateCompletedOrderTables()
            };

            try
            {
                var purposeResponse = await _apiRequestService.GetDeliveryPurposeAsync();
                var purposeOptions = JsonConvert.DeserializeObject<Dictionary<int, string>>(purposeResponse);
                viewModel.PurposeOptions = purposeOptions ?? new Dictionary<int, string>();
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Unable to connect to the service. Please try again later.";
            }
            catch (JsonException)
            {
                ViewBag.ErrorMessage = "Invalid JSON received from the service.";
            }

            return View(viewModel);
        }

        [Route("/pending")]
        public IActionResult PendingOrders()
        {
            var viewModel = new TableConfig
            {
                TableId = "pendingOrders",
                Title = "Pending Orders",
                AjaxUrl = "/getPendingOrders",
                SheetName = "Pending Orders",
                Type = "pendingOrders"
            };
            return View(viewModel);
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
                TakeAwayTableId => DeliveryOrderTypes.PickUpAtBranch,
                DineInTableId => DeliveryOrderTypes.InHouseDining,
                ScheduledDeliveryTableId => DeliveryOrderTypes.ScheduledDeliveryToLocation,
                ScheduledTakeawayTableId => DeliveryOrderTypes.ScheduledPickUp,
                _ => DeliveryOrderTypes.DeliveryToLocation
            };

            var result = orderType == DeliveryOrderTypes.DeliveryToLocation
                ? await _apiRequestService.GetCompletedOrdersAsync(skipCache: false)
                : await _apiRequestService.GetOrdersByTypeAsync((int)orderType, skipCache:false);

            if (result == null)
            {
                TempData["Message"] = $"Unable to fetch details of Order: {voucher}.";
                return RedirectToAction("Index");
            }

            var order = result.Data?.FirstOrDefault(o => o.VoucherCode == voucher);
            if (CompanyTin != AdminCompanyTin && CompanyTin != order?.Tin)
            {
                TempData["Message"] = $"You do not have the necessary permissions to view Order: {voucher}.";
                return RedirectToAction("Index");
            }

            var voucherDetail = await _apiRequestService.Gethistorydetail(voucher, order?.CompanyCode.ToString() ?? "", skipCache: false);

            var driver = await _apiRequestService.GetDriverDetailsByPhoneNumber<Driver>(phoneNumber: order?.DriverPhoneNumber ?? "", skipCache: false);

            var driverActivity = await _apiRequestService.GetDriverActivityAsync(order?.CompanyCode.ToString() ?? "", voucher, skipCache: false);

            var viewModel = new OrderDetail
            {
                CustomerFirstName = order?.FirstName,
                BranchName = order?.BranchName,
                SupervisedBy = order?.SupervisorPhoneNumber,
                SupervisorName = order?.SupervisorName,
                AssignedDriverPhoneNumber = order?.DriverPhoneNumber,
                LineItemsDetail = voucherDetail?.Data,
                Activities = driverActivity?.Data,
                VoucherCode = voucher,
                AssignedDriverName = driver?.Detail?.FullName,
            };

            return View(viewModel);
        }
        #endregion

        #region APIs
        /// <summary>
        /// Fetches completed orders filtered by company and optionally by date range.
        /// </summary>
        [HttpGet("/getCompletedOrders")]
        public async Task<IActionResult> GetCompletedOrders([FromQuery] OrderQueryParams query)
        {
            var result = await _ordersService.GetCompletedOrdersAsync(query);
            return Ok(result);
        }
        /// <summary>
        /// Fetches completed orders filtered by company and optionally by date range.
        /// </summary>
        [HttpGet("/getPendingOrders")]
        public async Task<IActionResult> GetPendingOrders([FromQuery] OrderQueryParams query)
        {
            var result = await _ordersService.GetPendingOrdersAsync(query);
            return Ok(result);
        }

        /// <summary>
        /// Fetches completed orders by type and filters by company and date range.
        /// </summary>
        [HttpGet("/getordersbytype")]
        public async Task<IActionResult> GetOrdersByType(
            [FromQuery] OrderQueryParams query)
        {
            var orders = await _ordersService.GetOrdersByTypeAsync(query);
            return Ok(orders);
        }

        [HttpGet("/getAllOrders")]
        public async Task<IActionResult> GetAllOrders([FromQuery] OrderQueryParams query)
        {

            var orders = await _ordersService.GetAllOrdersAsync(query);
            return Ok(orders);
        }
        /// <summary>
        /// Saves a review or note for a completed order.
        /// </summary>
        [HttpPost("/savenote")]
        public async Task<IActionResult> SaveOrderReview([FromBody] CompletedOrders request)
        {
            if (request == null)
                return BadRequest("Request cannot be null.");

            // Manual validation since properties are nullable
            if (string.IsNullOrWhiteSpace(request.VoucherCode))
                return BadRequest("VoucherCode is required.");
            if (string.IsNullOrWhiteSpace(request.Note))
                return BadRequest("Note is required.");
            if (string.IsNullOrWhiteSpace(request.Purpose))
                return BadRequest("Purpose is required.");
            try
            {
                var response = await OrderHelpers.BuildSupervisorNoteAsync(
                    request,
                    _authenticationManager,
                    _apiRequestService
                );
                
                if(response.IsSuccessful)
                {
                    request.Note = response.Data ?? "";
                }
                else
                {
                    return BadRequest(
                        response.ErrorMessages != null && response.ErrorMessages.Any()
                            ? string.Join(", ", response.ErrorMessages)
                            : "Failed to save note."
                    );
                }

                var result = await _apiRequestService.SaveDeliveryNote(
                    request.VoucherCode,
                    request.Note,
                    request.Purpose
                );

                if (!result.IsSuccessful)
                    return BadRequest(
                        result.ErrorMessages != null && result.ErrorMessages.Any()
                            ? string.Join(", ", result.ErrorMessages)
                            : "Failed to save delivery note."
                    );

                return Ok(result.IsSuccessful);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return BadRequest("An unexpected error occurred.");
            }
        }
        /// <summary>
        /// Saves a review or note for a completed order.
        /// </summary>
        [HttpPost("/completePendingOrder")]
        public async Task<IActionResult> CompletePendingOrderAsync([FromBody] CompletedOrders request)
        {
            if (request == null)
                return BadRequest("Request cannot be null.");
            if (string.IsNullOrWhiteSpace(request.VoucherCode))
                return BadRequest("VoucherCode is required.");
            if (string.IsNullOrWhiteSpace(request.DriverPhoneNumber))
                return BadRequest("Driver Phone is required.");
            if (request?.Eta == null)
                return BadRequest("ETA is required.");
            if (request?.Duration ==  null)
                return BadRequest("Duration is required.");
            try
            {
                var supervisorResponse =  await OrderHelpers.GetAuthenticatedSupervisorAsync(
                    _authenticationManager,
                    _apiRequestService
                );

                if (!supervisorResponse.IsSuccessful || supervisorResponse.Data == null)
                {
                    return BadRequest(supervisorResponse.ErrorMessages != null && supervisorResponse.ErrorMessages.Any()
                            ? string.Join(", ", supervisorResponse.ErrorMessages)
                            : "Faile To Complete Pending Order.");
                }

                var result = await _apiRequestService.CompletePendingOrdersAsync(request);
                

                if (!result.IsSuccessful)
                    return BadRequest(
                        result.ErrorMessages != null && result.ErrorMessages.Any()
                            ? string.Join(", ", result.ErrorMessages)
                            : "Failed to Complete Pending Order."
                    );

                return Ok(result.IsSuccessful);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return BadRequest("An unexpected error occurred.");
            }
        }
        /// <summary>
        /// Retrieves delivery activity for a specific voucher.
        /// </summary>
        [HttpGet("/getDeliveryActivity")]
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