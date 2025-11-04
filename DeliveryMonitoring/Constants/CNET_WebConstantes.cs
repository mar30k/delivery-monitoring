namespace DeliveryMonitoring.Constants
{
    public static class CNET_WebConstantes
    {
        public static string ClaimsIssuer => "DeliveryMonitorSystem";
        public static string CookieScheme => "delivery.monitor.system";
        public static string IdentificationCookie => "delivery.monitor.system.userId";
        public static string ApiBaseAddress => "delivery.monitor.system.api.baseAddress";
        public static string UserInfo => "delivery.monitor.system.user.info";

        public const int IdentificationCookieLifeTime = 10080;

        public const int IdentificationCookieDailyLifeTime = 1440;
    }
}
