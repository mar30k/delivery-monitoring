using CNET_ERP_V7.WebConstants;
using CNET_V7_Domain.Domain.SecuritySchema;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services;
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
        private readonly IApiRequestService _apiRequestService;
        private readonly IWebHostEnvironment _appEnvironment;
        private readonly AuthenticationManager _authenticationManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        public LoginController(AuthenticationManager authenticationManager,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration,
            IApiRequestService apiRequestService)
        {
            _authenticationManager = authenticationManager;
            _httpContextAccessor = httpContextAccessor;
            _appEnvironment = webHostEnvironment;
            _configuration = configuration;
            _apiRequestService = apiRequestService;
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
                    var user = await _apiRequestService.GetUserByUserName(model.Username?.Trim());
                    var status = await _apiRequestService.UpdateSupervisorsOnlineStatusAsync(isOnline: true, phoneNumber: user.UserName.ToString());
                    if (!status.IsSuccessful && !status.Data)
                    {
                        ModelState.AddModelError("", "Unable to update online status! Please try again.");
                        return View("Login", model); 
                    }
                    AddCookie("user", JsonConvert.SerializeObject(user), TimeSpan.FromMinutes(CNET_WebConstantes.IdentificationCookieLifeTime));
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
        public IActionResult Logout()
        {
            _authenticationManager.SignOut();
            return RedirectToAction("index","Login");
        }        

        [HttpPost]
        public async Task<IActionResult> CheckMyId([FromBody] VerifyIdModel model)
        {
            if (ModelState.IsValid)
            {
                string baseAddress;
                string message;
                if (model.myId?.Trim().ToLower() == "0076217301")
                {
                    baseAddress = _configuration["DeliveryLogin"];
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
                    var userValidation = await _apiRequestService.GetFilteredConsigneesAsync(tin: model.myId?.Trim());
                    if (userValidation?.Count > 0)
                    {
                        if (_appEnvironment.IsDevelopment())
                        {
                            baseAddress = "http://196.191.244.156:7038/api/";  // dev
                        }
                        else
                        {
                            baseAddress = userValidation.FirstOrDefault()?.BaseUrl + "/api/";  // prod
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