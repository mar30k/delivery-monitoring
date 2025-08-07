using CNET_ERP_V7.WebConstants;
using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;

namespace DeliveryMonitoring.Controllers
{
    [Route("/deviceControl")]
    [Authorize]
    public class RestaurantConnectivityController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RestaurantConnectivityController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }
        [Route("/deviceControl")]
        public async Task<IActionResult> Index(string date)
        {

            try
            {
                var deviceControl = (await GetDeviceControlByDate(date))?.ToList().Where(s => !s.Note.StartsWith("09")).ToList();
                return View(deviceControl);
            }
            catch (Exception ex)
            {
                return View(null);
            }
        }

        [HttpGet("/getDeviceControl")]
        public async Task<List<DeviceControl>?> GetDeviceControlByDate(string date)
        {
            if (date == null) date = DateTime.Now.ToString("yyyy-MM-dd");
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];
            var client = _httpClientFactory.CreateClient("ApiBaseUrl");

            var response = await client.GetAsync($"deviceControl?StartDate={date}&EndDate={date}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                // Log error if necessary
                return null;
            }

            var responseData = await response.Content.ReadAsStringAsync();
            var deviceControl = JsonConvert.DeserializeObject<List<DeviceControl>>(responseData);
            var latestByTinAndBranch = deviceControl?
                .Where(d => d.TimeStamp.HasValue) // Ensure TimeStamp is not null
                .GroupBy(d => new { d.Tin, d.BranchName, d.DeviceName }) // Group by Tin , BranchName and DeviceName
                .Select(g => g.OrderByDescending(d => d.TimeStamp).First()) // Get the one with latest TimeStamp
                .ToList();
            if (companyTin != "0076217301")
            {
                latestByTinAndBranch = latestByTinAndBranch?
                    .Where(x => x?.Tin?.ToString() == companyTin?.Trim())
                    .ToList();
            }

            return latestByTinAndBranch;
        }
    }
}
