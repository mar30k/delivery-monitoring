using CNET_ERP_V7.WebConstants;
using DeliveryMonitoring.Helpers;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
namespace DeliveryMonitoring.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _env;
        //HttpClient Setup starts here
        private readonly IApiRequestService _apiRequestService;
        public OrderController(
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment env,
            IApiRequestService apiRequestService)
        {

            _httpContextAccessor = httpContextAccessor;
            _env = env;
            _apiRequestService = apiRequestService;
        }
        //HttpClient Setup ends here

        //List of Orders Page -- Starts Here
        [HttpGet]
        [Route("/orders")]
        public async Task<IActionResult> Index()
        {
            List<OrderDetail>? orders = new();
            List<SupervisorsDTO>? superVisors = new();
            try
            {            
                var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];
                if (string.IsNullOrWhiteSpace(companyTin))
                {
                    return RedirectToAction("Logout", "Login");
                }

                orders = await _apiRequestService.GetOrderRequestsAsync();
                if (companyTin != "0076217301")
                {
                    orders = orders?.Where(o => o.DeliveryTin == companyTin).ToList();
                }
                superVisors = await _apiRequestService.GetSupervisorsAsync();
                var orderViewModel = new OrderViewModel
                {
                    OrderDetail = orders,
                    Supervisors = superVisors
                };
                return View(orderViewModel);
            }
            catch (Exception ex)
            {
                return View(null);
            }
        }
        //List of Orders Page -- Ends Here

        //Order Details Page -- Starts Here
        [HttpGet("/Order/{voucherCode}")]
        public async Task<IActionResult> Details(string voucherCode)
        {
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];
            OrderDetail? order = null;
            List<SupervisorsDTO>? superVisors = new();
            try
            {
                order = await _apiRequestService.GetOrderDetailByVoucher(voucherCode);

                if (order == null)
                {
                    if (_env.IsDevelopment())
                        return View(GetSampleOrder.CreateSampleOrder());

                    TempData["Message"] = $"Order {voucherCode}: Not found or failed to load.";
                    return RedirectToAction("Index");
                }

                if (companyTin != "0076217301" && companyTin != order?.DeliveryTin)
                {
                    TempData["Message"] = $"You do not have the necessary permissions to view Order {voucherCode}.";
                    return RedirectToAction("index");
                }
                superVisors = await _apiRequestService.GetSupervisorsAsync();
                if (order == null)
                {
                    TempData["Message"] = $"Order {voucherCode} not found!";
                    return RedirectToAction("index");
                }
                else
                {
                    var supervisor = superVisors?.FirstOrDefault(s => s.UserName == order.SupervisedBy);
                    order.SupervisorName = supervisor?.FirstName + " " + supervisor?.SecondName;
                }
                return View(order);
            }
            catch (HttpRequestException)
            {
                return StatusCode(500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Dispatch([FromBody] OrderDetail order)
        {
            try
            {
                order.Customer = new CustomerDetail
                {
                    FirstName = order.CustomerFirstName,
                    GeocodeAddress = order.CustomerGeocodeAddress,
                    SpecificAddress = order.CustomerSpecificAddress,
                    PhoneNumber = order.CustomerPhoneNumber,
                    DeviceID = order.CustomerDeviceID,
                    LatLng = new Location
                    {
                        lat = order.CustomerLat,
                        lng = order.CustomerLng
                    }
                };
                order.TargetBranchLocation = new Location
                {
                    lat = order.TargetBranchLat,
                    lng = order.TargetBranchLng
                };
                long unixMilliseconds = new DateTimeOffset(DateTime.Parse(order.CreatedAt.ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind)).ToUnixTimeMilliseconds();
                order.RequestCreatedAtIso = order.CreatedAt;
                order.RequestCreatedAt = unixMilliseconds;
                order.IsAssignedAck = false;
                order.IsNoDriversAck = false;
                order.OrderArrivedAckByCustomer = false;
                order.OrderArrivedAckByDriver = false;
                order.OrderReceiveNotification = null;
                order.Alert = null;
                order.Status = "requested";
                order.DriverAssignedAt = 0;
                order.ExceptDrivers = null;
                if (order == null)
                    return BadRequest("Invalid order data.");

                var redispatchResult = await _apiRequestService.RedispatchDriversAsync(order);
                if(!redispatchResult.IsSuccessful)
                    return StatusCode(500, $"Unable to redispatch the order! {string.Join(", ", redispatchResult.ErrorMessages ?? new List<string>())}");
                return Ok(redispatchResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Exception: {ex.Message}");
            }            
        }
        
        [HttpPost]
        public async Task<IActionResult> CheckRedispatchEligibility([FromBody] OrderDetail order)
        {
            if (order.VoucherCode == null)
                return BadRequest("Invalid voucher code.");

            try
            {
                var orderDetail = await _apiRequestService.GetOrderDetailByVoucher(order.VoucherCode);
                if (orderDetail == null)
                {
                    return StatusCode(500, $"Getting Order Detail Failed!:");
                }
                var isRedespatchAble = new[] { "drivernotfound", "declined", "requested" ,"sos", "assigned" }
                    .Contains(orderDetail?.Status, StringComparer.OrdinalIgnoreCase);
                
                return isRedespatchAble ? Ok(isRedespatchAble) : StatusCode(500, $"Can't Redispatch");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Exception: {ex.Message}");
            }
        }

        [HttpGet("order/getAvailableSupervisors")]
        public async Task<IActionResult> GetAvailableSupervisors()
        {
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];
            try
            {
                var supervisors = await _apiRequestService.GetSupervisorsAsync();
                var completedOrders = await _apiRequestService.GetCompletedOrdersAsync();
                foreach (var supervisor in supervisors ?? new List<SupervisorsDTO>())
                {
                    supervisor.TotalSupervisedOrders = completedOrders?.Data?.Count(x => x.SupervisorPhoneNumber == supervisor.UserName) ?? 0;
                }
                return Ok(supervisors ?? new List<SupervisorsDTO>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Exception: {ex.Message}");
            }
        }

        [Route("/sendAlertMessage")]
        public async Task<IActionResult> SendAlertMessage([FromBody] AlertMessageDto messageDto)
        {

            var response = await _apiRequestService.SendMessageAsync(messageDto);
            if(!response.IsSuccessful)
                return StatusCode(500, $"failed: {response.ErrorMessages?.FirstOrDefault()}");
            return Ok("Message sent successfully.");
        }
        [Route("/supervisorAccept")]
        public async Task<IActionResult> AcceptOrderBySupervisor([FromBody] OrderDetail order)
        {
            var requestPayload = new
            {
                tin = order.CompanyTin,
                voucherCode = order.VoucherCode,
                clientPhoneNumber = order.CustomerPhoneNumber,
                driverPhoneNumber = order.AssignedDriverPhoneNumber,
                status = "seen"
            };

            var response = await _apiRequestService.InsertActivityLogAsync(requestPayload);
            if (!response.IsSuccessful)
                return StatusCode(500, $"failed: {response.ErrorMessages?.FirstOrDefault()}");

            return Ok("Order accepted and activity logged successfully.");
        }

        [HttpGet]
        [Route("/getDeviceID/{phoneNumber}")]
        public async Task<IActionResult> GetDriverDeviceId(string phoneNumber)
        {
            try
            {
                // Deserialize into Driver object
                var driver = await _apiRequestService.GetDriverDetailsByPhoneNumber<Driver>(phoneNumber);

                if (driver == null || string.IsNullOrWhiteSpace(driver.DeviceId))
                {
                    return NotFound("Driver not found or DeviceId missing.");
                }

                return Ok(new
                {
                    driver.DeviceId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> AssignSupervisor([FromBody] AssignSuperVisorDTO assignSuperVisorDTO)
        {
            if (assignSuperVisorDTO.voucherCode == null)
                return BadRequest("Invalid voucher data.");
            var response = await _apiRequestService.ChangeOrderStatusAsync(assignSuperVisorDTO);
            if (!response.IsSuccessful)
                return StatusCode(500, $"failed: {response.ErrorMessages?.FirstOrDefault()}");
            return Ok(response);
        }
        [HttpPost]
        [Route("/changeorderstatus")]
        public async Task<IActionResult> ChangeOrderStatus([FromBody] OrderDetail orderDetail)
        {
            if (orderDetail == null || orderDetail.VoucherCode == null)
                return BadRequest("Invalid voucher data.");
            var param = new
            {
                voucherCode = orderDetail.VoucherCode,
                orderStatus = orderDetail.Status,
                driverPhoneNumber = orderDetail.AssignedDriverPhoneNumber,
                isReassignMode = true
            };

            var response = await _apiRequestService.ChangeOrderStatusAsync(param);
            if (!response.IsSuccessful)
                return StatusCode(500, $"failed: {response.ErrorMessages?.FirstOrDefault()}");
            return Ok(response);
        }
    }
}
