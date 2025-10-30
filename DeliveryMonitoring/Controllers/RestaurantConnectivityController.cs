using CNET_ERP_V7.WebConstants;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IApiRequestService _apiRequestService;

        public RestaurantConnectivityController(
            IHttpContextAccessor httpContextAccessor,
            IApiRequestService apiRequestService)
        {
            _httpContextAccessor = httpContextAccessor;
            _apiRequestService = apiRequestService;
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
            date ??= DateTime.Now.ToString("yyyy-MM-dd");
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];
            var deviceControl = await _apiRequestService.GetDeviceControlAsync(date);
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

            // Filter out items with Note starting with "09"
            var result = latestByTinAndBranch?
                .Where(s => string.IsNullOrEmpty(s.Note) || !s.Note.StartsWith("09"))
                .ToList();
            return result;
        }
    }
}
