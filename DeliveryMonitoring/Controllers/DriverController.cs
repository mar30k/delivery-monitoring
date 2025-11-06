using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;
using Tweetinvi.Core.Events;

namespace DeliveryMonitoring.Controllers
{
    [Authorize]
    [Route("Driver")]
    public class DriverController : Controller
    {
        private IHttpContextAccessor _httpContextAccessor;
        private readonly IApiRequestService _apiRequestService;
        private readonly AuthenticationManager _authenticationManager;
        private string CompanyTin => _authenticationManager.GetSecureCookie(CNET_WebConstantes.IdentificationCookie) ?? string.Empty;

        public DriverController(
            AuthenticationManager authenticationManager,
            IHttpContextAccessor httpContextAccessor,
            IApiRequestService apiRequestService)
        {
            _httpContextAccessor = httpContextAccessor;
            _apiRequestService = apiRequestService;
            _authenticationManager = authenticationManager;
        }

        //Driver Index Page - starts here
        [HttpGet]
        [Route("/drivers")]
        public async Task<IActionResult> Index(string tin)
        {
            var status = StatusInfo.StatusMap;
            List<Driver> drivers =await _apiRequestService.GetAvailableDriversAsync();
            drivers = drivers
                .OrderBy(d => status.GetValueOrDefault(d.Status?.ToLower() ?? "", status["default"]).Priority)
                .ToList();
            return View(drivers);
        }
        //Driver Index Page - ends here

        //Used for fetching the all driver's location regularly - starts here
        [HttpGet("/Driver/LiveLocation")]
        public async Task<IActionResult> LiveLocation()
        {
            var data = await _apiRequestService.GetAvailableDriversAsync();
            foreach (var item in data ?? new List<Driver>())
            {
                if (item?.UpdatedAt != null)
                {
                    var utc = DateTime.SpecifyKind(item.UpdatedAt.Value, DateTimeKind.Utc);
                    var etTime = new DateTimeOffset(utc).ToOffset(TimeSpan.FromHours(3));
                    item.LastUpdatedAtIso = etTime.ToString("yyyy-MM-dd hh:mm:ss tt");
                    //item.UpdatedAt = etTime.DateTime;
                }
            }

            return Ok(data);
        }

        //Driver Details-- Starts Here
        [HttpGet("/driverdetail/{phoneNumber}")]
        public async Task<IActionResult> Details(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(CompanyTin))
            {
                return RedirectToAction("Logout", "Login");
            }
            // Fetch order data
            List<OrderDetail> orders = new();



            //Fetch driver data
            Driver? driver = new();
            RouteModel? getRoutedata = new();

            try
            {
                orders = await _apiRequestService.GetOrderRequestsAsync();
                driver = await _apiRequestService.GetDriverDetailsByPhoneNumber<Driver>(phoneNumber);
                if (driver!=null)
                {
                    if (CompanyTin != "0076217301" && CompanyTin != driver.CompanyTin)
                    {
                        return NotFound();
                    }
                    var status = driver.Status;
                    var driverOrder = orders?.FirstOrDefault(x => x.AssignedDriverPhoneNumber == phoneNumber);
                    string destLat = "0.0";
                    string destLng = "0.0";

                    if (status == "delivering" || status == "arrivedatbranch")
                    {
                        destLat = driverOrder?.CustomerLat?.ToString() ?? "0.0";
                        destLng = driverOrder?.CustomerLng?.ToString() ?? "0.0";
                    }
                    else if (status == "accepted")
                    {
                        destLat = driverOrder?.TargetBranchLat?.ToString() ?? "0.0";
                        destLng = driverOrder?.TargetBranchLng?.ToString() ?? "0.0";
                    }
                    else if (driverOrder != null)
                    {
                        destLat = driverOrder?.CustomerLat?.ToString() ?? "0.0";
                        destLng = driverOrder?.CustomerLng?.ToString() ?? "0.0";
                    }
                    getRoutedata = await _apiRequestService.GetDriverRouteDetailAsync(driver.Lat?.ToString() ?? "0.0", driver.Lng?.ToString() ?? "0.0", destLat, destLng, "car");
                    var reorderedCoordinates = getRoutedata?.Coordinates
                        ?.Select(coord => new Location { lat = coord[1], lng = coord[0] }) // Swap [lng, lat] → {lat, lng}
                        ?.ToList();

                    if (driver == null)
                    {
                        return NotFound(); // Return a 404 Not Found response if no driver is found.
                    }
                    else
                    {
                        driver.Coordinates = reorderedCoordinates;
                    }
                }
            }

            catch (HttpRequestException)
            {
            }

            ViewData["Orders"] = orders;
            ViewData["Routedata"] = getRoutedata;
            return View(driver);
        }
        //Driver Details -- Ends Here

        //Used for fetching the driver's location regularly - starts here
        [HttpGet("/Driver/LiveLocation/{phoneNumber}")]
        public async Task<IActionResult> LiveLocation(string phoneNumber)
        {
            var driver = new Driver();
            List<OrderDetail> orders = new();
            RouteModel? getRoutedata = new();
            driver = await _apiRequestService.GetDriverDetailsByPhoneNumber<Driver>(phoneNumber);
            if(driver == null)
            {
                return NotFound();
            }
            orders = await _apiRequestService.GetOrderRequestsAsync();
            var status = driver.Status;
            var driverOrder = orders?.FirstOrDefault(x => x.AssignedDriverPhoneNumber == phoneNumber);
            string destLat = "0.0";
            string destLng = "0.0";

            if (status == "delivering" || status == "arrivedatbranch")
            {
                destLat = driverOrder?.CustomerLat?.ToString() ?? "0.0";
                destLng = driverOrder?.CustomerLng?.ToString() ?? "0.0";
            }
            else if (status == "accepted")
            {
                destLat = driverOrder?.TargetBranchLat?.ToString() ?? "0.0";
                destLng = driverOrder?.TargetBranchLng?.ToString() ?? "0.0";
            }
            else if (driverOrder != null)
            {
                destLat = driverOrder?.CustomerLat?.ToString() ?? "0.0";
                destLng = driverOrder?.CustomerLng?.ToString() ?? "0.0";
            }
            getRoutedata = await _apiRequestService.GetDriverRouteDetailAsync(driver.Lat?.ToString() ?? "0.0", driver.Lng?.ToString() ?? "0.0", destLat, destLng, "car");
            var reorderedCoordinates = getRoutedata?.Coordinates
                ?.Select(coord => new Location { lat = coord[1], lng = coord[0] }) // Swap [lng, lat] → {lat, lng}
                ?.ToList();
            driver.Coordinates = reorderedCoordinates;
            return Ok(driver);
        }
        //Used for fetching the driver's location regularly - ends here

        //Driver Update Page - starts here
        [HttpGet("/updatedriverinfo/{phoneNumber}")]
        public async Task<IActionResult> Update(string phoneNumber)
        {
            UpdateDriverModel? driver;
            try
            {
                driver = await _apiRequestService.GetDriverDetailsByPhoneNumber<UpdateDriverModel>(phoneNumber);
            }

            catch (HttpRequestException)
            {
                return StatusCode(500); // Handle exception with a 500 Internal Server Error
            }

            return View(driver);
        }


        [HttpPatch("/Driver/Update/{phoneNumber}")]
        public async Task<IActionResult> Update(string phoneNumber, [FromBody] UpdateDriverModel updateModel)
        {
            if (updateModel == null)
                return BadRequest("Invalid driver data.");
            try
            {
               var response = await _apiRequestService.UpdateDriverDetailsAsync(updateModel, phoneNumber);
                if(!response.IsSuccessful)
                    return StatusCode(500, "Failed to update driver: " + string.Join(", ", response.ErrorMessages ?? new List<string>()));
                return Ok("Driver updated successfully.");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, "Failed to update driver" + ex.Message);
            }
        }
        [HttpGet("getDrivers")]
        public async Task<IActionResult> GetAvailableDrivers()
        {
            var drivers = await _apiRequestService.GetAvailableDriversAsync();
            return Ok(drivers);
        }

        [HttpGet("/Review/{phoneNumber}")]
        public async Task<IActionResult> DriverReview(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 10)
            {
                ViewBag.Error = "Invalid phone number.";
                return View("review", null);
            }

            string trimmedPhone = phoneNumber[1..]; // Remove first character
            int page = 1;

            try
            {
                // Fetch driver first
                var driver = await _apiRequestService.GetDriverDetailsByPhoneNumber<Driver>(phoneNumber);

                if (driver == null)
                {
                    ViewBag.Error = "Driver not found.";
                    return View("review", null);
                }

                // Fetch reviews only if driver exists
                var allReviews = await _apiRequestService.GetDriverReviewsAsync(trimmedPhone, page);
                if (allReviews != null && allReviews.Data!=null)
                    allReviews.Data.DriveInfo = driver;
                if (allReviews?.Data?.Reviews?.Count == 0)
                {
                    ViewBag.Error = "No reviews found for this driver.";
                    return View("review", null);
                }

                // Pass data to view
                return View("review", allReviews?.Data);
            }
            catch
            {
                ViewBag.Error = "Error loading reviews. Please try again later.";
                return View("review", null);
            }
        }

        [HttpGet("/Driver/fetchReview")]
        public async Task<IActionResult> FetchReview([FromQuery] string phoneNumber, [FromQuery] int page)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 2)
                return BadRequest("Invalid phone number.");

            string trimmedPhone = phoneNumber[1..]; // Remove first character
            var result = await _apiRequestService.GetDriverReviewsAsync(trimmedPhone, page);

            if (result?.Data?.Reviews?.Count == 0)
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}
