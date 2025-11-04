using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using static NuGet.Packaging.PackagingConstants;

namespace DeliveryMonitoring.Controllers
{
    [Authorize]
    public class CompanyController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IApiRequestService _apiRequestService;
        private readonly AuthenticationManager _authenticationManager;
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
            if(!string.IsNullOrWhiteSpace(currentCompanyTin) && currentCompanyTin != "0076217301")
            {
                var companyDetailsModel = await _apiRequestService.GetCompanyDetailsAsync(currentCompanyTin);
                companyDetailsList.Add(companyDetailsModel ?? new Company());
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
                companyDetailsList.Add(companyDetailsModel ?? new Company());
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
            {
                return RedirectToAction("Logout", "Login");
            }
            else if (currentCompanyTin != "0076217301" && currentCompanyTin != companyTin) { return RedirectToAction("index", "company"); }

            try
            {
                var company = await _apiRequestService.GetCompanyDetailsAsync(companyTin);
                if (company == null )
                {
                    // Return a view indicating that company details are not found
                    return View("Error");
                }

                return View(company);
            }
            catch (HttpRequestException)
            {
                return StatusCode(500); // Handle exception with a 500 Internal Server Error
            }
        }
        
    }
}
