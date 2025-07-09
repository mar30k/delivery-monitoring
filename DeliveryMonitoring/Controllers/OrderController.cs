using CNET_ERP_V7.WebConstants;
using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using static NuGet.Packaging.PackagingConstants;

namespace DeliveryMonitoring.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private IHttpContextAccessor _httpContextAccessor;
        //HttpClient Setup starts here
        private readonly IHttpClientFactory _httpClientFactory;
        public OrderController(IHttpClientFactory httpClientFactory , IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }
        //HttpClient Setup ends here

        //List of Orders Page -- Starts Here
        [HttpGet]
        [Route("/orders")]
        public async Task<IActionResult> Index()
        {
            var _client = _httpClientFactory.CreateClient("Delivery");
            var _V7client = _httpClientFactory.CreateClient("CnetApiBaseUrl");
            List<OrderDetail>? orders = new ();
            List<SupervisorsDTO>? superVisors = new ();
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];
            if (string.IsNullOrWhiteSpace(companyTin) || string.IsNullOrWhiteSpace(companyTin))
            {
                return RedirectToAction("Logout", "Login");
            }

            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + $"/orderRequests?companyTin={companyTin}");
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                orders = JsonConvert.DeserializeObject<List<OrderDetail>>(data);
            }
            HttpResponseMessage getsupervisors = await _V7client.GetAsync(_V7client.BaseAddress + "auth/getsupervisors");      
            if (getsupervisors.IsSuccessStatusCode)
            {
                string data = await getsupervisors.Content.ReadAsStringAsync();
                superVisors = JsonConvert.DeserializeObject<List<SupervisorsDTO>>(data);
            }
            var orderViewModel = new OrderViewModel
            {
                OrderDetail = orders,
                Supervisors = superVisors
            };
            return View(orderViewModel);
        }
        //List of Orders Page -- Ends Here

        //Order Details Page -- Starts Here
        [HttpGet("/Order/{voucherCode}")]
        public async Task<IActionResult> Details(string voucherCode)
        { 
            var _client = _httpClientFactory.CreateClient("Delivery");
            var _V7client = _httpClientFactory.CreateClient("CnetApiBaseUrl");
            OrderDetail? order = null;
            List<SupervisorsDTO>? superVisors = new();
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"{_client.BaseAddress}/orderRequests/{voucherCode}");

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    order = JsonConvert.DeserializeObject<OrderDetail>(data);
                    //order.assignedDriverPhoneNumber = "0924438476";
                }
                HttpResponseMessage getsupervisors = await _V7client.GetAsync(_V7client.BaseAddress + "auth/getsupervisors");
                if (getsupervisors.IsSuccessStatusCode)
                {
                    string data = await getsupervisors.Content.ReadAsStringAsync();
                    superVisors = JsonConvert.DeserializeObject<List<SupervisorsDTO>>(data);
                }
                if (order == null)
                {
                    //return RedirectToAction("index");
                    //var sampleOrder = GetSampleOrder();
                    //return View(sampleOrder);
                    return RedirectToAction("Index");
                }
                else
                {
                    var supervisor = superVisors?.FirstOrDefault(s => s.UserName == order.SupervisedBy);
                    order.SupervisorName = supervisor?.FirstName + supervisor?.SecondName;
                }
                return View(order);
            }
            catch (HttpRequestException)
            {
                return StatusCode(500);
            }
        }

        private OrderDetail GetSampleOrder()
        {
            return new OrderDetail
            {
                Id = "ORD123456",
                AssignedDriverPhoneNumber = "0912345678",
                BranchName = "Addis Branch",
                CompanyCode = 1001,
                CompanyName = "Tech Logistics",
                CompanyTin = "1234567890",
                DeliveryTin = "0987654321",
                SupervisedBy = "SUP001",
                SupervisorName = "Mr. Dawit",
                SosReason = "Delayed",
                GrandTotal = "1500.00",
                Platform = "Web",
                RequestCreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                CreatedAtString = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                RequestCreatedAtIso = DateTime.UtcNow,
                DriverAssignedTime = DateTime.UtcNow.AddMinutes(-10),
                DeliveryDateTime = DateTime.UtcNow.AddHours(1),
                CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                UpdatedAt = DateTime.UtcNow,
                Eta = DateTime.UtcNow.AddHours(2),
                Status = "In Transit",
                TargetBranchLocation = new Location
                {
                    lat = 8.9806,
                    lng = 38.7578
                },
                TargetBranchLat = 8.9806,
                TargetBranchLng = 38.7578,
                VoucherCode = "PROMO2025",
                Alert = "Check Package",
                ExceptDrivers = "DRV002,DRV003",

                Customer = new CustomerDetail
                {
                    DeviceID = "DEV123",
                    FirstName = "Abebe",
                    GeocodeAddress = "Bole Medhanialem, Addis Ababa",
                    PhoneNumber = "0911122233",
                    SpecificAddress = "Behind XYZ Building",
                    LatLng = new Location
                    {
                        lat = 8.998812,
                        lng = 38.785802
                    }
                },

                CustomerDeviceID = "DEV123",
                CustomerFirstName = "Abebe",
                CustomerGeocodeAddress = "Bole Medhanialem, Addis Ababa",
                CustomerLat = 8.998812,
                CustomerLng = 38.785802,
                CustomerPhoneNumber = "0911122233",
                CustomerSpecificAddress = "Behind XYZ Building",
                DriverAssignedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                IsAssignedAck = true,
                IsNoDriversAck = false,
                OrderArrivedAckByCustomer = false,
                OrderArrivedAckByDriver = true,

                LineItemsDetail = new LineItemsDetail
                {
                    LineItems = new List<LineItem>
        {
            new LineItem
            {
                Article = 101,
                Name = "Laptop",
                UnitAmount = 1200.00m,
                Quantity = 1,
                TaxableAmount = 1200.00m
            },
            new LineItem
            {
                Article = 202,
                Name = "Mouse",
                UnitAmount = 100.00m,
                Quantity = 2,
                TaxableAmount = 200.00m
            }
        },
                    ExtraCharge = new Dictionary<string, decimal>
        {
            { "VAT", 150.00m },
            { "Delivery", 50.00m }
        },
                    GrandTotal = 1500.00m,
                    ExtraInformation = new Dictionary<string, object>
        {
            { "DeliveredBy", "Drone" },
            { "Packaging", "Eco-friendly" }
        },
                    ExtraData = new ExtraData
                    {
                        VoucherId = 555,
                        Tin = "1234567890"
                    },
                    IssuedDate = DateTime.UtcNow,
                    BranchCode = 10,
                    PromoDetail = "10% Discount",
                    PhoneNumber = "0911122233",
                    CompanyName = "Tech Logistics",
                    VoucherCode = "PROMO2025"
                },

                Activities = new Activities
                {
                    StartTime = DateTime.UtcNow.AddHours(-1),
                    CurrentTime = DateTime.UtcNow,
                    Eta = DateTime.UtcNow.AddHours(1),
                    ActualArrival = null,
                    Alert = "Driver Delayed",
                    ActivityResponse = new List<ActivityResponse>
        {
            new ActivityResponse
            {
                Name = "Picked Up",
                Time = DateTime.UtcNow.AddMinutes(-30),
                TimeElapsed = "30 minutes ago"
            },
            new ActivityResponse
            {
                Name = "En Route",
                Time = DateTime.UtcNow.AddMinutes(-10),
                TimeElapsed = "10 minutes ago"
            }
            }
                },

                OrderAcceptedNotification = DateTime.UtcNow.AddMinutes(-20),
                OrderReceiveNotification = DateTime.UtcNow.AddMinutes(-5)
            };
        }
       
        [HttpPost]
        public async Task<IActionResult> Dispatch([FromBody] OrderDetail order)
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
            if (order == null)
                return BadRequest("Invalid order data.");

            var client = _httpClientFactory.CreateClient("CnetApiBaseUrl");
            var jsonBody = JsonConvert.SerializeObject(order);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("driver/dispatch", content); // dispatch

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Redispatch failed: {error}");
                }

                var responseData = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Boolean>(responseData);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Exception: {ex.Message}");
            }
        }
        

        [HttpPost]
        public async Task<IActionResult> OrderDetails([FromBody] OrderDetail order)
        {
            if (order.VoucherCode == null)
                return BadRequest("Invalid voucher code.");

            var _client = _httpClientFactory.CreateClient("Delivery");
            try
            {

                HttpResponseMessage response = await _client.GetAsync($"{_client.BaseAddress}/orderRequests/{order.VoucherCode?.ToString()}");

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Getting Order Detail Failed!: {error}");
                }

                var responseData = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<OrderDetail>(responseData);
                var isRedespatchAble = new[] { "drivernotfound", "declined", "requested" ,"sos", "assigned" }
                    .Contains(result?.Status, StringComparer.OrdinalIgnoreCase);
                
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

            var _client = _httpClientFactory.CreateClient("CnetApiBaseUrl");
            try
            {
                HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + "auth/getsupervisors");

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Redispatch failed: {error}");
                }
                string data = await response.Content.ReadAsStringAsync();
                var supervisors = JsonConvert.DeserializeObject<List<SupervisorsDTO>>(data) ?? new List<SupervisorsDTO>();

                return Ok(supervisors ?? new List<SupervisorsDTO>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Exception: {ex.Message}");
            }
        }
        public class AssignSuperVisorDTO
        {
            public string? voucherCode { get; set; }
            public string? id { get; set; }
        }
        [HttpPost]
        public async Task<IActionResult> AssignSupervisor([FromBody] AssignSuperVisorDTO assignSuperVisorDTO)
        {
            if (assignSuperVisorDTO.voucherCode == null)
                return BadRequest("Invalid voucher data.");

            var client = _httpClientFactory.CreateClient("CnetApiBaseUrl");
            var uri = assignSuperVisorDTO.id == "all" ?
                $"auth/assign?voucherCode={assignSuperVisorDTO.voucherCode}" :
                $"auth/manualassign?voucherCode={assignSuperVisorDTO.voucherCode}&id={assignSuperVisorDTO.id}";


            try
            {
                var response = await client.GetAsync(client.BaseAddress + uri);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode);
                }

                var responseData = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Boolean>(responseData);
                return result ? Ok(result) : BadRequest("Unable To Assign Supervisor!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Exception: {ex.Message}");
            }
        }
    }
}
