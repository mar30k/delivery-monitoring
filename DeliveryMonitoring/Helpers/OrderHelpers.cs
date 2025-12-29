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
            IApiRequestService apiService,
            string? assignedSupervisor = "")
        {
            var user = authManager.GetUserFromCookie();
            if (user == null)
                return new HulubejeResponse<SupervisorsDTO>
                {
                    IsSuccessful = false,
                    ErrorMessages = new List<string> { "User not authenticated." }
                };

            var supervisor = (await apiService.GetSupervisorsAsync())
                             .FirstOrDefault(s => s.UserName == user.UserName);

            if (supervisor == null)
                return new HulubejeResponse<SupervisorsDTO>
                {
                    IsSuccessful = false,
                    ErrorMessages = new List<string> { "Unable to find the supervisor. Please try again!" }
                };

            if (!string.IsNullOrWhiteSpace(assignedSupervisor) && assignedSupervisor != supervisor?.UserName)
            {
                return new HulubejeResponse<SupervisorsDTO>
                {
                    IsSuccessful = false,
                    ErrorMessages = new List<string> { "You are not the assigned supervisor for this delivery." }
                };
            }

            return new HulubejeResponse<SupervisorsDTO>
            {
                IsSuccessful = true,
                Data = supervisor
            };
        }



        /// <summary>
        /// Builds the supervisor note for a completed order.
        /// </summary>
        public static HulubejeResponse<string>BuildNote(SaveNoteRequest order, SupervisorsDTO supervisor)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            if (supervisor == null) throw new ArgumentNullException(nameof(supervisor));

            if (order.IsDelivery && supervisor.UserName != order.SupervisorPhoneNumber)
            {
                return new HulubejeResponse<string>
                {
                    IsSuccessful = false,
                    ErrorMessages = new List<string> { "You are not the assigned supervisor for this order." }
                };
            }

            if (string.IsNullOrWhiteSpace(order.Note))
            {
                return new HulubejeResponse<string>
                {
                    IsSuccessful = false,
                    ErrorMessages = new List<string>
                    {
                        "Supervisor note cannot be empty."
                    }
                };
            }

            var note = order.IsDelivery
                ? order.Note.Trim()
                : $"{supervisor.FirstName} {order.Note.Trim()}";

            return new HulubejeResponse<string>
            {
                IsSuccessful = true,
                Data = note
            };
        }

        /// <summary>
        /// Convenience method: gets authenticated supervisor and builds the note.
        /// </summary>
        public static async Task<HulubejeResponse<string>> BuildSupervisorNoteAsync(
            SaveNoteRequest order,
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
            return BuildNote(order, supervisorResponse.Data);
        }

        public static async Task<HulubejeResponse<bool>> ValidatePendingOrderCompletionAsync(
            OrderCompletionRequest request,
            AuthenticationManager authManager,
            IApiRequestService apiService)
        {
            // Supervisor authentication & authorization
            var supervisorResponse = await GetAuthenticatedSupervisorAsync(
                authManager,
                apiService,
                assignedSupervisor: request.SupervisorPhone
            );

            if (!supervisorResponse.IsSuccessful)
                return HulubejeResponse<bool>.Fail(supervisorResponse.ErrorMessages ?? new List<string>());

            // Order must not be active in delivery
            var activeOrders = await apiService.GetOrderRequestsAsync();
            if (activeOrders.Any(o => o.VoucherCode == request.VoucherCode))
            {
                return HulubejeResponse<bool>.Fail(
                    "Order is still in delivery."
                );
            }

            return HulubejeResponse<bool>.Success(true);
        }
        public static async Task<HulubejeResponse<SaveNoteRequest>> ValidateAndBuildNoteAsync(
            SaveNoteRequest request,
            AuthenticationManager authManager,
            IApiRequestService apiService)
        {
            var noteResponse = await BuildSupervisorNoteAsync(
                request,
                authManager,
                apiService
            );

            if (!noteResponse.IsSuccessful)
            {
                return HulubejeResponse<SaveNoteRequest>.Fail(
                    noteResponse.ErrorMessages ?? new List<string> { "Invalid note." }
                );
            }

            request.Note = noteResponse.Data!;
            return HulubejeResponse<SaveNoteRequest>.Success(request);
        }

    }
}
