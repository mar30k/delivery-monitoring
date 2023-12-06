using CNET_V7_Domain.Domain.SecuritySchema;
using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using System.Net.Http;
using Tweetinvi.Core.Models;
using Tweetinvi.Parameters;

namespace DeliveryMonitoring.Controllers
{
    
    public class LoginController : Controller

    {
        private readonly AuthenticationManager _authenticationManager;
        private readonly IHttpClientFactory _httpClientFactory;
        public LoginController(AuthenticationManager authenticationManager,
            IHttpClientFactory httpClientFactory)
        {
            _authenticationManager = authenticationManager;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Login()
        {
            var identificationResult = await _authenticationManager.identificationValid();

            if (identificationResult.isValid)
            {
                return RedirectToAction("Index", "Home");
            }
            return View("Login");

        }


        [HttpPost]
        public async Task<IActionResult> Login(Login model)
        {
            if (ModelState.IsValid)
            {
                var loginResult = await _authenticationManager.AuthenticateUser(model.Username?.Trim(), model.Password);
                if (loginResult.Success)
                {
                    var user = await GetUserByUserName(model.Username?.Trim());
                    _authenticationManager.SignIn(user, model.RememberMe);

                    return RedirectToAction("Index", "Home");

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
        public virtual async Task<UserDTO?> GetUserByUserName(string _userName)
        {
            var _client = _httpClientFactory.CreateClient("DeliveryLogin");

            UserDTO? _loggedInUser;

            var response = await _client.GetAsync(_client.BaseAddress + "/User/filter?userName=" + _userName);
            if (!response.IsSuccessStatusCode)
                return null;

            var juser = await response.Content.ReadAsStringAsync();
            var usernameUser = JsonConvert.DeserializeObject<List<UserDTO>>(juser);

            _loggedInUser = usernameUser != null && usernameUser.Count > 0 ? usernameUser.FirstOrDefault() : null;

            return _loggedInUser;
        }
    }
}
