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

        /// <summary>
        /// Gets the authenticated supervisor based on the current user.
        /// </summary>
        public static async Task<HulubejeResponse<SupervisorsDTO>> GetAuthenticatedSupervisorAsync(
            AuthenticationManager authManager,
            IApiRequestService apiService)
        {
            var response = new HulubejeResponse<SupervisorsDTO>();

            var user = authManager.GetUserFromCookie();
            if (user == null)
            {
                response.IsSuccessful = false;
                response.ErrorMessages = new List<string> { "User not authenticated." };
                return response;
            }

            var supervisors = await apiService.GetSupervisorsAsync();
            var supervisor = supervisors.FirstOrDefault(s => s.UserName == user.UserName);

            if (supervisor == null)
            {
                response.IsSuccessful = false;
                response.ErrorMessages = new List<string> { "Unable to find the supervisor. Please try again!" };
                return response;
            }

            response.IsSuccessful = true;
            response.Data = supervisor;
            return response;
        }


        /// <summary>
        /// Builds the supervisor note for a completed order.
        /// </summary>
        public static string BuildNote(CompletedOrders order, SupervisorsDTO supervisor)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            if (supervisor == null) throw new ArgumentNullException(nameof(supervisor));

            if (order.IsDelivery)
                return order.Note ?? "";

            return $"{supervisor.FirstName} {order.Note ?? ""}";
        }

        /// <summary>
        /// Convenience method: gets authenticated supervisor and builds the note.
        /// </summary>
        public static async Task<HulubejeResponse<string>> BuildSupervisorNoteAsync(
            CompletedOrders order,
            AuthenticationManager authManager,
            IApiRequestService apiService)
        {
            var supervisorResponse = await GetAuthenticatedSupervisorAsync(authManager, apiService);
            if (!supervisorResponse.IsSuccessful || supervisorResponse.Data == null)
            {
                return new HulubejeResponse<string>
                {
                    IsSuccessful = false,
                    Data = order.Note ?? "",
                    ErrorMessages = supervisorResponse.ErrorMessages
                };
            }

            return new HulubejeResponse<string>
            {
                IsSuccessful = true,
                Data = BuildNote(order, supervisorResponse.Data)
            };
        }
    }
}
