using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using Tweetinvi.Core.Models;
using static NuGet.Packaging.PackagingConstants;

namespace DeliveryMonitoring.Controllers
{
    [Authorize]
    public class CompanyController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IApiRequestService _apiRequestService;
        private readonly AuthenticationManager _authenticationManager;
        private const string AdminCompanyTin = AppConstants.Company.AdminTin;
        public CompanyController(
            AuthenticationManager authenticationManager,
            IApiRequestService apiRequestService,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _apiRequestService = apiRequestService;
            _authenticationManager = authenticationManager;
        }

        [Route("companies")]
        public async Task<IActionResult> Index()
        {
            var currentCompanyTin = _authenticationManager.GetSecureCookie(
                CNET_WebConstantes.IdentificationCookie);
            if (string.IsNullOrWhiteSpace(currentCompanyTin) || string.IsNullOrWhiteSpace(currentCompanyTin))
            {
                return RedirectToAction("Logout", "Login");
            }
            var companiesModel = await _apiRequestService.GetCompaniesAsync();
            // Call the second endpoint for each company TIN to get detailed information
            var companyDetailsList = new List<Company>();
            if(!string.IsNullOrWhiteSpace(currentCompanyTin) && currentCompanyTin != AdminCompanyTin)
            {
                var companyDetailsModel = await _apiRequestService.GetCompanyDetailsAsync(currentCompanyTin);
                companyDetailsList.Add(companyDetailsModel?.Data ?? new Company());
                if (companiesModel != null)
                {
                    companiesModel.companyTins = new List<string> { currentCompanyTin };
                }

                return View(new CompanyIndex
                {
                    Companies = companiesModel ?? new Companies(),
                    Company = companyDetailsList
                });
            }
            foreach (var companyTin in companiesModel?.companyTins ?? new List<string>())
            {
                var companyDetailsModel = await _apiRequestService.GetCompanyDetailsAsync(companyTin);
                companyDetailsList.Add(companyDetailsModel?.Data ?? new Company());
            }

            // Create the CompanyIndex view model
            var viewModel = new CompanyIndex
            {
                Companies = companiesModel ?? new Companies(),
                Company = companyDetailsList
            };

            return View(viewModel);
        }

        
        [HttpGet("/Company/{companyTin}")]
        public async Task<IActionResult> Details(string companyTin)
        {
            var currentCompanyTin = _authenticationManager.GetSecureCookie(CNET_WebConstantes.IdentificationCookie);

            if (string.IsNullOrWhiteSpace(currentCompanyTin))
                return RedirectToAction("Logout", "Login");

            if (currentCompanyTin != AdminCompanyTin && currentCompanyTin != companyTin)
                return RedirectToAction("Index", "Company");
            try
            {
                var companyResponse = await _apiRequestService.GetCompanyDetailsAsync(companyTin);
                if (companyResponse !=null && !companyResponse.IsSuccessful && companyResponse?.Data == null )
                {
                    // Return a view indicating that company details are not found
                    return View("Error");
                }

                return View(companyResponse?.Data);
            }
            catch (HttpRequestException)
            {
                return StatusCode(500); // Handle exception with a 500 Internal Server Error
            }
        }
        [HttpGet("/getCompanyBranches")]
        public async Task<IActionResult> GetCompanyBranches([FromQuery] string tin)
        {
            if (string.IsNullOrWhiteSpace(tin))
                return BadRequest("tin is required.");

            try
            {
                var companyResponse = await _apiRequestService.GetCompanyDetailsAsync(tin);

                if (companyResponse == null)
                    return NotFound();

                return Ok(companyResponse);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpPost("/changeBranch")]
        public async Task<IActionResult> ChangeBranch([FromBody] ChangeBranchDTO request)
        {
            // Validate input
            if (request == null)
                return BadRequest(new { success = false, message = "Request cannot be null." });

            if (string.IsNullOrWhiteSpace(request.VoucherCode))
                return BadRequest(new { success = false, message = "Voucher code is required." });

            if (string.IsNullOrWhiteSpace(request.BranchCode?.ToString()))
                return BadRequest(new { success = false, message = "Branch code is required." });

            if (string.IsNullOrWhiteSpace(request.BranchName))
                return BadRequest(new { success = false, message = "Branch name is required." });

            // Get the order
            var orders = await _apiRequestService.GetOrderRequestsAsync();
            var order = orders?.FirstOrDefault(o => o.VoucherCode == request.VoucherCode);
            if (order == null)
                return NotFound(new { success = false, message = "Order not found." });

            // Get authenticated user
            var user = _authenticationManager.GetUserFromCookie();
            if (user == null)
                return Unauthorized(new { success = false, message = "User not authenticated." });

            // Get supervisor
            var supervisors = await _apiRequestService.GetSupervisorsAsync();
            var supervisor = supervisors.FirstOrDefault(s => s.UserName == user.UserName);

            if (supervisor == null)
                return BadRequest(new { success = true, message = "Unable to find supervisor. Please try again." });

            // Authorization check
            if (order.SupervisedBy != supervisor.UserName)
                return BadRequest(new { success = false, message = "You are not authorized to change the branch for this order." });

            // Update branch info
            order.BranchName = request.BranchName.Trim();
            request.DeliveryOrderRequest = order;

            // Call service to persist the change
            var updateResult = await _apiRequestService.ChangeOrderBranchAsync(request); // Use proper naming
            if (!updateResult.IsSuccessful)
            {
                var errorMessage = updateResult.ErrorMessages != null && updateResult.ErrorMessages.Count > 0
                    ? string.Join("; ", updateResult.ErrorMessages)
                    : "Failed to change branch. Please try again.";
                return BadRequest(new { success = false, message = errorMessage });
            }

            // Return success
            return Ok(new
            {
                success = true,
                message = $"Branch '{request.BranchName}' changed successfully!",
                voucherCode = request.VoucherCode,
                remark = request.Remark
            });
        }


    }
}
