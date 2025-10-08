using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryMonitoring.Services
{
    public interface IApiRequestService
    {
        Task<T?> GetAsync<T>(string endpoint);
        Task<T?> PostAsync<T>(string endpoint, object payload);
        Task<T?> PutAsync<T>(string endpoint, object payload);
        Task<bool> DeleteAsync(string endpoint);
    }

    public class ApiRequestService : IApiRequestService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ApiRequestService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateClient()
        {
            // Uses your named client: "CnetApiBaseUrl"
            return _httpClientFactory.CreateClient("CnetApiBaseUrl");
        }

        // --------------------------
        //  GET
        // --------------------------
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            var client = CreateClient();
            var response = await client.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }

        // --------------------------
        //  POST (Create)
        // --------------------------
        public async Task<T?> PostAsync<T>(string endpoint, object payload)
        {
            var client = CreateClient();
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(result);
        }

        // --------------------------
        //  PUT (Update)
        // --------------------------
        public async Task<T?> PutAsync<T>(string endpoint, object payload)
        {
            var client = CreateClient();
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(result);
        }

        // --------------------------
        //  DELETE
        // --------------------------
        public async Task<bool> DeleteAsync(string endpoint)
        {
            var client = CreateClient();
            var response = await client.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }
    }
}
