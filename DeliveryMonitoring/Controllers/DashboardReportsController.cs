using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryMonitoring.Controllers
{
    [Authorize]
    [Route("/dashboardReport")]
    public class DashboardReportsController : Controller
    {
        public DashboardReportsController()
        {
            
        }
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("Index");
        }

        // Embedded / standalone dashboard (no layout, custom scrolling)
        [AllowAnonymous]
        [HttpGet("embed")]
        public IActionResult Embed()
        {
            return View("Index.Embed");
        }
    }
}
