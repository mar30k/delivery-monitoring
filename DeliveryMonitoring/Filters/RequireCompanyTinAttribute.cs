using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DeliveryMonitoring.Filters
{
    public class RequireCompanyTinFilter : IActionFilter
    {
        private readonly AuthenticationManager _authenticationManager;

        public RequireCompanyTinFilter(AuthenticationManager authenticationManager)
        {
            _authenticationManager = authenticationManager;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var companyTin = _authenticationManager.GetSecureCookie(CNET_WebConstantes.IdentificationCookie);
            if (string.IsNullOrEmpty(companyTin))
            {
                // Redirect to login
                context.Result = new RedirectToRouteResult("verifyId", null);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
    // This is the attribute you actually use on controllers/actions
    public class RequireCompanyTinAttribute : TypeFilterAttribute
    {
        public RequireCompanyTinAttribute() : base(typeof(RequireCompanyTinFilter))
        {
        }
    }
}
