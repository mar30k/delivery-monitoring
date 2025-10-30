using CNET_ERP_V7.WebConstants;
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
    [Authorize]
    [Route("/CompletedOrders")]
    public class CompletedOrdersController : Controller
    {
        private IHttpContextAccessor _httpContextAccessor;
        private readonly IApiRequestService _apiRequestService;
        private readonly AuthenticationManager _authenticationManager;
        public CompletedOrdersController(IHttpClientFactory httpClientFactory,
            IApiRequestService apiRequestService,
            IHttpContextAccessor httpContextAccessor,
            AuthenticationManager authenticationManager   )
        {
            _httpContextAccessor = httpContextAccessor;
            _authenticationManager = authenticationManager;
            _apiRequestService = apiRequestService;
        }
        public async Task<IActionResult> Index()
        {
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];

            var CompletedOrdersViewModel = new CompletedOrdersViewModel
            {
                PurposeOptions = new Dictionary<int, string>(), // default empty dictionary
                CompanyTin = companyTin
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

            return View(CompletedOrdersViewModel); // always pass a view model
        }

        [HttpGet("/getCompletedOrders")]
        public async Task<IActionResult> GetCompletedOrdersApi(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] bool isClear = false)
        {
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];

            var result = await _apiRequestService.GetCompletedOrdersAsync();
            if (result == null || result.Data == null)
                return NotFound("Failed to retrieve or parse completed orders.");

            // Filter by company
            if (companyTin != "0076217301")
                result.Data = result.Data.Where(order => order.Tin == companyTin).ToList();

            // Apply date filter only if not cleared and dates exist
            if (!isClear && startDate.HasValue && endDate.HasValue)
            {
                result.Data = result.Data
                    .Where(o => o.RequestCreatedAt.Date >= startDate.Value.Date && o.RequestCreatedAt.Date <= endDate.Value.Date)
                    .ToList();
            }

            // Format date
            foreach (var item in result.Data)
                item.RequestCreatedAtString = item.RequestCreatedAt.ToString("yyyy-MM-dd hh:mm tt");

            return Ok(result);
        }

        [HttpGet("/getordersbytype")]
        public async Task<IActionResult> GetOrdersByType(
            [FromQuery] string type,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] bool isClear = false)
        {
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];

            // Fetch data from API
            var getOrdersByTypeData = await _apiRequestService.GetOrdersByTypeAsync(int.TryParse(type, out var intType) ? intType : 0);
            if (getOrdersByTypeData == null || getOrdersByTypeData.Data == null)
                return NotFound("Failed to retrieve or parse completed orders.");

            // Filter by company TIN (if not main company)
            if (companyTin != "0076217301")
            {
                getOrdersByTypeData.Data = getOrdersByTypeData.Data
                    .Where(order => order.Tin == companyTin)
                    .ToList();
            }

            // 🔹 Filter by date range (only if not cleared)
            if (!isClear && startDate.HasValue && endDate.HasValue)
            {
                getOrdersByTypeData.Data = getOrdersByTypeData.Data
                    .Where(o => o.RequestCreatedAt.Date >= startDate.Value.Date &&
                                o.RequestCreatedAt.Date <= endDate.Value.Date)
                    .ToList();
            }
            // Add readable date string
            foreach (var item in getOrdersByTypeData.Data)
            {
                item.RequestCreatedAtString = item.RequestCreatedAt.ToString("yyyy-MM-dd hh:mm tt");
                item.TableId = type == "2076" ? "takeAwayTable" : "dineInTable";
                // Default supervisor name
                string supervisorName = "N/A";

                if (!string.IsNullOrEmpty(item.Note) && item.Note.StartsWith("{"))
                {
                    var match = Regex.Match(item.Note, @"^\{(.*?)\}");
                    if (match.Success)
                    {
                        // Extract supervisor name
                        supervisorName = match.Groups[1].Value;

                        // Remove the {SupervisorName} part from the note
                        item.Note = item.Note.Substring(match.Length).TrimStart();
                    }
                }

                item.SupervisorName = supervisorName;
            }

            return Ok(getOrdersByTypeData);
        }
        [Route("/orderdetail")]
        public async Task<IActionResult> CompletedOrderDetail(string voucher ,string type = "")
        {
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];
            int intType = 0;
            if (type == "takeAwayTable" || type == "dineInTable")
            {
                intType = type == "dineInTable" ? 3203 : 2076;
            }
            var result = intType == 0 ? await _apiRequestService.GetCompletedOrdersAsync() : await _apiRequestService.GetOrdersByTypeAsync(intType);
            if (result == null)
            {
                TempData["Message"] = $"Unable to fetch details of Order: {voucher}.";
                return RedirectToAction("index");
            }
            var order = result != null ? result.Data?.FirstOrDefault(o => o.VoucherCode == voucher) : new CompletedOrders();

            if (companyTin != "0076217301" && companyTin != order?.Tin)
            {
                TempData["Message"] = $"You do not have the necessary permissions to view Order: {voucher}.";
                return RedirectToAction("index");
            }
            var voucherDetail = await _apiRequestService.Gethistorydetail(voucher, order?.CompanyCode.ToString() ?? "");

            var supervisors = await _apiRequestService.GetSupervisorsAsync();
            var supervisor = supervisors?.FirstOrDefault(s => s.UserName == order?.SupervisorPhoneNumber);
            if (order != null)
            {
                order.SupervisorName = $"{supervisor?.FirstName} {supervisor?.SecondName}";
            }
            
            var driverActivity = await _apiRequestService.GetDriverActivityAsync(order?.CompanyCode.ToString() ?? "", voucher);

            // Combine both results into a view model
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

        [HttpPost("savenote")]
        public async Task<IActionResult> SaveOrderReview([FromBody] CompletedOrders request)
        {
            if (request == null)
                return BadRequest("please fill all the required fields.");
            try
            {
                if (!request.IsDelivery)
                {
                    var user = _authenticationManager.GetUserFromCookie(Request);
                    if (user != null)
                    {
                        var supervisors = await _apiRequestService.GetSupervisorsAsync();
                        var supervisor = supervisors.FirstOrDefault(s => s.UserName == user.UserName);
                        if (supervisor != null)
                        {
                            // Prepend supervisor info safely
                            request.Note = $"{{{supervisor.FirstName} {supervisor.SecondName}}} {request.Note}";
                        }
                        else
                        {
                            return BadRequest("Unable to find the supervisor. Please try Again!");
                        }
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
        [HttpGet("getDeliveryActivity")]
        public async Task<IActionResult> GetDeliveryActivity(string voucherCode, string companyCode)
        {
            try
            {
                var response = await _apiRequestService.GetDriverActivityAsync(companyCode, voucherCode);
                if(response == null)
                {
                    return NotFound("Failed to retrieve delivery activity.");
                }
                Response.Headers["Cache-Control"] = "public, max-age=10"; // cache for 5 minutes
                Response.Headers["Vary"] = "Accept-Encoding";
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message[0]);
            }
        }
    }
}
