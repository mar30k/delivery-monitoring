using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DeliveryMonitoring.Controllers
{
    [Authorize]
    public class SupervisorsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public SupervisorsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        [Route("/supervisors")]
        public async Task<IActionResult>Index()
        {
            var client = _httpClientFactory.CreateClient("CnetApiBaseUrl");
            List<SupervisorsDTO> superVisors = new();
            var supervisorTask = client.GetAsync($"{client.BaseAddress}auth/getsupervisors");           
            if (supervisorTask.Result.IsSuccessStatusCode)
            {
                var data = await supervisorTask.Result.Content.ReadAsStringAsync();
                superVisors = JsonConvert.DeserializeObject<List<SupervisorsDTO>>(data) ?? new List<SupervisorsDTO>();
            }
            return View(superVisors);
        }
    }
}
