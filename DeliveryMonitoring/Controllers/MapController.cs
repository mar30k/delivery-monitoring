using DeliveryMonitoring.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace DeliveryMonitoring.Controllers
{
    [Authorize]
    public class MapController: Controller
    {
        
        private readonly IApiRequestService _apiRequestService;
        public MapController(IApiRequestService apiRequestService)
        {
            _apiRequestService = apiRequestService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMapData()
        {
            var jsContent = await _apiRequestService.GetGoogleMapsJsAsync();
            return Content(jsContent, "application/javascript");
        }
    }
}
