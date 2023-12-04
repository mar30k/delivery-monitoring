using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryMonitoring.Controllers
{
    public class LoginController : Controller

    {
        private readonly AuthenticationManager _authenticationManager;

        public LoginController(AuthenticationManager authenticationManager)
        {
            _authenticationManager = authenticationManager;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Login model, string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                var loginResult = await _authenticationManager.AuthenticateUser(model.Username?.Trim(), model.Password);
                if (loginResult.Success)
                {

                    _authenticationManager.SignIn(loginResult, model.RememberMe);
                    if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                    {
                        return RedirectToAction("Login", "Login");
                    }
                    return Redirect(returnUrl);

                }
                else
                {
                    ModelState.AddModelError("", "Incorrect Username or Password!");
                }
            }

            return View("Login", model);
        }

        public async Task<IActionResult> Logout()
        {
            _authenticationManager.SignOut();
            return RedirectToAction("Login");
        }
    }
}
