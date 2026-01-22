using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryMonitoring.Controllers
{
    [Authorize]
    [Route("/dashboardReport")]
    public class DashboardReports : Controller
    {
        public DashboardReports()
        {
            
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
