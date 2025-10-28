using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DeliveryMonitoring.Services
{
    public class ApiRequestService : IApiRequestService
    {
        private readonly HttpClient _client;
        private readonly HttpClient _deliveryClient;

        public ApiRequestService(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("CnetApiBaseUrl");
            _deliveryClient = httpClientFactory.CreateClient("Delivery");
        }

        public async Task<string> GetCompletedOrdersRawAsync()
        {
            var response = await _client.GetAsync("voucher/getcompletedorders");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                return data;
            }
            return null;
        }
        public async Task<string> GetDeliveryPurposeRawAsync()
        {
            var response = await _client.GetAsync("delivery/getpurpose");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                return data;
            }
            return null;
        }

        public async Task<string> GetOrdersByTypeRawAsync(int type)
        {
            var response = await _client.GetAsync($"voucher/getordersbytype?type={type}");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                return data;
            }
            return null;
        }
    }
}