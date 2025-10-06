using Newtonsoft.Json;

namespace DeliveryMonitoring.Services
{
    public interface ICookieService
    {
        T? GetCookie<T>(string key) where T : class;
    }

    public class CookieService : ICookieService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CookieService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public T? GetCookie<T>(string key) where T : class
        {
            var cookieValue = _httpContextAccessor.HttpContext?.Request.Cookies[key];
            if (string.IsNullOrEmpty(cookieValue))
                return null;

            try
            {
                return JsonConvert.DeserializeObject<T>(cookieValue);
            }
            catch
            {
                // optionally log deserialization errors
                return null;
            }
        }
    }
}
