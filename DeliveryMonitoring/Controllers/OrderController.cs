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
            HttpResponseMessage getsupervisors = await _V7client.GetAsync(_V7client.BaseAddress + "auth/getsupervisors");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                orders = JsonConvert.DeserializeObject<List<OrderDetail>>(data);
            }
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
            OrderDetail? order = null;

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"{_client.BaseAddress}/orderRequests/{voucherCode}");

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    order = JsonConvert.DeserializeObject<OrderDetail>(data);
                    //order.assignedDriverPhoneNumber = "0924438476";
                }

                if (order == null)
                {
                    return NotFound(); // Return a 404 Not Found response if no driver is found.
                }

                return View(order);
            }
            catch (HttpRequestException)
            {
                return StatusCode(500); // Handle exception with a 500 Internal Server Error
            }
        }
       
        [HttpPost]
        public async Task<IActionResult> Dispatch([FromBody] OrderDetail order)
        {
            order.IsAssignedAck = false;
            order.IsNoDriversAck = false;
            order.OrderArrivedAckByCustomer = false;
            order.OrderArrivedAckByDriver = false;
            order.OrderReceiveNotification = null;
            order.Alert = null;
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
            List<SupervisorsDTO> supervisors = new();
            try
            {
                HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + "auth/getsupervisors");

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Redispatch failed: {error}");
                }
                string data = await response.Content.ReadAsStringAsync();
                supervisors = JsonConvert.DeserializeObject<List<SupervisorsDTO>>(data) ?? new List<SupervisorsDTO>();

                return Ok(supervisors);
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
                $"auth/assign?voucherCode={assignSuperVisorDTO.voucherCode}&id={assignSuperVisorDTO.id}";


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
