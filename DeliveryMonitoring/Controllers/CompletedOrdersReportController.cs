using Bogus.DataSets;
using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Helpers;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace DeliveryMonitoring.Controllers
{
    [Authorize]
    public class CompletedOrdersReportController : Controller
    {
        private IHttpContextAccessor _httpContextAccessor;
        private readonly IApiRequestService _apiRequest;
        private readonly AuthenticationManager _authenticationManager;
        private string CompanyTin =>
        _authenticationManager.GetSecureCookie(CNET_WebConstantes.IdentificationCookie) ?? string.Empty;
        public CompletedOrdersReportController(IHttpContextAccessor httpContextAccessor, AuthenticationManager authenticationManager, IApiRequestService apiRequest)
        {
            _httpContextAccessor = httpContextAccessor;
            _apiRequest = apiRequest;
            _authenticationManager = authenticationManager;
        }
        [Route("report/{type?}")]
        public IActionResult Index(ReportByOrderType type = ReportByOrderType.All)
        {
            var config = TableConfigFactory.CreateReport(type);
            return View(config);
        }
    }
}
