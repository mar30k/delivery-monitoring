using System.Security.Claims;
using CNET_ERP_V7.WebConstants;
using CNET_V7_Domain.Domain.SecuritySchema;
using CNET_V7_Domain.Misc;
using DeliveryMonitoring.Models;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using NuGet.Protocol.Plugins;


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
            var _client = _httpClientFactory.CreateClient("DeliveryLogin");
            var _s = new ResponseModel<LoginResponse>();
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                //return AuthenticationErrorType.IncorrectUserNamePassword;
                return _s;
            }
            else
            {
                var endpoint = "/SysInitialize/authenticate";
                string queryString = $"?userName={userName}&password={password}&tin=0076217301";
                string requestUrl = $"{endpoint}{queryString}";

                HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + requestUrl);

                string juservalidation = await response.Content.ReadAsStringAsync();
                var userValidation = JsonConvert.DeserializeObject<ResponseModel<LoginResponse>>(juservalidation);

                if (!response.IsSuccessStatusCode)
                    return userValidation;

                if (userValidation.Success)
                    return userValidation;
                else
                    return userValidation;
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
            //and sign out from the current authentication scheme
            await _httpContextAccessor.HttpContext.SignOutAsync(CNET_WebConstantes.CookieScheme);
        }
    }
}