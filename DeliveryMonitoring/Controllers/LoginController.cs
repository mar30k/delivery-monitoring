using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryMonitoring.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
