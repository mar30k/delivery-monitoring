using DeliveryMonitoring.Controllers;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services.Api;
using System.Text.RegularExpressions;

namespace DeliveryMonitoring.Helpers
{
    public static class OrderHelpers
    {
        public static void PrepareOrderDisplayValues(CompletedOrders order)
        {
            order.RequestCreatedAtString = order.RequestCreatedAt.ToString("yyyy-MM-dd hh:mm tt");
            order.EtaDifference = order.Eta - order.Duration;
            string supervisorName = order.SupervisorName ?? "N/A";
            if (!string.IsNullOrEmpty(order.Note) && order.Note.StartsWith("{"))
            {
                var match = Regex.Match(order.Note, @"^\{(.*?)\}");
                if (match.Success)
                {
                    supervisorName = match.Groups[1].Value;
                    order.Note = order.Note[match.Length..].TrimStart();
                }
            }
            order.SupervisorName = supervisorName;
        }

        public static List<CompletedOrders> FilterOrders(List<CompletedOrders> orders, DateTime? startDate, DateTime? endDate, bool isClear, string companyTin, string adminCompanyTin)
        {
            if (companyTin != adminCompanyTin)
                orders = orders.Where(o => o.Tin == companyTin).ToList();

            if (isClear)
                return orders;

            if ( startDate.HasValue && endDate.HasValue)
                orders = orders.Where(o => o.RequestCreatedAt.Date >= startDate.Value.Date &&
                                           o.RequestCreatedAt.Date <= endDate.Value.Date).ToList();

            return orders;
        }

        public static bool IsTodayIncluded(DateTime? startDate, DateTime? endDate)
        {
            var today = DateTime.UtcNow.Date;

            if (!startDate.HasValue && !endDate.HasValue)
                return true;

            if (startDate.HasValue && endDate.HasValue)
                return startDate.Value.Date <= today && endDate.Value.Date >= today;

            if (startDate.HasValue)
                return startDate.Value.Date <= today;

            if (endDate.HasValue)
                return endDate.Value.Date >= today;

            return false;
        }

        public static async Task<string> BuildSupervisorNoteAsync(
            CompletedOrders order,
            AuthenticationManager authManager,
            IApiRequestService apiService)
        {
            

            var user = authManager.GetUserFromCookie() ?? throw new InvalidOperationException("User not authenticated.");
            var supervisors = await apiService.GetSupervisorsAsync();
            var supervisor = supervisors.FirstOrDefault(s => s.UserName == user.UserName) 
                ?? throw new InvalidOperationException("Unable to find the supervisor. Please try again!");

            if (order.IsDelivery)
                return order.Note ?? "";

            return $"{{{supervisor.FirstName} {supervisor.SecondName}}} {order.Note ?? ""}";
        }
    }
}
