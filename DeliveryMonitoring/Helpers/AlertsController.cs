using CNET_ERP_V7.WebConstants;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace DeliveryMonitoring.Helpers
{
    public class AlertsController : Controller
    {
        private IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IApiRequestService _apiRequestService;
        public AlertsController(IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            IApiRequestService apiRequestService)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _apiRequestService = apiRequestService;
        }
        [Route("/GetOrders")]
        public async Task<List<OrderDetail>?> Index()
        {
            try
            {
                var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];

                if (string.IsNullOrWhiteSpace(companyTin)) { return new List<OrderDetail>(); }

                var response = await _apiRequestService.GetOrderRequestsAsync();
                if(response.Count>0)
                    response?.ForEach(x => x.CreatedAtString = new DateTimeOffset(DateTime.SpecifyKind(x.CreatedAt.Value, DateTimeKind.Utc))
                                    .ToOffset(TimeSpan.FromHours(3))
                                    .ToString("yyyy-MM-dd HH:mm:ss"));
                if (!string.IsNullOrWhiteSpace(companyTin) && companyTin != "0076217301" && response!=null)
                {
                    response = response.Where(order => order.DeliveryTin == companyTin).ToList();
                }
                return response ?? new List<OrderDetail>();
            }
            catch
            {
                return new List<OrderDetail>();
            }
        }
    }
}
