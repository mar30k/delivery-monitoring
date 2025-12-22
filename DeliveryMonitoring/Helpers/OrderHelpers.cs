using DeliveryMonitoring.Controllers;
using DeliveryMonitoring.Models;
using DeliveryMonitoring.Services.Api;
using System.Text.RegularExpressions;

namespace DeliveryMonitoring.Helpers
{
    public static class OrderHelpers
    {
        public static void PrepareDisplayValues(CompletedOrders order)
        {
            order.RequestCreatedAtString = order.RequestCreatedAt.ToString("yyyy-MM-dd hh:mm tt");
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

        public static List<CompletedOrders> FilterOrders(List<CompletedOrders> orders, OrderQueryParams @params, string companyTin, string adminCompanyTin)
        {
            if (companyTin != adminCompanyTin)
                orders = orders.Where(o => o.Tin == companyTin).ToList();

            if (@params.IsClear)
                return orders;

            if (@params.StartDate.HasValue && @params.EndDate.HasValue)
                orders = orders.Where(o => o.RequestCreatedAt.Date >= @params.StartDate.Value.Date &&
                                           o.RequestCreatedAt.Date <= @params.EndDate.Value.Date).ToList();

            return orders;
        }

        public static bool IsTodayIncluded(OrderQueryParams @params)
        {
            var today = DateTime.UtcNow.Date;

            if (!@params.StartDate.HasValue && !@params.EndDate.HasValue)
                return true;

            if (@params.StartDate.HasValue && @params.EndDate.HasValue)
                return @params.StartDate.Value.Date <= today && @params.EndDate.Value.Date >= today;

            if (@params.StartDate.HasValue)
                return @params.StartDate.Value.Date <= today;

            if (@params.EndDate.HasValue)
                return @params.EndDate.Value.Date >= today;

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
