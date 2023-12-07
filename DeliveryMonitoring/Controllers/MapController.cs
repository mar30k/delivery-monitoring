using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace DeliveryMonitoring.Controllers
{
    [Authorize]
    public class MapController: Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public MapController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetMapData()
        {
            // Replace YOUR_GOOGLE_MAPS_API_KEY with your actual API key
            string apiKey = "AIzaSyDihZpSLFD2uyptHT-UQDSfJm9BKHRK-VU";
            string apiUrl = "https://maps.googleapis.com/maps/api/js?key=" + apiKey + "&callback=initMap&libraries=places,geometry&v=weekly";

            using (var client = _httpClientFactory.CreateClient())
            {
                var result = await client.GetStringAsync(apiUrl);
                return Content(result, "application/javascript");
            }
        }
    }
}
