using Newtonsoft.Json;
using System.Text;

namespace DeliveryMonitoring.Services
{
    public interface IApiRequestService
    {
        Task<string> MakeApiRequestAsync(string endpoint, object payload);
    }
    public class ApiRequestService :IApiRequestService
    {
        private readonly HttpClient _httpClient;

        public ApiRequestService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> MakeApiRequestAsync(string endpoint, object payload)
        {
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
} 
