using CNET_ERP_V7.WebConstants;
using CNET_V7_Domain.Domain.SecuritySchema;
using CNET_V7_Domain.Misc;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services;
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
        private readonly IApiRequestService _apiRequestService;
        private UserDTO _cachedUser;
        public AuthenticationManager(
                IHttpContextAccessor httpContextAccessor,
                IApiRequestService apiRequestService)
        {
            _httpContextAccessor = httpContextAccessor;
            _apiRequestService = apiRequestService;
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
            context.Response.Cookies.Delete(CNET_WebConstantes.IdentificationCookie);
            context.Response.Cookies.Delete("apibaseAddress");
            var user = _httpContextAccessor.HttpContext?.User;
            _ = await _apiRequestService.UpdateSupervisorsOnlineStatusAsync(isOnline: false, phoneNumber: user?.Identity?.Name);
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