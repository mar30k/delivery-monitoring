using CNET_ERP_V7.WebConstants;
using CNET_V7_Domain.Domain.SecuritySchema;
using CNET_V7_Domain.Misc;
using DeliveryMonitoring.Models;
using MediaBrowser.Model.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Protocol.Plugins;
using System.Security.Claims;
using Tweetinvi.Core.Models;


namespace DeliveryMonitoring.Controllers
{
    public class AuthenticationManager
    {
        private IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;

        private UserDTO _cachedUser;
        public AuthenticationManager(
                IHttpContextAccessor httpContextAccessor,
                IHttpClientFactory httpClientFactory
                )
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClientFactory = httpClientFactory;
        }


        public async Task<ResponseModel<LoginResponse>> AuthenticateUser(string userName, string password)
        {
            var apibaseAddress = _httpContextAccessor.HttpContext?.Request.Cookies["apibaseAddress"];
            var companyTin = _httpContextAccessor.HttpContext?.Request.Cookies[CNET_WebConstantes.IdentificationCookie];

            var _client = new HttpClient
            {
                BaseAddress = new Uri(apibaseAddress)
            };
            var _s = new ResponseModel<LoginResponse>();
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                //return AuthenticationErrorType.IncorrectUserNamePassword;
                return _s;
            }
            else
            {
                var endpoint = "/SysInitialize/authenticate";
                string queryString = $"?userName={userName}&password={password}&tin={companyTin}";
                string requestUrl = $"{endpoint}{queryString}";

                HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + requestUrl);

                string responseBody = await response.Content.ReadAsStringAsync();
                var userValidation = JsonConvert.DeserializeObject<ResponseModel<LoginResponse>>(responseBody);

                if (!response.IsSuccessStatusCode && userValidation!=null)
                {
                    return new ResponseModel<LoginResponse>
                    {
                        Success = false,
                        Message = userValidation.Message,
                        Data = new LoginResponse()
                    };
                }

                if (userValidation == null)
                {
                    return new ResponseModel<LoginResponse>
                    {
                        Success = false,
                        Message = "Empty or invalid response",
                        Data = new LoginResponse()
                    };
                }

                return userValidation;
            }
        }
        public async Task<List<EntityModel>> CheckMyId()
        {
            var _client = _httpClientFactory.CreateClient("DeliveryLogin");
            string requestUrl = $"/Consignee/filter?gslType=1";

            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + requestUrl);
            var userValidation = new List<EntityModel>();

            if (response.IsSuccessStatusCode)
            {
                string juservalidation = await response.Content.ReadAsStringAsync();
                userValidation = JsonConvert.DeserializeObject<List<EntityModel>>(juservalidation);
            }
            return userValidation ?? new List<EntityModel>();
        }

        public virtual async void SignIn(UserDTO user, bool isPersistent)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            //create claims for customer's username and email
            var claims = new List<Claim>();

            if (!string.IsNullOrEmpty(user.UserName))
                claims.Add(new Claim(ClaimTypes.Name, user.UserName, ClaimValueTypes.String, CNET_WebConstantes.ClaimsIssuer));


            //create principal for the current authentication scheme
            var userIdentity = new ClaimsIdentity(claims, CNET_WebConstantes.CookieScheme);
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            //set value indicating whether session is persisted and the time at which the authentication was issued
            var authenticationProperties = new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                IssuedUtc = DateTime.UtcNow
            };

            //sign in
            await _httpContextAccessor.HttpContext.SignInAsync(CNET_WebConstantes.CookieScheme, userPrincipal, authenticationProperties);

            //cache authenticated customer
            _cachedUser = user;
        }
        public virtual async Task<cookieValidation> identificationValid()
        {
            var validinfo = new cookieValidation();

            var authenticateResult = await _httpContextAccessor.HttpContext.AuthenticateAsync();

            if (authenticateResult.Succeeded)
            {
                var authenticationProperties = authenticateResult.Properties;

                if (authenticationProperties?.IsPersistent == true)
                {
                    validinfo.isValid = true;  // User is authenticated and the authentication is persistent
                    return validinfo;
                }
            }

            validinfo.isValid = false;
            return validinfo;
        }

        public virtual async void SignOut()
        {
            //reset cached customer
            _cachedUser = null;
            var context = _httpContextAccessor.HttpContext;
            //and sign out from the current authentication scheme
            await context.SignOutAsync(CNET_WebConstantes.CookieScheme);
            context.Response.Cookies.Delete(CNET_WebConstantes.IdentificationCookie);
            context.Response.Cookies.Delete("apibaseAddress");
            var user = _httpContextAccessor.HttpContext?.User;
            _ = await OnlineStatus(false, user?.Identity?.Name);
        }

        public async Task<bool> OnlineStatus(bool isOnline, string phoneNumber)
        {
            var client = _httpClientFactory.CreateClient("CnetApiBaseUrl");

            try
            {
                var response = await client.GetAsync($"auth/status?code={phoneNumber}&online={isOnline}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                }

                var responseData = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Boolean>(responseData);
                return result;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public UserDTO? GetUserFromCookie(HttpRequest request)
        {
            if (request.Cookies.TryGetValue("user", out string? cookieValue) &&
                !string.IsNullOrWhiteSpace(cookieValue))
            {
                try
                {
                    return JsonConvert.DeserializeObject<UserDTO>(cookieValue);
                }
                catch (JsonException ex)
                {
                    // Log or handle malformed cookie
                    Console.WriteLine($"Error parsing user cookie: {ex.Message}");
                }
            }

            return null;
        }
    }
}