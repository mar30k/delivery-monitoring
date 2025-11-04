using Microsoft.AspNetCore.DataProtection;

namespace DeliveryMonitoring.Services
{
    public class SecureCookieService : ISecureCookieService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDataProtector _protector;

        public SecureCookieService(IHttpContextAccessor httpContextAccessor, IDataProtectionProvider dataProtectionProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _protector = dataProtectionProvider.CreateProtector("DeliveryMonitoring.Cookies");
        }

        public void SetCookie(string key, string value, TimeSpan expiry)
        {
            if (string.IsNullOrEmpty(value)) return;

            var protectedValue = _protector.Protect(value);
            var options = new CookieOptions
            {
                Expires = DateTimeOffset.Now.Add(expiry),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append(key, protectedValue, options);
        }

        public string? GetCookie(string key)
        {
            if (_httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(key, out var protectedValue))
            {
                try
                {
                    return _protector.Unprotect(protectedValue);
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public void DeleteCookie(string key)
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(key);
        }

    }

}
