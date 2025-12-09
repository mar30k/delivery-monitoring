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
                    return NotFound("company not found");

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
            // ----- 1. Model Validation -----
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new HulubejeResponse<object>
                {
                    IsSuccessful = false,
                    ErrorMessages = errors
                });
            }

            // ----- 2. Retrieve Order -----
            var orders = await _apiRequestService.GetOrderRequestsAsync();
            var order = orders?.FirstOrDefault(o => o.VoucherCode == request.VoucherCode);

            if (order == null)
            {
                return NotFound(new HulubejeResponse<object>
                {
                    IsSuccessful = false,
                    ErrorMessages = new List<string> { "Order not found." }
                });
            }

            // ----- 3. Authenticated User -----
            var user = _authenticationManager.GetUserFromCookie();
            if (user == null)
            {
                return Unauthorized(new HulubejeResponse<object>
                {
                    IsSuccessful = false,
                    ErrorMessages = new List<string> { "User not authenticated." }
                });
            }

            // ----- 4. Supervisor Check -----
            var supervisors = await _apiRequestService.GetSupervisorsAsync();
            var supervisor = supervisors.FirstOrDefault(s => s.UserName == user.UserName);

            if (supervisor == null)
            {
                return NotFound(new HulubejeResponse<object>
                {
                    IsSuccessful = false,
                    ErrorMessages = new List<string> { "Unable to find supervisor. Please try again." }
                });
            }

            // ----- 5. Authorization -----
            if (order.SupervisedBy != supervisor.UserName)
            {
                return StatusCode(403, new HulubejeResponse<object>
                {
                    IsSuccessful = false,
                    ErrorMessages = new List<string> { "You are not authorized to change the branch for this order." }
                });
            }

            // ----- 6. Update Order -----
            order.BranchName = request.BranchName?.Trim();
            request.DeliveryOrderRequest = order;

            var updateResult = await _apiRequestService.ChangeOrderBranchAsync(request);
            if (!updateResult.IsSuccessful)
            {
                return BadRequest(new HulubejeResponse<object>
                {
                    IsSuccessful = false,
                    ErrorMessages = updateResult.ErrorMessages ??
                                    new List<string> { "Failed to change branch. Please try again." }
                });
            }

            // ----- 7. SUCCESS -----
            return Ok(new HulubejeResponse<ChangeBranchDTO>
            {
                IsSuccessful = true,
                Data = new ChangeBranchDTO
                {
                    VoucherCode = request.VoucherCode,
                    Remark = request.Remark
                }
            });
        }
    }
}
