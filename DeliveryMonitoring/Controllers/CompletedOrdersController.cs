using CNET_ERP_V7.WebConstants;
using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace DeliveryMonitoring.Controllers
{
    [Authorize]
    [Route("/CompletedOrders")]
    public class CompletedOrdersController : Controller
    {
        private IHttpClientFactory _httpClientFactory;
        private IHttpContextAccessor _httpContextAccessor;
        public CompletedOrdersController(IHttpClientFactory httpClientFactory , IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IActionResult> Index()
        {
            var _client = _httpClientFactory.CreateClient("CnetApiBaseUrl");
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];
            try
            {
                var completedResult = await FetchCompletedOrders(_client, "voucher/getcompletedorders");

                var purposeResponse = await _client.GetAsync("delivery/getpurpose");
                if (!purposeResponse.IsSuccessStatusCode)
                {
                    var errorContent = await purposeResponse.Content.ReadAsStringAsync();
                    ViewBag.ErrorMessage = $"Failed to retrieve completed orders. Server responded with: {errorContent}";
                }

                
                var purposeResponseData = await purposeResponse.Content.ReadAsStringAsync();
                var purposeResult = JsonConvert.DeserializeObject<Dictionary<int, string>>(purposeResponseData);
                var dineIneResult = await FetchCompletedOrders(_client, "voucher/getordersbytype?type=3203");
                var takeAwayResult = await FetchCompletedOrders(_client, "voucher/getordersbytype?type=2076");
                    
                if (companyTin != "0076217301")
                {
                    if (completedResult != null)
                        completedResult.Data = completedResult.Data?.Where(order => order.Tin == companyTin).ToList();

                    if (dineIneResult != null)
                        dineIneResult.Data = dineIneResult.Data?.Where(order => order.Tin == companyTin).ToList();

                    if (takeAwayResult != null)
                        takeAwayResult.Data = takeAwayResult.Data?.Where(order => order.Tin == companyTin).ToList();      
                    foreach(var order in completedResult?.Data ?? new List<CompletedOrders>())
                    {
                        order.SupervisorName = null;
                        order.SupervisorPhoneNumber = null;
                    }
                }

                var CompletedOrdersViewModel = new CompletedOrdersViewModel
                {
                    CompletedOrders = completedResult,
                    DineInOders = dineIneResult,
                    TakeAwayOrders = takeAwayResult,
                    PurposeOptions = purposeResult,
                    CompanyTin = companyTin
                };
                return View(CompletedOrdersViewModel); 
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Unable to connect to the service. Please try again later.";
                return View(null);
            }
        }

        [HttpGet("/getCompletedOrders")]
        public async Task<IActionResult> GetCompletedOrdersApi()
        {
            var client = _httpClientFactory.CreateClient("CnetApiBaseUrl");
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];

            var result = await FetchCompletedOrders(client, "voucher/getcompletedorders");
            if (result == null || result.Data == null)
            {
                return NotFound("Failed to retrieve or parse completed orders.");
            }

            if (companyTin != "0076217301")
            {
                result.Data = result.Data.Where(order => order.Tin == companyTin).ToList();
            }
            foreach (var item in result.Data)
            {
                item.RequestCreatedAtString = item.RequestCreatedAt.ToString("yyyy-MM-dd hh:mm:ss");
            }
            return Ok(result);
        }

        [HttpGet("/getordersbytype")]
        public async Task<IActionResult> GetOrdersByType(string type)
        {
            var client = _httpClientFactory.CreateClient("CnetApiBaseUrl");
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];

            var getordersbytypeData = await FetchCompletedOrders(client, $"voucher/getordersbytype?type={type}");

            if (getordersbytypeData == null || getordersbytypeData.Data == null)
            {
                return NotFound("Failed to retrieve or parse completed orders.");
            }

            if (companyTin != "0076217301")
            {
                getordersbytypeData.Data = getordersbytypeData.Data.Where(order => order.Tin == companyTin).ToList();
            }
            foreach (var item in getordersbytypeData.Data)
            {
                item.RequestCreatedAtString = item.RequestCreatedAt.ToString("yyyy-MM-dd hh:mm:ss");
            }
            return Ok(getordersbytypeData);
        }
        [Route("/orderdetail")]
        public async Task<IActionResult> CompletedOrderDetail(string voucher ,string type = "")
        {
            var client = _httpClientFactory.CreateClient("CnetApiBaseUrl");
            var supervisors = new List<SupervisorsDTO>();
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];
            string url = "voucher/getcompletedorders";
            if (type == "dineInTable")
            {
                url = "voucher/getordersbytype?type=3203";
            }
            else if (type == "takeAwayTable")
            {
                url = "voucher/getordersbytype?type=2076";
            }
            var result = await FetchCompletedOrders(client, url);
            if (result == null)
            {
                TempData["Message"] = $"Unable to fetch details of Order: {voucher}.";
                return RedirectToAction("index");
            }
            CompletedOrders? order = result != null ? result.Data?.FirstOrDefault(o => o.VoucherCode == voucher) : new CompletedOrders();

            if (companyTin != "0076217301" && companyTin != order?.Tin)
            {
                TempData["Message"] = $"You do not have the necessary permissions to view Order: {voucher}.";
                return RedirectToAction("index");
            }
            var voucherResponse = await client.GetAsync($"voucher/gethistorydetail?voucherCode={voucher}&companyCode={order?.CompanyCode}&industryType=1992");
            if (!voucherResponse.IsSuccessStatusCode)
            {
                var errorContent = await voucherResponse.Content.ReadAsStringAsync();
                ViewBag.ErrorMessage = $"Failed to retrieve completed orders. Server responded with: {errorContent}";
            }

            var voucherContent = await voucherResponse.Content.ReadAsStringAsync();
            var voucherDetail = JsonConvert.DeserializeObject<HulubejeResponse<LineItemsDetail>>(voucherContent)
                                ?? new HulubejeResponse<LineItemsDetail>();

            // 2. Fetch driver activity
            var driverResponse = await client.GetAsync($"driveractivity/get?voucherCode={voucher}&companyCode={order?.CompanyCode}");
            if (!driverResponse.IsSuccessStatusCode)
            {
                var errorContent = await driverResponse.Content.ReadAsStringAsync();
                ViewBag.ErrorMessage = $"Failed to retrieve completed orders. Server responded with: {errorContent}";
            }

            var getsupervisors = await client.GetAsync(client.BaseAddress + "auth/getsupervisors");
            if (getsupervisors.IsSuccessStatusCode)
            {
                string Supervisordata = await getsupervisors.Content.ReadAsStringAsync();
                supervisors = JsonConvert.DeserializeObject<List<SupervisorsDTO>>(Supervisordata);
                var supervisor = supervisors?.FirstOrDefault(s => s.UserName == order?.SupervisorPhoneNumber);
                if (order != null)
                {
                    order.SupervisorName = $"{supervisor?.FirstName} {supervisor?.SecondName}";
                }
            }
            var driverContent = await driverResponse.Content.ReadAsStringAsync();
            var driverActivity = JsonConvert.DeserializeObject<HulubejeResponse<Activities>>(driverContent)
                                 ?? new HulubejeResponse<Activities>();

            // Combine both results into a view model
            var viewModel = new OrderDetail
            {
                CustomerFirstName = order?.FirstName,
                BranchName = order?.BranchName,
                SupervisedBy = order?.SupervisorPhoneNumber,
                SupervisorName = order?.SupervisorName,
                AssignedDriverPhoneNumber = order?.DriverPhoneNumber,
                LineItemsDetail = voucherDetail.Data,
                Activities = driverActivity.Data,
                VoucherCode = voucher
            };

            return View(viewModel);
        }


        private static async Task<HulubejeResponse<List<CompletedOrders>>?> FetchCompletedOrders(HttpClient client, string url)
        {
            try
            {
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<HulubejeResponse<List<CompletedOrders>>>(responseData) ?? new HulubejeResponse<List<CompletedOrders>>();
            }
            catch (Exception)
            {
                return null;
            }
        }

        [HttpPost("savenote")]
        public async Task<IActionResult> SaveOrderReview([FromBody] CompletedOrders request)
        {
            var client = _httpClientFactory.CreateClient("CnetApiBaseUrl");

            try
            {
                var response = await client.GetAsync($"delivery/savenote?voucherCode={request.VoucherCode}&note={request.Note}&purpose={request.Purpose}" );

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return BadRequest(errorContent);
                }

                var responseData = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Boolean>(responseData);

                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message[0]);
            }
        }
        [HttpGet("getDeliveryActivity")]
        public async Task<IActionResult> GetDeliveryActivity(string voucherCode, string companyCode)
        {
            var client = _httpClientFactory.CreateClient("CnetApiBaseUrl");

            try
            {
                var response = await client.GetAsync($"driveractivity/get?companyCode={companyCode}&voucherCode={voucherCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return BadRequest(errorContent);
                }

                var responseData = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<HulubejeResponse<Activities>>(responseData);
                Response.Headers["Cache-Control"] = "public, max-age=10"; // cache for 5 minutes
                Response.Headers["Vary"] = "Accept-Encoding";
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message[0]);
            }
        }
    }
}
