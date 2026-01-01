using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Helpers;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using static NuGet.Packaging.PackagingConstants;
namespace DeliveryMonitoring.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _env;
        //HttpClient Setup starts here
        private readonly IApiRequestService _apiRequestService;
        private readonly AuthenticationManager _authenticationManager;
        private string CompanyTin => _authenticationManager.GetSecureCookie(CNET_WebConstantes.IdentificationCookie) ?? string.Empty;
        private const string AdminCompanyTin = AppConstants.Company.AdminTin;
        public OrderController(
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment env,
            IApiRequestService apiRequestService,
            AuthenticationManager authenticationManager)
        {

            _httpContextAccessor = httpContextAccessor;
            _env = env;
            _apiRequestService = apiRequestService;
            _authenticationManager = authenticationManager;
        }
        //HttpClient Setup ends here

        //List of Orders Page -- Starts Here
        [HttpGet]
        [Route("/orders")]
        public async Task<IActionResult> Index()
        {
            try
            {            
                if (string.IsNullOrWhiteSpace(CompanyTin))
                {
                    return RedirectToAction("Logout", "Login");
                }
                var superVisors = await _apiRequestService.GetSupervisorsAsync();

                return View(new OrderViewModel
                {
                    Supervisors = superVisors
                });
            }
            catch (Exception)
            {
                return View(null);
            }
        }

        [Route("/GetOrders")]
        public async Task<List<OrderDetail>?> GetOrders()
        {
            try
            {

                if (string.IsNullOrWhiteSpace(CompanyTin)) { return new List<OrderDetail>(); }

                var response = await _apiRequestService.GetOrderRequestsAsync();
                var superVisors = await _apiRequestService.GetSupervisorsAsync();
                if (response.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(CompanyTin) && CompanyTin != AdminCompanyTin)
                    {
                        response = response.Where(order => order.DeliveryTin == CompanyTin).ToList();
                    }
                    response?.ForEach(x =>
                    {
                        // Validate CreatedAt
                        if (x.CreatedAt is DateTime createdAt &&
                            createdAt != DateTime.MinValue)
                        {
                            var createdAtOffset = new DateTimeOffset(
                                    DateTime.SpecifyKind(createdAt, DateTimeKind.Utc))
                                .ToOffset(TimeSpan.FromHours(3));

                            x.CreatedAtString = createdAtOffset.ToString("yyyy-MM-dd hh:mm:ss tt");

                            // Validate ETA
                            if (x.Eta is DateTime eta && eta >= createdAt)
                            {
                                var etaOffset = new DateTimeOffset(
                                        DateTime.SpecifyKind(eta, DateTimeKind.Utc))
                                    .ToOffset(TimeSpan.FromHours(3));

                                x.EtaString = etaOffset.ToString("yyyy-MM-dd hh:mm:ss tt");
                            }
                            else
                            {
                                x.EtaString = null;
                                x.Eta = null;
                            }
                        }
                        else
                        {
                            x.CreatedAtString = null;
                            x.EtaString = null;
                            x.Eta = null;
                            x.CreatedAt = null;
                        }

                        // Supervisor
                        var supervisor = superVisors
                            .FirstOrDefault(sup => sup.UserName == x.SupervisedBy);

                        x.SupervisorName = supervisor != null
                            ? $"{supervisor.FirstName} {supervisor.SecondName}"
                            : null;

                        x.CurrentTime = DateTime.UtcNow;
                    });
                }

                //if (response != null && response.Count == 0 && _env.IsDevelopment())
                //{
                //    return new List<OrderDetail> { GetSampleOrder.CreateSampleOrder() };
                //}
                return response ?? new List<OrderDetail>();
            }
            catch
            {
                return new List<OrderDetail>();
            }
        }
        //Order Details Page -- Starts Here
        [HttpGet("/Order/{voucherCode}")]
        public async Task<IActionResult> Details(string voucherCode)
        {
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

                if (CompanyTin != AdminCompanyTin && CompanyTin != order?.DeliveryTin)
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
                long unixMilliseconds = order.CreatedAt.HasValue
                    ? new DateTimeOffset(DateTime.SpecifyKind(order.CreatedAt.Value, DateTimeKind.Utc))
                        .ToUnixTimeMilliseconds()
                    : 0;
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

        [HttpGet("/getAvailableSupervisors")]
        public async Task<IActionResult> GetAvailableSupervisors()
        {
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
                return StatusCode(500, $"failed: {string.Join("", response.ErrorMessages ?? new List<string>())}");
            return Ok("Message sent successfully.");
        }


        [Route("/supervisorAccept")]
        public async Task<IActionResult> AcceptOrderBySupervisor([FromBody] OrderDetail order)
        {
            // 1. Payload validation
            if (order == null)
            {
                return BadRequest(new HulubejeResponse<object>
                {
                    IsSuccessful = false,
                    ErrorMessages = new() { "Order payload is missing." }
                });
            }

            // 2. Authentication
            var user = _authenticationManager.GetUserFromCookie();
            if (user == null)
            {
                return Unauthorized(new HulubejeResponse<object>
                {
                    IsSuccessful = false,
                    ErrorMessages = new() { "User is not authenticated." }
                });
            }

            // 3. Authorization (Supervisor validation)
            if (!string.Equals(user.UserName, order.SupervisedBy, StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new HulubejeResponse<object>
                    {
                        IsSuccessful = false,
                        ErrorMessages = new()
                        {
                            "You are not the assigned supervisor for this order."
                        }
                    });
            }

            // 4. Prepare API payload
            var requestPayload = new
            {
                tin = order.CompanyTin,
                voucherCode = order.VoucherCode,
                clientPhoneNumber = order.CustomerPhoneNumber,
                driverPhoneNumber = order.AssignedDriverPhoneNumber,
                status = "seen"
            };

            // 5. Call external API
            var apiResponse = await _apiRequestService.InsertActivityLogAsync(requestPayload);

            if (!apiResponse.IsSuccessful)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new HulubejeResponse<object>
                    {
                        IsSuccessful = false,
                        ErrorMessages = apiResponse.ErrorMessages ?? new()
                        {
                    "Unknown error occurred while logging activity."
                        }
                    });
            }

            // 6. Success response
            return Ok(new HulubejeResponse<object>
            {
                IsSuccessful = true,
                Data = new
                {
                    message = "Order accepted and activity logged successfully.",
                    supervisor = user.UserName,
                    order = order.VoucherCode
                }
            });
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
            var response = await _apiRequestService.AssignOrderSupervisorAsync(assignSuperVisorDTO);
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
