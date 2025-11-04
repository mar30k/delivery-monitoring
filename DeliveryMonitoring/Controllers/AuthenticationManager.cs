using CNET_V7_Domain.Domain.SecuritySchema;
using CNET_V7_Domain.Misc;
using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services;
using MediaBrowser.Model.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
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
        private readonly IApiRequestService _apiRequestService;
        private UserDTO _cachedUser;
        private readonly IDataProtector _protector;
        private readonly ISecureCookieService _cookieService;

        public AuthenticationManager(
            IDataProtectionProvider dataProtectionProvider,
                IHttpContextAccessor httpContextAccessor,
                IApiRequestService apiRequestService,
                ISecureCookieService cookieService)
        {
            _httpContextAccessor = httpContextAccessor;
            _apiRequestService = apiRequestService;
            _protector = dataProtectionProvider.CreateProtector("DeliveryMonitoring.Cookies");
            _cookieService = cookieService;
        }


        public async Task<ResponseModel<LoginResponse>> AuthenticateUser(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                string message = string.IsNullOrWhiteSpace(userName)
                    ? (string.IsNullOrWhiteSpace(password) ? "Username and password are required." : "Username is required.")
                    : "Password is required.";

                return new ResponseModel<LoginResponse>
                {
                    Success = false,
                    Message = message,
                    Data = new LoginResponse()
                };
            }
            else
            {
                return  await _apiRequestService.AuthenticateUser(userName, password);
            }
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
            DeleteCookie(CNET_WebConstantes.IdentificationCookie);
            DeleteCookie(CNET_WebConstantes.ApiBaseAddress);
            DeleteCookie(CNET_WebConstantes.UserInfo);
            var user = _httpContextAccessor.HttpContext?.User;
            _ = await _apiRequestService.UpdateSupervisorsOnlineStatusAsync(isOnline: false, phoneNumber: user?.Identity?.Name);
        }

        public UserDTO? GetUserFromCookie()
        {
            // Retrieve the secure cookie value
            var cookieValue = GetSecureCookie(CNET_WebConstantes.UserInfo);

            if (string.IsNullOrWhiteSpace(cookieValue))
                return null;

            try
            {
                // Deserialize the JSON into UserDTO
                return JsonConvert.DeserializeObject<UserDTO>(cookieValue);
            }
            catch (JsonException ex)
            {
                // TODO: replace with proper logging
                Console.WriteLine($"Failed to deserialize user cookie: {ex.Message}");
                return null;
            }
        }
        public void AddSecureCookie(string key, string value, TimeSpan expiry)
        => _cookieService.SetCookie(key, value, expiry);

        public string? GetSecureCookie(string key)
            => _cookieService.GetCookie(key);

        public void DeleteCookie(string key)
            => _cookieService.DeleteCookie(key);
    }
}