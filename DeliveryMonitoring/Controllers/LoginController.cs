using CNET_ERP_V7.WebConstants;
using CNET_V7_Domain.Domain.SecuritySchema;
using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using System.Net.Http;
using System.Web;
using Tweetinvi.Core.Models;
using Tweetinvi.Parameters;

namespace DeliveryMonitoring.Controllers
{

    public class LoginController : Controller

    {
        private readonly IWebHostEnvironment _appEnvironment;
        private readonly AuthenticationManager _authenticationManager;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public LoginController(AuthenticationManager authenticationManager,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor, IWebHostEnvironment webHostEnvironment)
        {
            _authenticationManager = authenticationManager;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _appEnvironment = webHostEnvironment;
        }
        [Route("/login")]
        public async Task<IActionResult> index() 
        {
            var identificationResult = await _authenticationManager.identificationValid();

            if (identificationResult.isValid)
            {
                return RedirectToAction("Index", "Home");
            }
            return View("index");
        }
        [Route("Login/Login")]
        public async Task<IActionResult> Login() {
            var identificationResult = await _authenticationManager.identificationValid();

            if (identificationResult.isValid)
            {
                return RedirectToAction("Index", "Home"); 
            }
            return View("Login"); 
        }

        [HttpPost]
        [Route("Login/Authenticate")]
        public async Task<IActionResult> Login(Login model)
        {
            if (ModelState.IsValid)
            {
                var loginResult = await _authenticationManager.AuthenticateUser(model.Username?.Trim(), model.Password);
                if (loginResult.Success)
                {
                    var user = await GetUserByUserName(model.Username?.Trim());
                    var status = await _authenticationManager.OnlineStatus(true, user.UserName.ToString());
                    if (!status)
                    {
                        ModelState.AddModelError("", "Unable to update online status! Please try again.");
                        return View("Login", model); 
                    }

                    _authenticationManager.SignIn(user, model.RememberMe);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", loginResult.Message);
                }
            }

            return View("Login", model);
        }
        public async Task<IActionResult> Logout()
        {
            _authenticationManager.SignOut();
            return RedirectToAction("index","Login");
        }
        public virtual async Task<UserDTO?> GetUserByUserName(string _userName)
        {
            var apibaseAddress = _httpContextAccessor.HttpContext?.Request.Cookies["apibaseAddress"];
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];

            var _client = new HttpClient
            {
                BaseAddress = new Uri(apibaseAddress)
            };
            UserDTO? _loggedInUser;

            var response = await _client.GetAsync(_client.BaseAddress + "/User/filter?userName=" + _userName);
            if (!response.IsSuccessStatusCode)
                return null;

            var juser = await response.Content.ReadAsStringAsync();
            var usernameUser = JsonConvert.DeserializeObject<List<UserDTO>>(juser);

            _loggedInUser = usernameUser != null && usernameUser.Count > 0 ? usernameUser.FirstOrDefault() : null;

            return _loggedInUser;
        }



        [HttpPost]
        public async Task<IActionResult> checkMyId([FromBody] VerifyIdModel model)
        {
            var _client = _httpClientFactory.CreateClient("DeliveryLogin");
            if (ModelState.IsValid)
            {
                string message = string.Empty;
                string baseAddress;
                if (model.myId?.Trim().ToLower() == "0076217301")
                {
                    baseAddress = _client.BaseAddress.ToString();
                    AddCookie(CNET_WebConstantes.IdentificationCookie, "0076217301", TimeSpan.FromMinutes(CNET_WebConstantes.IdentificationCookieLifeTime));
                    AddCookie("apibaseAddress", baseAddress, TimeSpan.FromMinutes(CNET_WebConstantes.IdentificationCookieLifeTime));
                    return Json(new
                    {
                        d = true,
                        m = "Verified successfuly"
                    });
                }
                else
                {

                    string requestUrl = $"/Consignee/filter?Tin={model.myId}";

                    HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + requestUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string juservalidation = await response.Content.ReadAsStringAsync();
                        List<EntityModel>? userValidation = JsonConvert.DeserializeObject<List<EntityModel>>(juservalidation);
                        if (userValidation?.Count > 0)
                        {
                            if (_appEnvironment.IsDevelopment())
                            {
                                baseAddress = "http://196.191.244.156:7038/api";  // dev
                            }
                            else
                            {
                                baseAddress = userValidation.FirstOrDefault()?.BaseUrl + "/api";  // prod
                            }
                            AddCookie(CNET_WebConstantes.IdentificationCookie, userValidation?.FirstOrDefault()?.Tin, TimeSpan.FromMinutes(CNET_WebConstantes.IdentificationCookieDailyLifeTime));
                            AddCookie("apibaseAddress", baseAddress, TimeSpan.FromMinutes(CNET_WebConstantes.IdentificationCookieDailyLifeTime));
                            if (userValidation.FirstOrDefault()?.Tin == model.myId?.Trim())
                            {
                                return Json(new
                                {
                                    d = true,
                                    m = "Verified successfuly"
                                });
                            }
                            message = "Invalid identification no.";
                        }
                        else
                        {
                            message = "Bad Request at Api.";
                        }
                    }


                }
                return Json(new
                {
                    d = false,
                    m = message
                });
            }

            return View("Index", model);
        }

        public void AddCookie(string key, string value, TimeSpan expiry)
        {
            var options = new CookieOptions
            {
                Expires = DateTimeOffset.Now.Add(expiry),
                HttpOnly = true,
                Secure = true, // set to false if not using HTTPS
                SameSite = SameSiteMode.Strict
            };
            Response.Cookies.Append(key, value, options);
        }
    }
}