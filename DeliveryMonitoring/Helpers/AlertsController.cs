using CNET_ERP_V7.WebConstants;
using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace DeliveryMonitoring.Helpers
{
    public class AlertsController : Controller
    {
        private IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;
        public AlertsController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }
        [Route("/GetOrders")]
        public async Task<List<OrderDetail>?> Index()
        {
            try
            {
                var response = new List<OrderDetail>();
                var _client = _httpClientFactory.CreateClient("Delivery");
                var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];

                if (string.IsNullOrWhiteSpace(companyTin)) { return new List<OrderDetail>(); }

                HttpResponseMessage responseMessage = await _client.GetAsync(_client.BaseAddress + $"/orderRequests?companyTin={companyTin}");
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseMessageData = await responseMessage.Content.ReadAsStringAsync();
                    response = JsonConvert.DeserializeObject<List<OrderDetail>>(responseMessageData);
                }
                response?.ForEach(x => x.CreatedAtString = new DateTimeOffset(DateTime.SpecifyKind(x.CreatedAt.Value, DateTimeKind.Utc))
                                    .ToOffset(TimeSpan.FromHours(3))
                                    .ToString("yyyy-MM-dd HH:mm:ss"));
                return response ?? new List<OrderDetail>();
            }
            catch
            {
                return new List<OrderDetail>();
            }
        }
    }
}
