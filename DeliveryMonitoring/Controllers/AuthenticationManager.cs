using System.Security.Claims;
using CNET_ERP_V7.WebConstants;
using CNET_V7_Domain.Domain.SecuritySchema;
using CNET_V7_Domain.Misc;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using NuGet.Protocol.Plugins;

namespace DeliveryMonitoring.Controllers
{
    public class AuthenticationManager
    {
        private IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
 
        private UserDTO _cachedUser;
        public AuthenticationManager(
                IHttpContextAccessor httpContextAccessor,
                IHttpClientFactory httpClientFactory
                )
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient("DeliveryLogin");
        }


        public async Task<ResponseModel<LoginResponse>> AuthenticateUser(string userName, string password)
        {
            var _s = new ResponseModel<LoginResponse>();
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                //return AuthenticationErrorType.IncorrectUserNamePassword;
                return _s;
            }
            else
            {

                HttpResponseMessage response = await _httpClient.GetAsync(_httpClient.BaseAddress);
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

        //public virtual async Task<UserDTO?> GetAuthenticatedUser()
        //{
        //    if (_cachedUser != null)
        //        return _cachedUser;

        //    var authenticateResult = await _httpContextAccessor.HttpContext.AuthenticateAsync(CNET_WebConstantes.CookieScheme);
        //    if (!authenticateResult.Succeeded)
        //        return null;

        //    UserDTO? user = null;

        //    //try to get customer by username
        //    var usernameClaim = authenticateResult.Principal.FindFirst(claim => claim.Type == ClaimTypes.Name
        //        && claim.Issuer.Equals(CNET_WebConstantes.ClaimsIssuer, StringComparison.InvariantCultureIgnoreCase));
        //    if (usernameClaim != null)
        //    {
        //        user = await _sharedHelpers.GetUserByUserName(usernameClaim.Value) ?? null;
        //    }

        //    //whether the found user is available
        //    if (user == null)
        //        return null;

        //    //cache authenticated customer
        //    _cachedUser = user;

        //    return _cachedUser;
        //}

        public virtual async void SignOut()
        {
            //reset cached customer
            _cachedUser = null;
            //and sign out from the current authentication scheme
            await _httpContextAccessor.HttpContext.SignOutAsync(CNET_WebConstantes.CookieScheme);
        }
    }
}
